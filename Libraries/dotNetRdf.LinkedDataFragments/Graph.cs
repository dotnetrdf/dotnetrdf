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
using System.Collections.Generic;
using VDS.RDF.LDF.Hydra;

namespace VDS.RDF.LDF
{
    // TODO: Remove System.Linq once classes here renamed
    public class Graph : RDF.Graph
    {
        private readonly IriTemplate template;

        public Graph(string baseUri)
        {
            using var tripleStore = new TripleStore(UriFactory.Create(baseUri));
            this.template = tripleStore.Metadata.Search;
            this._triples = new TripleCollection(this.template);
        }

        public override bool Equals(IGraph g, out Dictionary<INode, INode> mapping)
        {
            if (g is Graph fragments)
            {
                if (this.template.Template == fragments.template.Template)
                {
                    mapping = new Dictionary<INode, INode>();
                    return true;
                }
            }

            return base.Equals(g, out mapping);
        }

        #region Mutation methods throw because this graph is read-only

        public override bool Assert(Triple t) => throw new NotSupportedException("This graph is read-only.");

        public override bool Assert(IEnumerable<Triple> ts) => throw new NotSupportedException("This graph is read-only.");

        public override void Clear() => throw new NotSupportedException("This graph is read-only.");

        public override void Merge(IGraph g) => throw new NotSupportedException("This graph is read-only.");

        public override void Merge(IGraph g, bool keepOriginalGraphUri) => throw new NotSupportedException("This graph is read-only.");

        public override bool Retract(Triple t) => throw new NotSupportedException("This graph is read-only.");

        public override bool Retract(IEnumerable<Triple> ts) => throw new NotSupportedException("This graph is read-only.");

        #endregion

        #region Some methods and properties short-circuit to empty due to unsupported features in LDF

        public override IEnumerable<INode> AllQuotedNodes => System.Linq.Enumerable.Empty<INode>();

        public override bool ContainsQuotedTriple(Triple t) => false;

        public override IBlankNode GetBlankNode(string nodeId) => null;

        public override IEnumerable<Triple> GetQuoted(INode n) => System.Linq.Enumerable.Empty<Triple>();

        public override IEnumerable<Triple> GetQuoted(Uri uri) => System.Linq.Enumerable.Empty<Triple>();

        public override IEnumerable<Triple> GetQuotedWithObject(Uri u) => System.Linq.Enumerable.Empty<Triple>();

        public override IEnumerable<Triple> GetQuotedWithPredicate(Uri u) => System.Linq.Enumerable.Empty<Triple>();

        public override IEnumerable<Triple> GetQuotedWithSubject(Uri u) => System.Linq.Enumerable.Empty<Triple>();

        public override IEnumerable<Triple> GetQuotedWithObject(INode n) => System.Linq.Enumerable.Empty<Triple>();

        public override IEnumerable<Triple> GetQuotedWithPredicate(INode n) => System.Linq.Enumerable.Empty<Triple>();

        public override IEnumerable<Triple> GetQuotedWithSubject(INode n) => System.Linq.Enumerable.Empty<Triple>();

        public override IEnumerable<Triple> GetQuotedWithSubjectPredicate(INode subj, INode pred) => System.Linq.Enumerable.Empty<Triple>();

        public override IEnumerable<Triple> GetQuotedWithSubjectObject(INode subj, INode obj) => System.Linq.Enumerable.Empty<Triple>();

        public override IEnumerable<Triple> GetQuotedWithPredicateObject(INode pred, INode obj) => System.Linq.Enumerable.Empty<Triple>();

        public override IEnumerable<INode> QuotedNodes => System.Linq.Enumerable.Empty<INode>();

        public override IEnumerable<Triple> QuotedTriples => System.Linq.Enumerable.Empty<Triple>();

        #endregion
    }
}
