using System;
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
            get
            {
                HashSet<String> vars = new HashSet<string>();
                foreach (Triple t in this.Triples)
                {
                    if (t.Subject.NodeType == NodeType.Variable) vars.Add(t.Subject.VariableName);
                    if (t.Subject.NodeType == NodeType.Blank) vars.Add(t.Subject.AnonID.ToString());
                    if (t.Predicate.NodeType == NodeType.Variable) vars.Add(t.Predicate.VariableName);
                    if (t.Predicate.NodeType == NodeType.Blank) vars.Add(t.Predicate.AnonID.ToString());
                    if (t.Object.NodeType == NodeType.Variable) vars.Add(t.Object.VariableName);
                    if (t.Object.NodeType == NodeType.Blank) vars.Add(t.Object.AnonID.ToString());
                }
                return vars;
            }
        }

        public IEnumerable<string> ProjectedVariables
        {
            get
            {
                HashSet<String> vars = new HashSet<string>();
                foreach (Triple t in this.Triples)
                {
                    if (t.Subject.NodeType == NodeType.Variable) vars.Add(t.Subject.VariableName);
                    if (t.Predicate.NodeType == NodeType.Variable) vars.Add(t.Predicate.VariableName);
                    if (t.Object.NodeType == NodeType.Variable) vars.Add(t.Object.VariableName);
                }
                return vars;
            }
        }
    }
}