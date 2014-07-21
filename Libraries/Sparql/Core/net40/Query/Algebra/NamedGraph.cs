using System;
using System.Collections.Generic;
using VDS.RDF.Collections;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;

namespace VDS.RDF.Query.Algebra
{
    public class NamedGraph
        : BaseUnaryAlgebra
    {
        public NamedGraph(INode graphName, IAlgebra innerAlgebra)
            : base(innerAlgebra)
        {
            if (graphName == null) throw new ArgumentNullException("graphName");
            this.Graph = graphName;
        }

        public INode Graph { get; private set; }

        public override IEnumerable<string> FixedVariables
        {
            get
            {
                if (this.Graph.NodeType != NodeType.Variable) return base.FixedVariables;
                return base.FixedVariables.AddDistinct(this.Graph.VariableName);
            }
        }

        public override IEnumerable<string> FloatingVariables
        {
            get
            {
                if (this.Graph.NodeType != NodeType.Variable) return base.FloatingVariables;
                return base.FloatingVariables.OmitAll(this.Graph.VariableName);
            }
        }

        public override IEnumerable<string> ProjectedVariables
        {
            get
            {
                if (this.Graph.NodeType != NodeType.Variable) return base.ProjectedVariables;
                return base.ProjectedVariables.AddDistinct(this.Graph.VariableName);
            }
        }

        public override bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is NamedGraph)) return false;

            NamedGraph ng = (NamedGraph) other;
            return this.Graph.Equals(ng.Graph) && this.InnerAlgebra.Equals(ng.InnerAlgebra);
        }

        public override void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }
    }
}
