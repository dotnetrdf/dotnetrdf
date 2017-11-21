/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Inference;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Spin
{
    /// <summary>
    /// TODO make this class internal
    /// TODO refactor the initialization process to allow for dynamic SPIN configuration updates through the dataset API.
    /// TODO avoid multiple query transformations (ie String => SparqlQuery => SPIN Resources => String) by using the native dotNetRDF algebra classes instead of IResource
    /// </summary>
    internal class SpinProcessor //: IInferenceEngine
    {

        // TODO change support to OWL ?
        private IInferenceEngine _reasoner = new StaticRdfsReasoner();

        internal InMemoryDataset _spinConfiguration;

        private IGraph _currentSparqlGraph = new ThreadSafeGraph();

        #region "SPIN processor Initialisation"

        internal SpinProcessor()
        {
            _spinConfiguration = new InMemoryDataset(true);

            // Ensure that SP, SPIN and SPL are present
            IRdfReader rdfReader = new RdfXmlParser();
            Initialise(UriFactory.Create(SP.BASE_URI), rdfReader);
            Initialise(UriFactory.Create(SPIN.BASE_URI), rdfReader);
            Initialise(UriFactory.Create(SPL.BASE_URI), rdfReader);
        }

        internal void Initialise(Uri spinGraphUri, IRdfReader rdfReader = null)
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
        internal IGraph Initialise(IGraph spinGraph)
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


        internal void SortClasses(List<Resource> classList)
        {
            classList.Sort(delegate(Resource x, Resource y)
            {
                if (RDFUtil.sameTerm(x, y)) return 0;
                if (ContainsTriple(x, RDFS.PropertySubClassOf, y))
                {
                    return 1;
                }
                return -1;
            });
        }

        internal void SortProperties(List<Resource> propertyList)
        {
            propertyList.Sort(delegate(Resource x, Resource y)
            {
                if (RDFUtil.sameTerm(x, y)) return 0;
                if (ContainsTriple(x, RDFS.PropertySubPropertyOf, y))
                {
                    return 1;
                }
                return -1;
            });
        }

        internal IEnumerable<IUpdate> GetConstructorsForClass(INode cls)
        {
            List<IUpdate> constructors = GetTriplesWithSubjectPredicate(cls, SPIN.PropertyConstructor).Select(t => SPINFactory.asUpdate(Resource.Get(t.Object, this))).ToList();
            return constructors;
        }

        internal IEnumerable<IResource> GetAllInstances(INode cls)
        {
            List<Resource> resourceList = GetTriplesWithPredicateObject(RDF.PropertyType, cls).Select(t => Resource.Get(t.Subject, this)).ToList();
            return resourceList;
        }

        internal IEnumerable<IResource> GetAllSubClasses(INode root, bool includeRoot = false)
        {
            List<Resource> classList = GetTriplesWithPredicateObject(RDFS.PropertySubClassOf, root).Select(t => Resource.Get(t.Subject, this)).ToList();
            if (includeRoot)
            {
                classList.Add(Resource.Get(root, this));
            }
            SortClasses(classList);
            return classList;
        }

        internal IEnumerable<IResource> GetAllSuperClasses(INode root, bool includeRoot = false)
        {
            List<Resource> classList = GetTriplesWithSubjectPredicate(root, RDFS.PropertySubClassOf).Select(t => Resource.Get(t.Object, this)).ToList();
            if (includeRoot)
            {
                classList.Add(Resource.Get(root, this));
            }
            SortClasses(classList);
            return classList;
        }

        internal IEnumerable<IResource> GetAllSubProperties(INode root, bool includeRoot = false)
        {
            List<Resource> propertyList = GetTriplesWithPredicateObject(RDFS.PropertySubPropertyOf, root).Select(t => Resource.Get(t.Subject, this)).ToList();
            if (includeRoot)
            {
                propertyList.Add(Resource.Get(root, this));
            }
            SortProperties(propertyList);
            return propertyList;
        }

        internal IEnumerable<IResource> GetAllSuperProperties(INode root, bool includeRoot = false)
        {
            List<Resource> propertyList = GetTriplesWithSubjectPredicate(root, RDFS.PropertySubPropertyOf).Select(t => Resource.Get(t.Object, this)).ToList();
            if (includeRoot)
            {
                propertyList.Add(Resource.Get(root, this));
            }
            SortProperties(propertyList);
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


        // TODO the three following methods are not needed anymore. Refactor them into a SPARQLMotion API ?

        /*
        /// <summary>
        /// The default implementation applies all rules then checks all constraints and returns wether the results raised constraints violations or not.
        /// Allow any subclass to define how SPIN processing would be applied when a dataset is flushed. 
        /// TODO this should be complemented with an ExecuteScript method that would allow to exploit SPARQLMotion scripts in the model.
        /// </summary>
        /// <param name="dataset">The Dataset to apply SPIN processing on</param>
        /// <param name="resources">A list of resources to check constraints on</param>
        /// <returns>true if no constraint violation is raised, false otherwise</returns>
        internal bool Apply(SpinWrappedDataset dataset, IEnumerable<INode> resources)
        {
            dataset.CreateExecutionContext(resources);
            return ApplyInternal(dataset);
        }

        // TODO perhaps use the null uri to avoid creating a graph with all rdftype triples ?
        internal bool Apply(SpinWrappedDataset dataset)
        {
            dataset.CreateExecutionContext(null);
            return ApplyInternal(dataset);
        }

        // TODO perhaps use the null uri to avoid creating a graph with all rdftype triples ?
        internal protected virtual bool ApplyInternal(SpinWrappedDataset dataset)
        {



            dataset.QueryExecutionMode = SpinWrappedDataset.QueryMode.SpinConstraintsChecking;
            IEnumerable<ConstraintViolation> vios = new List<ConstraintViolation>(); //runConstraints(dataset, resources, null);
            dataset.QueryExecutionMode = SpinWrappedDataset.QueryMode.UserQuerying;
            return vios.Count() == 0;
        }
        */
        #endregion

        #region SPIN user's queries wrapping

        // TODO make the cache dynamic and set limits on the queryCache
        private Dictionary<String, ICommand> queryCache = new Dictionary<String, ICommand>();

        internal IQuery BuildQuery(String sparqlQuery)
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

        internal IEnumerable<IUpdate> BuildUpdate(String sparqlQuery)
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