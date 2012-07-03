/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

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
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    public class ListGraphsTask 
        : NonCancellableTask<IEnumerable<Uri>>
    {
        private IStorageProvider _manager;

        public ListGraphsTask(IStorageProvider manager)
            : base("List Graphs")
        {
            this._manager = manager;
        }

        protected override IEnumerable<Uri> RunTaskInternal()
        {
            if (!this._manager.IsReady)
            {
                this.Information = "Waiting for Store to become ready...";
                this.RaiseStateChanged();
                while (!this._manager.IsReady)
                {
                    Thread.Sleep(250);
                }
            }

            if (this._manager.ListGraphsSupported)
            {
                return this._manager.ListGraphs();
            }
            else if (this._manager is IQueryableStorage)
            {
                List<Uri> uris = new List<Uri>();
                Object results = ((IQueryableStorage)this._manager).Query("SELECT DISTINCT ?g WHERE {GRAPH ?g {?s ?p ?o}}");
                if (results is SparqlResultSet)
                {
                    SparqlResultSet rset = (SparqlResultSet)results;
                    foreach (SparqlResult res in rset)
                    {
                        if (res["g"] != null && res["g"].NodeType == NodeType.Uri)
                        {
                            uris.Add(((IUriNode)res["g"]).Uri);
                        }
                    }
                    return uris;
                }
                else
                {
                    throw new RdfStorageException("Store failed to list graphs");
                }
            }
            else
            {
                throw new RdfStorageException("Store does not provide a means to list graphs");
            }
        }
    }
}
