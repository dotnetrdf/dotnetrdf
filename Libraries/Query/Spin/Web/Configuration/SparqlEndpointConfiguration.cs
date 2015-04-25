/*

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Linq;
using VDS.RDF.Configuration;
using VDS.RDF.Query.Spin.Utility;
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