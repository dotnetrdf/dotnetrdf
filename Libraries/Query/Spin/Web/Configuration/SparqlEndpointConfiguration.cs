using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Query.Spin.SparqlStrategies;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Storage;
using VDS.RDF.Web;
using VDS.RDF.Web.Configuration.Server;

namespace VDS.RDF.Query.Spin.Web.Configuration
{
    // TODO check whether we need to/can relocate this directly into the connection ?

    /// <summary>
    /// Concrete implementation of a Handler Configuration for Spin Servers
    /// </summary>
    public class SparqlEndpointConfiguration
        : BaseSparqlServerConfiguration
    {

        public Connection Connection { get; private set; }
        /// <summary>
        /// Creates a new SPARQL Server Configuration from information in a Configuration Graph
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public SparqlEndpointConfiguration(IHttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode)
        {
            INode storageNode = g.GetTriplesWithSubjectPredicate(objNode, RDFHelper.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyStorageProvider))).Select(t => t.Object).FirstOrDefault();
            object storageTemp = ConfigurationLoader.LoadObject(g, storageNode);
            if (!(storageTemp is SpinStorageProvider)) throw new DotNetRdfConfigurationException("The storage provider for Uri <" + objNode.ToString() + "> is not a SpinStorageProvider as required");

            SpinStorageProvider storage = (SpinStorageProvider)storageTemp;
            Connection = storage.GetConnection();

            //_expressionFactories.Add(SpinModel.Get(Connection));
            _propertyFunctionFactories.Add(SpinModel.Get(Connection));

            // TODO get the service description for this connection and update the configuration with it instead of this
            _queriesEnabled = true;
            _updatesEnabled = true; // !Connection.IsReadOnly;
            _httpProtocolEnabled = true;
        }

        /// <summary>
        /// Creates a new SPARQL Server Configuration from information in a Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public SparqlEndpointConfiguration(IGraph g, INode objNode)
            : base(null, g, objNode)
        {
        }

    }
}
