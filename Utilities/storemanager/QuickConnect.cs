using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Storage;

namespace dotNetRDFStore
{
    public class QuickConnect
    {
        private IGraph _g;
        private INode _objNode;

        public QuickConnect(IGraph g, INode objNode)
        {
            this._g = g;
            this._objNode = objNode;
        }

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
    }
}
