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
            if (this.IndexingMode == IndexingMode.Custom) throw new FullTextIndexException("Indexers deriving from BaseSimpleFullTextIndexer which use Custom IndexingMode must override the Index(Triple t) method");

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
            if (this.IndexingMode == IndexingMode.Custom) throw new FullTextIndexException("Indexers deriving from BaseSimpleFullTextIndexer which use Custom IndexingMode must override the Unindex(Triple t) method");

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
