using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Inference;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.PropertyFunctions;
using VDS.RDF.Query.Spin.Core.Runtime;
using VDS.RDF.Query.Spin.Core.Runtime.Factories;
using VDS.RDF.Query.Spin.Core.Runtime.Registries;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Storage;

namespace VDS.RDF.Query.Spin
{
    /// <summary>
    /// An instance of a SPIN model that is defined for a specific Storage
    /// </summary>
    /// <remarks>
    /// TODO try and use the Ontology API
    /// TODO check whether and how to define security for SPIN imported graphs access
    /// TODO add a property to get the list of spin imports' uris
    /// </remarks>
    public class SpinModel
        : IPropertyFunctionFactory, ISparqlCustomExpressionFactory
    {
        #region Static members

        // TODO replace this with a static initializer that uses the SPINImports class
        private static HashSet<Uri> BUILTIN_SPIN_GRAPHS = new HashSet<Uri>(RDFHelper.uriComparer)
        {
            UriFactory.Create(SP.BASE_URI),
            UriFactory.Create(SPIN.BASE_URI),
            UriFactory.Create(SPINMAP.BASE_URI),
            UriFactory.Create(SPINx.BASE_URI),
            UriFactory.Create(SPL.BASE_URI),
            UriFactory.Create(SPR.BASE_URI),
            UriFactory.Create(SPRA.BASE_URI)
        };

        // A single hashet reference to keep track of already setup storages
        private static HashSet<String> _setupStorages = new HashSet<String>();

        // The registry for existing SpinModel instances
        private static Dictionary<String, SpinModel> _models = new Dictionary<String, SpinModel>();

        // TODO find a cleaner way to maintain the registry
        internal static SpinModel Get(Connection connection)
        {
            if (_models.ContainsKey(connection.ID)) return _models[connection.ID];
            if (_models.ContainsKey(connection.StorageProvider.Resource.ToString()))
            {
                // TODO listen to connection events to detect any spinImports modification or change to a SpinImported graph if possible
                return _models[connection.StorageProvider.Resource.ToString()];
            }
            return null;
        }

        // TODO find a cleaner way to maintain the registry
        internal static void Register(SpinStorageProvider storageProvider, IQueryableStorage store, IEnumerable<Uri> spinImports)
        {
            SpinModel model = new SpinModel(storageProvider, store, spinImports);
            _models[storageProvider.Resource.ToString()] = model;
        }

        /// <summary>
        /// Ensures that the base SPIN graph are added to the storage if not present
        /// </summary>
        /// <remarks>
        /// The corresponding graphs should be somehow tagged as readonly since we have no authority to update them.
        /// </remarks>
        /// <param name="storage"></param>
        internal static void EnsureSPINBase(IQueryableStorage storage)
        {
            if (_setupStorages.Contains(storage.ToString())) return;
            IRdfReader rdfReader = new RdfXmlParser();
            // TODO use the SPINImports class instead
            foreach (Uri graphUri in BUILTIN_SPIN_GRAPHS.Except(storage.ListGraphs()))
            {
                IGraph g = new Graph();
                g.BaseUri = graphUri;
                g.LoadFromUri(graphUri, rdfReader);
                storage.SaveGraph(g);
            }
            _setupStorages.Add(storage.ToString());
        }

        #endregion Static members

        private HashSet<Uri> _spinImports = new HashSet<Uri>(RDFHelper.uriComparer);
        private IGraph _inferenceGraph = new ThreadSafeGraph();
        private InMemoryDataset _spinDataset = new InMemoryDataset(new TripleStore(), true);
        private IInferenceEngine _reasoner;

        // A simple in-memory graph to be able to create new resources for the current model
        // TODO check whether we still need this 
        private IGraph _localResourceBuilder = new ThreadSafeGraph();

        #region Instantiation and initialisation

        private SpinModel(SpinStorageProvider storageProvider, IQueryableStorage store, IEnumerable<Uri> spinImports)
        {
            EnsureSPINBase(store);
            Queue<Uri> importList = new Queue<Uri>();
            foreach (Uri importUri in spinImports)
            {
                importList.Enqueue(importUri);
            }
            while (importList.Count > 0)
            {
                Uri importUri = importList.Dequeue();
                if (!_spinImports.Contains(importUri))
                {
                    IGraph spinGraph = SPINImports.Load(importUri, new RdfXmlParser());
                    store.SaveGraph(spinGraph);
                    _spinDataset.AddGraph(spinGraph);
                    _spinImports.Add(importUri);
                    foreach (Uri dependency in spinGraph.GetTriplesWithPredicate(SPIN.PropertyImports).Select(t => ((IUriNode)t.Object).Uri))
                    {
                        importList.Enqueue(dependency);
                    }
                    foreach (Uri dependency in spinGraph.GetTriplesWithPredicate(OWL.PropertyImports).Select(t => ((IUriNode)t.Object).Uri))
                    {
                        importList.Enqueue(dependency);
                    }
                }
            }
            _spinImports.UnionWith(BUILTIN_SPIN_GRAPHS);
            Initialize();
        }

        /// <summary>
        /// Creates a SPIN model from a connection
        /// </summary>
        /// <param name="connection">The connection to get the model configuration from</param>
        private SpinModel(Connection connection)
        {
            IGraph spinConfiguration = connection.ServiceDescription;
        }

        // TODO check how to define the model efficiently
        internal void Initialize()
        {
            _reasoner = new StaticRdfsReasoner();
            foreach (IGraph spinGraph in _spinDataset.Graphs.Where(g => g.BaseUri != null).ToList())
            {
                _reasoner.Initialise(spinGraph);
            }
            foreach (IGraph spinGraph in _spinDataset.Graphs.Where(g => g.BaseUri != null).ToList())
            {
                _reasoner.Apply(spinGraph, _inferenceGraph);
            }
            _spinDataset.AddGraph(_inferenceGraph);
        }

        public void Dispose()
        {
            foreach (IGraph g in _spinDataset.Graphs)
            {
                SPINImports.UnRegister(g);
                g.Dispose();
            }
        }

        #endregion Instantiation and initialisation

        #region Spin Model utilities

        internal SpinResource AsResource(INode node)
        {
            return SpinResource.Get(node, this);
        }

        internal void SortClasses(List<SpinResource> classList)
        {
            classList.Sort(delegate(SpinResource x, SpinResource y)
            {
                if (RDFHelper.SameTerm(x, y)) return 0;
                if (ContainsTriple(x, RDFS.PropertySubClassOf, y))
                {
                    return 1;
                }
                return -1;
            });
        }

        internal void SortProperties(List<SpinResource> propertyList)
        {
            propertyList.Sort(delegate(SpinResource x, SpinResource y)
            {
                if (RDFHelper.SameTerm(x, y)) return 0;
                if (ContainsTriple(x, RDFS.PropertySubPropertyOf, y))
                {
                    return 1;
                }
                return -1;
            });
        }

        internal IEnumerable<IUpdateResource> GetConstructorsForClass(INode cls)
        {
            List<IUpdateResource> constructors = null; // GetTriplesWithSubjectPredicate(cls, SPIN.PropertyConstructor).Select(t => new CommandWrapper(SPINFactory.asUpdate(Resource.Get(t.Object, this)))).ToList();
            return constructors;
        }

        internal IEnumerable<IResource> GetAllInstances(INode cls)
        {
            List<SpinResource> resourceList = GetTriplesWithPredicateObject(RDF.PropertyType, cls).Select(t => SpinResource.Get(t.Subject, this)).ToList();
            return resourceList;
        }

        internal IEnumerable<IResource> GetAllSubClasses(INode root, bool includeRoot = false)
        {
            List<IResource> classList = new List<IResource>();
            if (includeRoot)
            {
                classList.Add(SpinResource.Get(root, this));
            }
            foreach (Triple t in GetTriplesWithPredicateObject(RDFS.PropertySubClassOf, root))
            {
                classList.AddRange(GetAllSubClasses(t.Object, true));
            }
            return classList;
        }

        internal IEnumerable<IResource> GetAllSuperClasses(INode root, bool includeRoot = false)
        {
            List<IResource> classList = new List<IResource>();
            if (includeRoot)
            {
                classList.Add(SpinResource.Get(root, this));
            }
            foreach (Triple t in GetTriplesWithSubjectPredicate(root, RDFS.PropertySubClassOf))
            {
                classList.AddRange(GetAllSuperClasses(t.Object, true));
            }
            return classList;
        }

        internal IEnumerable<IResource> GetAllSubProperties(INode root, bool includeRoot = false)
        {
            List<SpinResource> propertyList = GetTriplesWithPredicateObject(RDFS.PropertySubPropertyOf, root).Select(t => SpinResource.Get(t.Subject, this)).ToList();
            if (includeRoot)
            {
                propertyList.Add(SpinResource.Get(root, this));
            }
            SortProperties(propertyList);
            return propertyList;
        }

        internal IEnumerable<IResource> GetAllSuperProperties(INode root, bool includeRoot = false)
        {
            List<SpinResource> propertyList = GetTriplesWithSubjectPredicate(root, RDFS.PropertySubPropertyOf).Select(t => SpinResource.Get(t.Object, this)).ToList();
            if (includeRoot)
            {
                propertyList.Add(SpinResource.Get(root, this));
            }
            SortProperties(propertyList);
            return propertyList;
        }

        internal IEnumerable<IUriNode> GetMostSpecificClasses(IEnumerable<IUriNode> classes)
        {
            return classes
                            .Where(leaf => !classes
                                .SelectMany(c => GetAllSuperClasses(c, false)
                                    .Select(r => ((IUriNode)r.AsNode())))
                                    .Contains(leaf));
            //return classList;
        }

        internal IEnumerable<IUriNode> GetMostSpecificProperties(IEnumerable<IUriNode> properties)
        {
            return properties
                            .Where(leaf => !properties
                                .SelectMany(c => GetAllSuperProperties(c, false)
                                    .Select(r => ((IUriNode)r.AsNode())))
                                    .Contains(leaf));
            //return classList;
        }

        private INode GetSourceNode(INode resource)
        {
            if (resource is IResource)
            {
                return ((IResource)resource).AsNode();
            }
            return resource;
        }

        internal IResource CreateResource(INode rdfType = null)
        {
            // TODO put that in another graph to not overflow the configuration with temporary requests ?
            INode resource = _localResourceBuilder.CreateBlankNode();
            if (rdfType != null)
            {
                _localResourceBuilder.Assert(resource, Tools.CopyNode(RDF.PropertyType, _localResourceBuilder), Tools.CopyNode(rdfType, _localResourceBuilder));
            }
            return SpinResource.Get(resource, this);
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
            return _spinDataset.GetTriplesWithSubject(GetSourceNode(subj));
        }

        internal IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            return _spinDataset.GetTriplesWithPredicate(GetSourceNode(pred));
        }

        internal IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            return _spinDataset.GetTriplesWithObject(GetSourceNode(obj));
        }

        internal IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return _spinDataset.GetTriplesWithPredicateObject(GetSourceNode(pred), GetSourceNode(obj));
        }

        internal IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return _spinDataset.GetTriplesWithSubjectPredicate(GetSourceNode(subj), GetSourceNode(pred));
        }

        internal IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return _spinDataset.GetTriplesWithSubjectObject(GetSourceNode(subj), GetSourceNode(obj));
        }

        internal bool ContainsTriple(INode subj, INode pred, INode obj)
        {
            return ContainsTriple(new Triple(Tools.CopyNode(GetSourceNode(subj), _localResourceBuilder), Tools.CopyNode(GetSourceNode(pred), _localResourceBuilder), Tools.CopyNode(GetSourceNode(obj), _localResourceBuilder)));
        }

        internal bool ContainsTriple(Triple t)
        {
            return _spinDataset.ContainsTriple(t);
        }

        internal bool IsMagicPropertyFunction(Uri u)
        {
            // TODO use the compiled spinConfiguration
            IResource resource = AsResource(RDFHelper.CreateUriNode(u));
            return resource.CanAs(SPIN.ClassMagicProperty);
        }

        internal bool IsFunction(Uri u)
        {
            // TODO use the compiled spinConfiguration
            IResource resource = AsResource(RDFHelper.CreateUriNode(u));
            return resource.CanAs(SPIN.ClassFunction);
        }

        // TODO handle a template call class
        internal ITemplateResource GetTemplate(Uri uri)
        {
            ITemplateResource spinDeclaration = ResourceFactory.asTemplate(AsResource(RDFHelper.CreateUriNode(uri)));
            if (spinDeclaration == null) return null;
            return spinDeclaration; // new SpinMagicPropertyCall(spinDeclaration);
        }

        internal SpinMagicPropertyCall GetMagicPropertyCall(Uri functor)
        {
            IFunctionResource spinDeclaration = ResourceFactory.asFunction(AsResource(RDFHelper.CreateUriNode(functor)));
            if (spinDeclaration == null || !spinDeclaration.isMagicProperty()) return null;
            SpinMagicPropertyCall call = (SpinMagicPropertyCall)SpinFactory.TryCreate(functor, this);
            if (call != null) return call;
            return new SpinMagicPropertyCall(spinDeclaration);
        }

        internal SpinFunctionCall GetSpinFunctionCall(Uri functor)
        {
            IFunctionResource spinDeclaration = ResourceFactory.asFunction(AsResource(RDFHelper.CreateUriNode(functor)));
            if (spinDeclaration == null) return null;
            SpinFunctionCall call = (SpinFunctionCall)SpinFactory.TryCreate(functor, this);
            if (call != null) return call;
            return new SpinFunctionCall(spinDeclaration);
        }

        #endregion Spin Model utilities

        #region IPropertyFunctionFactory members

        public bool IsPropertyFunction(Uri u)
        {
            return IsMagicPropertyFunction(u);
        }

        public bool TryCreatePropertyFunction(PropertyFunctionInfo info, out Patterns.IPropertyFunctionPattern function)
        {
            SpinMagicPropertyCall propertyFunction = GetMagicPropertyCall(info.FunctionUri);
            if (propertyFunction != null)
            {
                function = new PropertyFunctionPattern(info, propertyFunction);
                return true;
            }
            function = null;
            return false;
        }

        #endregion IPropertyFunctionFactory members

        #region ISparqlCustomExpressionFactory members

        public bool TryCreateExpression(Uri u, List<ISparqlExpression> args, Dictionary<string, ISparqlExpression> scalarArguments, out ISparqlExpression expr)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Uri> AvailableExtensionFunctions
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Uri> AvailableExtensionAggregates
        {
            get { throw new NotImplementedException(); }
        }

        #endregion ISparqlCustomExpressionFactory members
    }
}