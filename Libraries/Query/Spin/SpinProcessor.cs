using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Inference;
using VDS.RDF.Query.Spin.Constraints;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.Progress;
using VDS.RDF.Query.Spin.Statistics;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Spin
{
    // Even though the class is exposed as a Processor, the features of this class are more related to those of a Reasoner.
    // TODO refactor the initialization process
    /// <summary>
    /// 
    /// </summary>
    public class SpinProcessor //: IInferenceEngine
    {

        // TODO allow for an OWL Reasoner
        private IInferenceEngine _reasoner = new StaticRdfsReasoner();

        internal InMemoryDataset _spinConfiguration;

        private IGraph _currentSparqlGraph = new ThreadSafeGraph();

        #region "SPIN processor Initialisation"

        public SpinProcessor()
        {
            _spinConfiguration = new InMemoryDataset(true);

            // Ensure that SP, SPIN and SPL are present
            IRdfReader rdfReader = new RdfXmlParser();
            Initialise(UriFactory.Create(SP.BASE_URI), rdfReader);
            Initialise(UriFactory.Create(SPIN.BASE_URI), rdfReader);
            Initialise(UriFactory.Create(SPL.BASE_URI), rdfReader);
        }

        public void Initialise(Uri spinGraphUri, IRdfReader rdfReader = null)
        {
            if (_spinConfiguration.GraphUris.Contains(spinGraphUri))
            {
                return;
            }
            Initialise(SPINImports.GetInstance().getImportedGraph(spinGraphUri, rdfReader));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spinGraph"></param>
        /// <returns></returns>
        public IGraph Initialise(IGraph spinGraph)
        {
            if (_spinConfiguration.GraphUris.Contains(spinGraph.BaseUri))
            {
                _spinConfiguration.RemoveGraph(spinGraph.BaseUri);
            }
            String ontQuery = "CONSTRUCT { ?graphUri a <" + SPIN.ClassLibraryOntology + ">} WHERE { {?s (<" + SPIN.PropertyImports + ">|<" + OWL.PropertyImports + ">) ?graphUri} UNION {?graphUri a <" + SPIN.ClassLibraryOntology + ">} }";
            IGraph imports = (Graph)spinGraph.ExecuteQuery(ontQuery);

            // Explore for subsequent imports
            foreach (Triple t in imports.GetTriplesWithPredicateObject(RDF.PropertyType, SPIN.ClassLibraryOntology))
            {
                Uri importUri = ((IUriNode)t.Subject).Uri;
                if (!_spinConfiguration.GraphUris.Contains(importUri) && !RDFUtil.sameTerm(importUri, spinGraph.BaseUri))
                {
                    Initialise(importUri);
                }
            }
            _spinConfiguration.AddGraph(spinGraph);
            _reasoner.Initialise(spinGraph);

            IGraph inferenceGraph = ApplyInference(spinGraph);
            _spinConfiguration.AddGraph(inferenceGraph);
            _reasoner.Initialise(inferenceGraph);

            return inferenceGraph;
        }

        #endregion

        #region "Spin model utilities"

        internal IEnumerable<IResource> GetAllInstances(INode cls)
        {
            List<Resource> classList = GetTriplesWithPredicateObject(RDF.PropertyType, cls).Select(t => Resource.Get(t.Subject, this)).ToList();
            return classList;
        }

        internal IEnumerable<IResource> GetAllSubClasses(INode root, bool includeRoot = false)
        {
            List<Resource> classList = GetTriplesWithPredicateObject(RDFS.PropertySubClassOf, root).Select(t => Resource.Get(t.Subject, this)).ToList();
            if (includeRoot)
            {
                classList.Add(Resource.Get(root, this));
            }
            classList.Sort(delegate(Resource x, Resource y)
            {
                if (_spinConfiguration.ContainsTriple(new Triple(x.getSource(), RDFS.PropertySubClassOf, y.getSource())))
                {
                    return 1;
                }
                return -1;
            });
            return classList;
        }

        internal IEnumerable<IResource> GetAllSuperClasses(INode root, bool includeRoot = false)
        {
            List<Resource> classList = GetTriplesWithSubjectPredicate(root, RDFS.PropertySubClassOf).Select(t => Resource.Get(t.Object, this)).ToList();
            if (includeRoot)
            {
                classList.Add(Resource.Get(root, this));
            }
            classList.Sort(delegate(Resource x, Resource y)
            {
                if (_spinConfiguration.ContainsTriple(new Triple(x.getSource(), RDFS.PropertySubClassOf, y.getSource())))
                {
                    return 1;
                }
                return -1;
            });
            return classList;
        }

        internal IEnumerable<IResource> GetAllSubProperties(INode root, bool includeRoot = false)
        {
            List<Resource> propertyList = GetTriplesWithPredicateObject(RDFS.PropertySubPropertyOf, root).Select(t => Resource.Get(t.Subject, this)).ToList();
            if (includeRoot)
            {
                propertyList.Add(Resource.Get(root, this));
            }
            propertyList.Sort(delegate(Resource x, Resource y)
            {
                if (_spinConfiguration.ContainsTriple(new Triple(x.getSource(), RDFS.PropertySubPropertyOf, y.getSource())))
                {
                    return 1;
                }
                return -1;
            });
            return propertyList;
        }

        internal IEnumerable<IResource> GetAllSuperProperties(INode root, bool includeRoot = false)
        {
            List<Resource> propertyList = GetTriplesWithSubjectPredicate(root, RDFS.PropertySubPropertyOf).Select(t => Resource.Get(t.Object, this)).ToList();
            if (includeRoot)
            {
                propertyList.Add(Resource.Get(root, this));
            }
            propertyList.Sort(delegate(Resource x, Resource y)
            {
                if (_spinConfiguration.ContainsTriple(new Triple(x.getSource(), RDFS.PropertySubPropertyOf, y.getSource())))
                {
                    return 1;
                }
                return -1;
            });
            return propertyList;
        }

        private INode GetSourceNode(INode resource)
        {
            if (resource is IResource)
            {
                return ((IResource)resource).getSource();
            }
            return resource;
        }

        internal IResource CreateResource(INode rdfType = null)
        {
            // TODO put that in another graph to not overflow the configuration with temporary requests ?
            INode resource = _currentSparqlGraph.CreateBlankNode();
            if (rdfType != null)
            {
                _currentSparqlGraph.Assert(resource, Tools.CopyNode(RDF.PropertyType, _currentSparqlGraph), Tools.CopyNode(rdfType, _currentSparqlGraph));
            }
            return Resource.Get(resource, this);
        }

        internal IResource CreateList(IResource[] elements)
        {
            return CreateList((IEnumerator<IResource>)elements.GetEnumerator());
        }

        internal IResource CreateList(IEnumerator<IResource> elements)
        {
            IResource first = CreateResource();
            IResource root = first;
            if (!elements.MoveNext())
            {
                first.AddProperty(RDF.PropertyFirst, RDF.Nil);
                return first;
            }
            do
            {
                first.AddProperty(RDF.PropertyFirst, elements.Current);
                IResource rest = CreateResource();
                first.AddProperty(RDF.PropertyRest, rest);
                first = rest;
            } while (elements.MoveNext());
            first.AddProperty(RDF.PropertyRest, RDF.Nil);
            return root;
        }


        internal IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            return _spinConfiguration.GetTriplesWithSubject(GetSourceNode(subj));
        }

        internal IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            return _spinConfiguration.GetTriplesWithPredicate(GetSourceNode(pred));
        }

        internal IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            return _spinConfiguration.GetTriplesWithObject(GetSourceNode(obj));
        }

        internal IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return _spinConfiguration.GetTriplesWithPredicateObject(GetSourceNode(pred), GetSourceNode(obj));
        }

        internal IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return _spinConfiguration.GetTriplesWithSubjectPredicate(GetSourceNode(subj), GetSourceNode(pred));
        }

        internal IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return _spinConfiguration.GetTriplesWithSubjectObject(GetSourceNode(subj), GetSourceNode(obj));
        }

        internal bool ContainsTriple(INode subj, INode pred, INode obj)
        {
            return ContainsTriple(new Triple(Tools.CopyNode(GetSourceNode(subj), _currentSparqlGraph), Tools.CopyNode(GetSourceNode(pred), _currentSparqlGraph), Tools.CopyNode(GetSourceNode(obj), _currentSparqlGraph)));
        }

        internal bool ContainsTriple(Triple t)
        {
            return _spinConfiguration.ContainsTriple(t);
        }

        #endregion

        #region Dataset utilities

        private Dictionary<IGraph, IGraph> _inferenceGraphs = new Dictionary<IGraph, IGraph>();

        internal IGraph ApplyInference(IGraph g)
        {
            IGraph inferedTriples;
            if (_inferenceGraphs.ContainsKey(g))
            {
                inferedTriples = _inferenceGraphs[g];
                inferedTriples.Clear();
            }
            else
            {
                inferedTriples = new ThreadSafeGraph();
            }
            _reasoner.Apply(g, inferedTriples);
            _inferenceGraphs[g] = inferedTriples;
            return inferedTriples;
        }

        /// <summary>
        /// The default implementation applies all rules then checks all constraints and returns wether the results raised constraints violations or not.
        /// Allow any subclass to define how SPIN processing would be applied when a dataset is flushed. 
        /// TODO this should be complemented with an ExecuteScript method that would allow to exploit SPARQLMotion scripts in the model.
        /// </summary>
        /// <param name="dataset">The Dataset to apply SPIN processing on</param>
        /// <param name="resources">A list of resources to check constraints on</param>
        /// <returns>true if no constraint violation is raised, false otherwise</returns>
        public bool Apply(SpinWrappedDataset dataset, IEnumerable<INode> resources)
        {
            dataset.RestrictSPINProcessingTo(resources);
            return ApplyInternal(dataset);
        }

        // TODO perhaps use the null uri to avoid creating a graph with all rdftype triples ?
        public bool Apply(SpinWrappedDataset dataset)
        {
            dataset.RestrictSPINProcessingTo(null);
            return ApplyInternal(dataset);
        }

        // TODO perhaps use the null uri to avoid creating a graph with all rdftype triples ?
        internal protected virtual bool ApplyInternal(SpinWrappedDataset dataset)
        {
            dataset.QueryExecutionMode = SpinWrappedDataset.QueryMode.SpinInferencing;

            dataset.QueryExecutionMode = SpinWrappedDataset.QueryMode.SpinConstraintsChecking;
            IEnumerable<ConstraintViolation> vios = new List<ConstraintViolation>(); //runConstraints(dataset, resources, null);
            dataset.QueryExecutionMode = SpinWrappedDataset.QueryMode.UserQuerying;
            return vios.Count() == 0;
        }

        #endregion

        #region Constraints checking API

        /// <summary>
        /// Checks all spin:constraints for a given Resource set.
        /// </summary>
        /// <param name="dataset">the dataset containing the resource</param>
        /// <param name="resource">the instance to run constraint checks on</param>
        /// <param name="monitor">an (optional) progress monitor (currently ignored)</param>
        /// <returns>a List of ConstraintViolations (empty if all is OK)</returns>
        public List<ConstraintViolation> CheckConstraints(SpinWrappedDataset dataset, IEnumerable<INode> resources, IProgressMonitor monitor)
        {
            return CheckConstraints(dataset, resources, new List<SPINStatistics>(), monitor);
        }

        /// <summary>
        /// Checks all spin:constraints for a given Resource set.
        /// </summary>
        /// <param name="dataset">the model containing the resource</param>
        /// <param name="resource">the instance to run constraint checks on</param>
        /// <param name="stats">an (optional) List to add statistics to</param>
        /// <param name="monitor">an (optional) progress monitor (currently ignored)</param>
        /// <returns>a List of ConstraintViolations (empty if all is OK)</returns>
        public List<ConstraintViolation> CheckConstraints(SpinWrappedDataset dataset, IEnumerable<INode> resources, List<SPINStatistics> stats, IProgressMonitor monitor)
        {
            List<ConstraintViolation> results = new List<ConstraintViolation>();
            //SPINConstraints.addConstraintViolations(this, results, dataset, resource, SPIN.constraint, false, stats, monitor);
            return results;
        }

        /// <summary>
        /// Checks all instances in a given Model against all spin:constraints and returns a List of constraint violations. 
        /// A IProgressMonitor can be provided to enable the user to get intermediate status reports and to cancel the operation.
        /// </summary>
        /// <param name="dataset">the dataset to run constraint checks on</param>
        /// <param name="monitor">an (optional) progress monitor (currently ignored)</param>
        /// <returns>a List of ConstraintViolations (empty if all is OK)</returns>
        public List<ConstraintViolation> CheckConstraints(SpinWrappedDataset dataset, IProgressMonitor monitor)
        {
            return CheckConstraints(dataset, (List<SPINStatistics>)null, monitor);
        }


        /// <summary>
        /// Checks all instances in a given Model against all spin:constraints and returns a List of constraint violations. 
        /// A IProgressMonitor can be provided to enable the user to get intermediate status reports and to cancel the operation.
        /// </summary>
        /// <param name="dataset">the dataset to run constraint checks on</param>
        /// <param name="stats">an (optional) List to add statistics to</param>
        /// <param name="monitor">an (optional) progress monitor (currently ignored)</param>
        /// <returns>a List of ConstraintViolations (empty if all is OK)</returns>
        public List<ConstraintViolation> CheckConstraints(SpinWrappedDataset dataset, List<SPINStatistics> stats, IProgressMonitor monitor)
        {
            List<ConstraintViolation> results = new List<ConstraintViolation>();
            //SPINConstraints.run(this, dataset, results, stats, monitor);
            return results;
        }


        #endregion

        //#region SPIN rules evaluation

        //public void ApplyRules(SpinWrapperDataset dataset, Uri spinRule)
        //{
        //}

        //#endregion

        #region SPIN user's queries wrapping

        // TODO make the cache dynamic and set limits on the queryCache
        private Dictionary<String, ICommand> queryCache = new Dictionary<String, ICommand>();

        // TODO make it also work with a SparqlParameterizedString object
        // TODO ?really make SparqlQuery really parameterized by remapping parameters into variables with values => find and equivalence for sql NULL ?
        public IQuery BuildQuery(String sparqlQuery)
        {
            IQuery spinQuery = null;
            if (queryCache.ContainsKey(sparqlQuery))
            {
                spinQuery = (IQuery)queryCache[sparqlQuery];
            }
            else
            {
                _currentSparqlGraph = new Graph();
                _currentSparqlGraph.BaseUri = UriFactory.Create("sparql-query:" + sparqlQuery);
                INode q = new SparqlQueryParser().ParseFromString(sparqlQuery).ToSpinRdf(_currentSparqlGraph);
                if (!_currentSparqlGraph.IsEmpty)
                {
                    _spinConfiguration.AddGraph(_currentSparqlGraph);
                    spinQuery = SPINFactory.asQuery(Resource.Get(q, this));
                    queryCache[sparqlQuery] = spinQuery;
                }
            }
            return spinQuery;
        }

        public IEnumerable<IUpdate> BuildUpdate(String sparqlQuery)
        {
            List<IUpdate> spinQueryList = new List<IUpdate>();
            SparqlUpdateCommandSet query = new SparqlUpdateParser().ParseFromString(sparqlQuery);
            query.Optimise();
            foreach (SparqlUpdateCommand command in query.Commands)
            {
                sparqlQuery = command.ToString();
                if (queryCache.ContainsKey(sparqlQuery))
                {
                    spinQueryList.Add((IUpdate)queryCache[sparqlQuery]);
                }
                else
                {
                    _currentSparqlGraph = new Graph();
                    _currentSparqlGraph.BaseUri = UriFactory.Create("sparql-query:" + sparqlQuery);
                    INode q = command.ToSpinRdf(_currentSparqlGraph);
                    if (!_currentSparqlGraph.IsEmpty)
                    {
                        _spinConfiguration.AddGraph(_currentSparqlGraph);
                        IUpdate spinQuery = SPINFactory.asUpdate(Resource.Get(q, this));
                        queryCache[sparqlQuery] = spinQuery;
                        spinQueryList.Add(spinQuery);
                    }
                }
            }
            return spinQueryList;
        }

        #endregion

    }
}