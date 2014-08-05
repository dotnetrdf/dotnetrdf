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