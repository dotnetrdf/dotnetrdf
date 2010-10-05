using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Storage;

namespace Alexandria
{
    /// <summary>
    /// Abstract Base Class for Alexandria Wide Table Store Managers
    /// </summary>
    public abstract class AlexandriaWideTableStoreManager : IGenericIOManager
    {
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

        public bool IsReady
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
