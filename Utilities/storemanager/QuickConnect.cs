using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Storage;

namespace dotNetRDFStore
{
    /// <summary>
    /// Class for quickly loading a Connection when the Configuration Graph and Object Node are known
    /// </summary>
    public class QuickConnect
    {
        private IGraph _g;
        private INode _objNode;

        /// <summary>
        /// Creates a new Quick Connection
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public QuickConnect(IGraph g, INode objNode)
        {
            this._g = g;
            this._objNode = objNode;
        }

        /// <summary>
        /// Attempts to load the Connection
        /// </summary>
        /// <returns></returns>
        public IGenericIOManager GetConnection()
        {
            Object temp = ConfigurationLoader.LoadObject(this._g, this._objNode);
            if (temp is IGenericIOManager)
            {
                return (IGenericIOManager)temp;
            }
            else
            {
                throw new DotNetRdfConfigurationException("Connection Configuration appears to be invalid as could not load an Object which implements the IGenericIOManager interface from the given Configuration Graph");
            }
        }

        /// <summary>
        /// Gets the Node that this Connection should be loaded from
        /// </summary>
        public INode ObjectNode
        {
            get
            {
                return this._objNode;
            }
        }
    }
}
