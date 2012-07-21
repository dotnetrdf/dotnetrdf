/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF;
using VDS.RDF.Storage;

namespace dotNetRDFTest
{
    class MockGenericManager 
        : IStorageProvider
    {
        #region IStorageProvider Members

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
