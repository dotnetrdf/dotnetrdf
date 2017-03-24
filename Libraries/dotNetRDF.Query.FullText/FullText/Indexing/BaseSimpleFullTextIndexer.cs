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

namespace VDS.RDF.Query.FullText.Indexing
{
    /// <summary>
    /// Abstract Implementation of a simple Full Text Indexer which simply indexes the full text of literal objects and associates a specific Node with that full text
    /// </summary>
    public abstract class BaseSimpleFullTextIndexer
        : BaseFullTextIndexer
    {
        /// <summary>
        /// Indexes a Triple
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        protected override void Index(String graphUri, Triple t)
        {
            if (this.IndexingMode == IndexingMode.Custom) throw new FullTextIndexException("Indexers deriving from BaseSimpleFullTextIndexer which use Custom IndexingMode must override the Index(String graphUri, Triple t) method");

            if (t.Object.NodeType == NodeType.Literal)
            {
                switch (this.IndexingMode)
                {
                    case IndexingMode.Predicates:
                        this.Index(graphUri, t.Predicate, ((ILiteralNode)t.Object).Value);
                        break;
                    case IndexingMode.Objects:
                        this.Index(graphUri, t.Object, ((ILiteralNode)t.Object).Value);
                        break;
                    case IndexingMode.Subjects:
                        this.Index(graphUri, t.Subject, ((ILiteralNode)t.Object).Value);
                        break;

                    default:
                        throw new FullTextQueryException("Indexers deriving from BaseSimpleFullTextIndexer can only use Subjects, Predicates or Objects indexing mode");
                }
            }
        }

        /// <summary>
        /// Abstract method that derived classes must implement to do the actual indexing of full text and node pairs
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="n">Node</param>
        /// <param name="text">Full Text</param>
        protected abstract void Index(String graphUri, INode n, String text);

        /// <summary>
        /// Unindexes a Triple
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        protected override void Unindex(String graphUri, Triple t)
        {
            if (this.IndexingMode == IndexingMode.Custom) throw new FullTextIndexException("Indexers deriving from BaseSimpleFullTextIndexer which use Custom IndexingMode must override the Unindex(String graphUri, Triple t) method");

            if (t.Object.NodeType == NodeType.Literal)
            {
                switch (this.IndexingMode)
                {
                    case IndexingMode.Predicates:
                        this.Unindex(graphUri, t.Predicate, ((ILiteralNode)t.Object).Value);
                        break;
                    case IndexingMode.Objects:
                        this.Unindex(graphUri, t.Object, ((ILiteralNode)t.Object).Value);
                        break;
                    case IndexingMode.Subjects:
                        this.Unindex(graphUri, t.Subject, ((ILiteralNode)t.Object).Value);
                        break;

                    default:
                        throw new FullTextIndexException("Indexers deriving from BaseSimpleFullTextIndexer can only use Subjects, Predicates or Objects indexing mode");
                }
            }
        }

        /// <summary>
        /// Abstract method that derived classes must implement to do the actual unindexing of full text and nodes
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="n">Node to index</param>
        /// <param name="text">Full Text to associate with the Node</param>
        protected abstract void Unindex(String graphUri, INode n, String text);
    }
}
