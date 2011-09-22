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
    /// Indicates the Indexing Mode used by an Indexer to index literals
    /// </summary>
    public enum IndexingMode
    {
        /// <summary>
        /// Indicates that the Indexer stores the original Object Literal whose text is indexed
        /// </summary>
        Objects,
        /// <summary>
        /// Indicates that the Indexer stores the Subject (Blank Node/URI) associated with the Literal whose text is indexed
        /// </summary>
        Subjects,
        /// <summary>
        /// Indicates that the Indexer stores the Predicate (URI) associated with the Literal whose text is indexed
        /// </summary>
        Predicates,
        /// <summary>
        /// Indicates that the Indexer uses some other custom indexing strategy
        /// </summary>
        Custom
    }

    /// <summary>
    /// Interface for classes that provide full text indexing functionality
    /// </summary>
    public interface IFullTextIndexer
        : IDisposable
    {
        /// <summary>
        /// Gets the Indexing Mode used
        /// </summary>
        IndexingMode IndexingMode
        {
            get;
        }

        /// <summary>
        /// Indexes a Triple
        /// </summary>
        /// <param name="t">Triple</param>
        void Index(Triple t);

        /// <summary>
        /// Indexes a Graph
        /// </summary>
        /// <param name="g">Graph</param>
        void Index(IGraph g);

        /// <summary>
        /// Indexes a Dataset
        /// </summary>
        /// <param name="dataset">Dataset</param>
        void Index(ISparqlDataset dataset);

        /// <summary>
        /// Unindexes a Triple
        /// </summary>
        /// <param name="t">Triple</param>
        void Unindex(Triple t);

        /// <summary>
        /// Unindexes a Graph
        /// </summary>
        /// <param name="g">Graph</param>
        void Unindex(IGraph g);

        /// <summary>
        /// Unindexes a Dataset
        /// </summary>
        /// <param name="dataset">Dataset</param>
        void Unindex(ISparqlDataset dataset);

        /// <summary>
        /// Flushes any outstanding changes to the Index
        /// </summary>
        void Flush();
    }
}
