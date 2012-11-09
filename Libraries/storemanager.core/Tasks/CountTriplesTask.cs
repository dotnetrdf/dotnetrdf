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
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    /// <summary>
    /// Task which counts the triples present in a Graph
    /// </summary>
    public class CountTriplesTask
        : CancellableTask<TaskValueResult<int>>
    {
        private IStorageProvider _manager;
        private String _graphUri;
        private CancellableHandler _canceller;
        private CountHandler _counter;

        /// <summary>
        /// Creates a new Count Triples task
        /// </summary>
        /// <param name="manager">Storage Provider</param>
        /// <param name="graphUri">Graph URI</param>
        public CountTriplesTask(IStorageProvider manager, String graphUri)
            : base("Count Triples")
        {
            this._manager = manager;
            this._graphUri = graphUri;
        }

        /// <summary>
        /// Runs the Task
        /// </summary>
        /// <returns></returns>
        protected override TaskValueResult<int> RunTaskInternal()
        {
            if (this._graphUri != null && !this._graphUri.Equals(String.Empty))
            {
                this.Information = "Counting Triples for Graph " + this._graphUri + "...";
            }
            else
            {
                this.Information = "Counting Triples for Default Graph...";
            }

            this._counter = new CountHandler();
            this._canceller = new CancellableHandler(this._counter);
            this._manager.LoadGraph(this._canceller, this._graphUri);
            this.Information = "Graph contains " + this._counter.Count + " Triple(s)";
            return new TaskValueResult<int>(this._counter.Count);
        }

        /// <summary>
        /// Cancels the Task
        /// </summary>
        protected override void CancelInternal()
        {
            if (this._canceller != null)
            {
                this._canceller.Cancel();
            }
        }
    }
}
