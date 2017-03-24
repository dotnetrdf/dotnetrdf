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
        /// <remarks>
        /// Implementations <emph>SHOULD NOT</emph> automatically Flush changes to the indexes at the end of this operation
        /// </remarks>
        void Index(Triple t);

        /// <summary>
        /// Indexes a Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <remarks>
        /// Implementations <emph>SHOULD</emph> automatically Flush changes to the indexes at the end of this operation
        /// </remarks>
        void Index(IGraph g);

        /// <summary>
        /// Indexes a Dataset
        /// </summary>
        /// <param name="dataset">Dataset</param>
        /// <remarks>
        /// Implementations <emph>SHOULD</emph> automatically Flush changes to the indexes at the end of this operation
        /// </remarks>
        void Index(ISparqlDataset dataset);

        /// <summary>
        /// Unindexes a Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <remarks>
        /// Implementations <emph>SHOULD NOT</emph> automatically Flush changes to the indexes at the end of this operation
        /// </remarks>
        void Unindex(Triple t);

        /// <summary>
        /// Unindexes a Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <remarks>
        /// Implementations <emph>SHOULD</emph> automatically Flush changes to the indexes at the end of this operation
        /// </remarks>
        void Unindex(IGraph g);

        /// <summary>
        /// Unindexes a Dataset
        /// </summary>
        /// <param name="dataset">Dataset</param>
        /// <remarks>
        /// Implementations <emph>SHOULD</emph> automatically Flush changes to the indexes at the end of this operation
        /// </remarks>
        void Unindex(ISparqlDataset dataset);

        /// <summary>
        /// Flushes any outstanding changes to the Index
        /// </summary>
        void Flush();
    }
}
