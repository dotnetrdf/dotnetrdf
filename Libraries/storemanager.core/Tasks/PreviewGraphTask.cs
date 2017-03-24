/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    /// <summary>
    /// Task which previews a graph
    /// </summary>
    public class PreviewGraphTask 
        : CancellableTask<IGraph>
    {
        private readonly IStorageProvider _manager;
        private readonly String _graphUri;
        private readonly int _previewSize = 100;
        private CancellableHandler _canceller;

        /// <summary>
        /// Creates a new preview graph task
        /// </summary>
        /// <param name="manager">Storage Provider</param>
        /// <param name="graphUri">URI of the graph to preview</param>
        /// <param name="previewSize">Preview Size</param>
        public PreviewGraphTask(IStorageProvider manager, String graphUri, int previewSize)
            : base("Preview Graph")
        {
            this._manager = manager;
            this._graphUri = graphUri;
            this._previewSize = previewSize;
        }

        /// <summary>
        /// Runs the task
        /// </summary>
        /// <returns></returns>
        protected override IGraph RunTaskInternal()
        {
            if (this._graphUri != null && !this._graphUri.Equals(String.Empty))
            {
                this.Information = "Previewing Graph " + this._graphUri + "...";
            }
            else
            {
                this.Information = "Previewing Default Graph...";
            }

            Graph g = new Graph();
            this._canceller = new CancellableHandler(new PagingHandler(new GraphHandler(g), this._previewSize));
            this._manager.LoadGraph(this._canceller, this._graphUri);
            this.Information = "Previewed Graph previews first " + g.Triples.Count + " Triple(s)";
            return g;
        }

        /// <summary>
        /// Cancels the task
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
