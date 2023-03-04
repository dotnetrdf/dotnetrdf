/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.FullText.Indexing
{
    /// <summary>
    /// Abstract Base Class for Full Text Indexers which implements the basic logic leaving derived classes to implement the index specific logic.
    /// </summary>
    public abstract class BaseFullTextIndexer
        : IFullTextIndexer
    {
        /// <summary>
        /// Destructor for the Indexer which ensures it is disposed of
        /// </summary>
        ~BaseFullTextIndexer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the Indexing Mode used by this Indexer.
        /// </summary>
        public abstract IndexingMode IndexingMode
        {
            get;
        }

        /// <summary>
        /// Indexes a Triple associating it with the given Graph.
        /// </summary>
        /// <param name="graphUri">Graph URI.</param>
        /// <param name="t">Triple.</param>
        protected abstract void Index(String graphUri, Triple t);

        /// <inheritdoc/>
        [Obsolete("Replaced by Index(IGraph, Triple)", true)]
        public virtual void Index(Triple t)
        {
            Index((IGraph)null, t);
        }

        /// <summary>
        /// Indexes a Triple.
        /// </summary>
        /// <param name="g">Graph context of the triple to be indexed.</param>
        /// <param name="t">Triple.</param>
        public virtual void Index(IGraph g, Triple t)
        {
            Index(g.Name.ToSafeString(), t);
        }

        /// <summary>
        /// Indexes a Graph.
        /// </summary>
        /// <param name="g">Graph.</param>
        public virtual void Index(IGraph g)
        {
            foreach (Triple t in g.Triples)
            {
                Index(g, t);
            }
            Flush();
        }

        /// <summary>
        /// Indexes a Dataset.
        /// </summary>
        /// <param name="dataset">Dataset.</param>
        public virtual void Index(ISparqlDataset dataset)
        {
            foreach (IRefNode u in dataset.GraphNames)
            {
                IGraph g = dataset[u];
                Index(g);
            }
        }

        /// <summary>
        /// Unindexes a Triple associating it with the given Graph.
        /// </summary>
        /// <param name="graphUri">Graph URI.</param>
        /// <param name="t">Triple.</param>
        protected abstract void Unindex(String graphUri, Triple t);

        /// <summary>
        /// Unindexes a Triple.
        /// </summary>
        /// <param name="t">Triple.</param>
        [Obsolete("Replaced by Unindex(IGraph, Triple). As triples no longer have a reference to a parent graph, this method should no longer be used.", true)]
        public virtual void Unindex(Triple t)
        {
            Unindex(string.Empty, t);
        }

        /// <inheritdoc />
        public virtual void Unindex(IGraph g, Triple t)
        {
            Unindex(g.Name.ToSafeString(), t);
        }

        /// <summary>
        /// Unindexes a Graph.
        /// </summary>
        /// <param name="g">Graph.</param>
        public virtual void Unindex(IGraph g)
        {
            foreach (Triple t in g.Triples)
            {
                Unindex(g, t);
            }
            Flush();
        }

        /// <summary>
        /// Unindexes a Dataset.
        /// </summary>
        /// <param name="dataset">Dataset.</param>
        public virtual void Unindex(ISparqlDataset dataset)
        {
            foreach (IRefNode u in dataset.GraphNames)
            {
                IGraph g = dataset[u];
                Unindex(g);
            }
        }

        /// <summary>
        /// Ensures any pending changes are flushed to the actual index.
        /// </summary>
        public virtual void Flush() { }

        /// <summary>
        /// Disposes of the Indexer.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes of the Indexer.
        /// </summary>
        /// <param name="disposing">Whether this was called by the Dispose method.</param>
        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            DisposeInternal();
        }

        /// <summary>
        /// Virtual method that can be overridden to add implementation specific dispose logic.
        /// </summary>
        protected virtual void DisposeInternal() { }
    }
}
