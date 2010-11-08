using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage
{
    //REQ: Implement a Fuseki Connector

    /// <summary>
    /// Class for connecting to any dataset that can be exposed via Fuseki
    /// </summary>
    /// <remarks>
    /// <strong>Not yet implemented</strong>
    /// </remarks>
    public class FusekiConnector : IGenericIOManager
    {
        #region IGenericIOManager Members

        public void LoadGraph(IGraph g, Uri graphUri)
        {
            throw new NotImplementedException();
        }

        public void LoadGraph(IGraph g, string graphUri)
        {
            throw new NotImplementedException();
        }

        public void SaveGraph(IGraph g)
        {
            throw new NotImplementedException();
        }

        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new NotImplementedException();
        }

        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new NotImplementedException();
        }

        public bool UpdateSupported
        {
            get { throw new NotImplementedException(); }
        }

        public void DeleteGraph(Uri graphUri)
        {
            throw new NotImplementedException();
        }

        public void DeleteGraph(string graphUri)
        {
            throw new NotImplementedException();
        }

        public bool DeleteSupported
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReady
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
