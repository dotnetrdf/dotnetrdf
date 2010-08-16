using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF;
using VDS.RDF.Storage;

namespace dotNetRDFTest
{
    class MockGenericManager : IGenericIOManager
    {
        #region IGenericIOManager Members

        public void LoadGraph(IGraph g, Uri graphUri)
        {
            Thread.Sleep(1000);
            if (g.IsEmpty) g.BaseUri = graphUri;
        }

        public void LoadGraph(IGraph g, string graphUri)
        {
            Thread.Sleep(1000);
            if (g.IsEmpty) g.BaseUri = new Uri(graphUri);
        }

        public void SaveGraph(IGraph g)
        {
            Thread.Sleep(2000);
        }

        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            Thread.Sleep(1500);
        }

        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            Thread.Sleep(1500);
        }

        public bool UpdateSupported
        {
            get { return true; }
        }

        public bool IsReady
        {
            get { return true; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            
        }

        #endregion
    }
}
