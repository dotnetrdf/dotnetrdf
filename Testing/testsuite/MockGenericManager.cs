/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;

namespace dotNetRDFTest
{
    class MockGenericManager 
        : IStorageProvider
    {
        #region IStorageProvider Members

        public IStorageServer ParentServer
        {
            get
            {
                return null;
            }
        }

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

        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            handler.StartRdf();
            Thread.Sleep(1000);
            handler.EndRdf(true);
        }

        public void LoadGraph(IRdfHandler handler, String graphUri)
        {
            handler.StartRdf();
            Thread.Sleep(1000);
            handler.EndRdf(true);
        }

        public void SaveGraph(IGraph g)
        {
            Thread.Sleep(2000);
        }

        public IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.GraphStore | IOBehaviour.CanUpdateTriples;
            }
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

        public void DeleteGraph(Uri graphUri)
        {
            Thread.Sleep(500);
        }

        public void DeleteGraph(String graphUri)
        {
            Thread.Sleep(500);
        }

        public bool DeleteSupported
        {
            get
            {
                return true;
            }
        }

        public IEnumerable<Uri> ListGraphs()
        {
            Thread.Sleep(100);
            return Enumerable.Empty<Uri>();
        }

        public bool ListGraphsSupported
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            
        }

        #endregion
    }
}
