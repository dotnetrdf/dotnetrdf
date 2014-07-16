using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Elements
{
    public class NamedGraphElement
        : IElement
    {
        public NamedGraphElement(INode graphName, IElement element)
        {
            if (graphName == null) throw new ArgumentNullException("graphName");
            if (element == null) throw new ArgumentNullException("element");

            this.Graph = Graph;
            this.Element = element;
        }

        public INode Graph { get; private set; }

        public IElement Element { get; private set; }

        public bool Equals(IElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is NamedGraphElement)) return false;

            NamedGraphElement ng = (NamedGraphElement) other;
            return this.Graph.Equals(ng.Graph) && this.Element.Equals(ng.Element);
        }

        public void Accept(IElementVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<string> Variables
        {
            get { return this.Graph.NodeType != NodeType.Variable ? this.Element.Variables : this.Element.Variables.Concat(this.Graph.VariableName.AsEnumerable()).Distinct(); }
        }

        public IEnumerable<string> ProjectedVariables
        {
            get { return this.Graph.NodeType != NodeType.Variable ? this.Element.ProjectedVariables : this.Element.ProjectedVariables.Concat(this.Graph.VariableName.AsEnumerable()).Distinct(); }
        }
    }
}