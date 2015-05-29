/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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

using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Elements
{
    /// <summary>
    /// An element consisting only of simple triple patterns
    /// </summary>
    public class TripleBlockElement
        : IElement
    {
        public TripleBlockElement()
            : this(null) {}

        public TripleBlockElement(IEnumerable<Triple> triples)
        {
            this.Triples = triples != null ? new List<Triple>(triples) : new List<Triple>();
        }

        public IList<Triple> Triples { get; private set; }

        public bool Equals(IElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (other is TripleBlockElement)
            {
                IList<Triple> otherTriples = ((TripleBlockElement) other).Triples;
                return AreEqual(this.Triples, otherTriples);
            }
            if (other is PathBlockElement)
            {
                PathBlockElement pb = (PathBlockElement) other;
                return !pb.Paths.Any(p => p.IsPath) && AreEqual(this.Triples, pb.Paths.Select(p => p.AsTriple()).ToList());
            }
            return false;
        }

        internal static bool AreEqual(IList<Triple> triples, IList<Triple> otherTriples)
        {
            if (triples.Count != otherTriples.Count) return false;
            for (int i = 0; i < triples.Count; i++)
            {
                if (!triples[i].Equals(otherTriples[i])) return false;
            }
            return true;
        }

        public void Accept(IElementVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<string> Variables
        {
            get { return this.ProjectedVariables; }
        }

        public IEnumerable<string> ProjectedVariables
        {
            get { return this.Triples.SelectMany(t => t.Nodes).Where(n => n.NodeType == NodeType.Variable).Select(n => n.VariableName); }
        }
    }
}