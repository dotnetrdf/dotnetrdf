using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Configuration;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlStrategies;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;
using VDS.RDF.Storage.Management.Provisioning;

namespace VDS.RDF.Query.Spin
{

    /// <summary>
    /// This class is responsible for 
    ///     => defining which SPARQL strategy handlers are necessary for a specific server and version
    ///     => creating the Connection objects to a specific store through the GetStore method.
    /// </summary>
    /// <remarks>
    /// TODO replace manual registration by the configuration then rename it finally to SpinStorageProvider 
    /// </remarks>
    public class SpinStorageProvider
        : ISparqlSDPlugin
    {

        private static readonly IUriNode PropertySparqlStrategy = RDFHelper.CreateUriNode(UriFactory.Create(ConfigurationLoader.ConfigurationNamespace + "sparqlStrategy"));


        private IGraph _configurationGraph;
        private INode _thisConfigurationNode;
        private INode _storageNode;
        private IGraph _sdFeaturesGraph = null;

        private List<ISparqlHandlingStrategy> _sparqlHandlers;

        internal SpinStorageProvider(IGraph g, INode thisNode, INode storageNode)
        {
            _configurationGraph = g;
            _thisConfigurationNode = thisNode;
            _storageNode = storageNode;

            INode strategyNode = thisNode;
            // Find and create the sparqlHandlers required for the underlying StorageProvider
            while (strategyNode != null)
            {
                if (strategyNode.IsListRoot(g))
                {
                    _sparqlHandlers = g.GetListItems(strategyNode)
                        .Select(n => (ISparqlHandlingStrategy)ConfigurationLoader.LoadObject(_configurationGraph, RDFHelper.CreateBlankNode(), Type.GetType(((ILiteralNode)n).Value))).ToList();
                    break;
                }
                strategyNode = g.GetTriplesWithSubjectPredicate(strategyNode, PropertySparqlStrategy).Select(t => t.Object).FirstOrDefault();
            }
            // Get the SPIN imports for this storage and create the model
            IEnumerable<Uri> spinImports = g.GetTriplesWithSubjectPredicate(_thisConfigurationNode, SPIN.PropertyImports).Select(t => ((IUriNode)t.Object).Uri);
            SpinModel.Register(this, (IQueryableStorage)ConfigurationLoader.LoadObject(_configurationGraph, _storageNode), spinImports);
        }

        public Connection GetConnection() {
            IStorageProvider storage = (IQueryableStorage)ConfigurationLoader.LoadObject(_configurationGraph, _storageNode);
            if (!(storage is IUpdateableStorage)) {
                throw new DotNetRdfConfigurationException("A SpinStorageProvider underlying storage must support SPARQL 1.1 Updates.");
            }
            return new Connection(this, (IUpdateableStorage)storage);
        }


        internal void Handle(SparqlCommandUnit command)
        {
            foreach (ISparqlHandlingStrategy handler in _sparqlHandlers)
            {
                handler.Handle(command);
            }
        }

        #region ISparqlSDPlugin members

        public INode Resource {
            get {
                return _thisConfigurationNode;
            }
        }

        public IGraph SparqlSDContribution
        {
            get
            {
                if (_sdFeaturesGraph != null) return _sdFeaturesGraph;
                _sdFeaturesGraph = new Graph();
                foreach (ISparqlHandlingStrategy handler in _sparqlHandlers)
                {
                    _sdFeaturesGraph.Assert(_thisConfigurationNode, SD.PropertyHasFeature, handler.Resource);
                    _sdFeaturesGraph.Merge(handler.SparqlSDContribution);
                }
                return _sdFeaturesGraph;
            }
        }

        #endregion

    }
}
