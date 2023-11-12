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
using VDS.RDF.LinkedPatternFragments.Hydra;

namespace VDS.RDF.LinkedPatternFragments
{
    public class Graph : RDF.Graph
    {
        private readonly IriTemplate template;

        public Graph(string baseUri)
        {
            using var tripleStore = new TripleStore(UriFactory.Create(baseUri));
            this.template = tripleStore.Metadata.Search;
            this._triples = new TripleCollection(this.template);
        }

        public override bool Assert(Triple t)
        {
            throw new NotSupportedException("This graph is read-only.");
        }

        public override bool Assert(IEnumerable<Triple> ts)
        {
            throw new NotSupportedException("This graph is read-only.");
        }

        public override void Clear()
        {
            throw new NotSupportedException("This graph is read-only.");
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

        public override IBlankNode GetBlankNode(string nodeId)
        {
            throw new NotSupportedException("This graph does not support blank nodes.");
        }

        public override void Merge(IGraph g, bool keepOriginalGraphUri)
        {
            throw new NotSupportedException("This graph is read-only.");
        }

        public override bool Retract(Triple t)
        {
            throw new NotSupportedException("This graph is read-only.");
        }

        public override bool Retract(IEnumerable<Triple> ts)
        {
            throw new NotSupportedException("This graph is read-only.");
        }
    }
}
