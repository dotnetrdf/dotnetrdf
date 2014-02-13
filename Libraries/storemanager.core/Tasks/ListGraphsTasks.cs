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
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    /// <summary>
    /// Task which lists graph
    /// </summary>
    public class ListGraphsTask 
        : NonCancellableTask<IEnumerable<Uri>>
    {
        private readonly IStorageProvider _manager;

        /// <summary>
        /// Creates a new List Graphs task
        /// </summary>
        /// <param name="manager">Storage Provider</param>
        public ListGraphsTask(IStorageProvider manager)
            : base("List Graphs")
        {
            this._manager = manager;
        }

        /// <summary>
        /// Runs the task
        /// </summary>
        /// <returns></returns>
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
            if (this._manager is IQueryableStorage)
            {
                List<Uri> uris = new List<Uri>();
                Object results = ((IQueryableStorage)this._manager).Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { } }");
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
                throw new RdfStorageException("Store failed to list graphs");
            }
            throw new RdfStorageException("Store does not provide a means to list graphs");
        }
    }
}
