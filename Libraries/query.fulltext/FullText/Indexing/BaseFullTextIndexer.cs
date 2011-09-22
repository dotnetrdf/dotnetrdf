/*

Copyright Robert Vesse 2009-11
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
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.FullText.Indexing
{
    /// <summary>
    /// Abstract Base Class for Full Text Indexers which implements the basic logic leaving derived classes to implement the index specific logic
    /// </summary>
    public abstract class BaseFullTextIndexer
        : IFullTextIndexer
    {
        /// <summary>
        /// Destructor for the Indexer which ensures it is disposed of
        /// </summary>
        ~BaseFullTextIndexer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the Indexing Mode used by this Indexer
        /// </summary>
        public abstract IndexingMode IndexingMode
        {
            get;
        }

        /// <summary>
        /// Indexes a Triple
        /// </summary>
        /// <param name="t">Triple</param>
        public abstract void Index(Triple t);

        /// <summary>
        /// Indexes a Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public void Index(IGraph g)
        {
            foreach (Triple t in g.Triples)
            {
                this.Index(t);
            }
        }

        /// <summary>
        /// Indexes a Dataset
        /// </summary>
        /// <param name="dataset">Dataset</param>
        public void Index(ISparqlDataset dataset)
        {
            foreach (Uri u in dataset.GraphUris)
            {
                IGraph g = dataset[u];
                this.Index(g);
            }
        }

        /// <summary>
        /// Unindexes a Triple
        /// </summary>
        /// <param name="t">Triple</param>
        public abstract void Unindex(Triple t);

        /// <summary>
        /// Unindexes a Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public void Unindex(IGraph g)
        {
            foreach (Triple t in g.Triples)
            {
                this.Unindex(t);
            }
        }

        /// <summary>
        /// Unindexes a Dataset
        /// </summary>
        /// <param name="dataset">Dataset</param>
        public void Unindex(ISparqlDataset dataset)
        {
            foreach (Uri u in dataset.GraphUris)
            {
                IGraph g = dataset[u];
                this.Unindex(g);
            }
        }

        /// <summary>
        /// Ensures any pending changes are flushed to the actual index
        /// </summary>
        public virtual void Flush() { }

        /// <summary>
        /// Disposes of the Indexer
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Disposes of the Indexer
        /// </summary>
        /// <param name="disposing">Whether this was called by the Dispose method</param>
        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            this.DisposeInternal();
        }

        /// <summary>
        /// Virtual method that can be overridden to add implementation specific dispose logic
        /// </summary>
        protected virtual void DisposeInternal() { }
    }
}
