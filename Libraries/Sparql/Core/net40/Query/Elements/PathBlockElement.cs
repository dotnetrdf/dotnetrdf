using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Graphs;
using VDS.RDF.Query.Paths;

namespace VDS.RDF.Query.Elements
{
    public class PathBlockElement
        : IElement
    {
        public PathBlockElement(IEnumerable<TriplePath> paths)
        {
            this.Paths = paths != null ? new List<TriplePath>(paths) : new List<TriplePath>();
        }

        public IList<TriplePath> Paths { get; private set; }

        public bool Equals(IElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (other is TripleBlockElement)
            {
                IList<Triple> otherTriples = ((TripleBlockElement)other).Triples;
                if (this.Paths.Any(p => p.IsPath)) return false;
                return TripleBlockElement.AreEqual(this.Paths.Select(p => p.AsTriple()).ToList(), otherTriples);
            }
            if (other is PathBlockElement)
            {
                PathBlockElement pb = (PathBlockElement)other;
                if (this.Paths.Count != pb.Paths.Count) return false;
                for (int i = 0; i < this.Paths.Count; i++)
                {
                    if (!this.Paths[i].Equals(pb.Paths[i])) return false;
                }
                return true;
            }
            return false;
        }

        public void Accept(IElementVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<string> Variables
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<string> ProjectedVariables
        {
            get { throw new NotImplementedException(); }
        }
    }
}
