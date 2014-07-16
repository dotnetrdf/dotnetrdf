using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Common.Tries;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;

namespace VDS.RDF.Query.Algebra
{
    public class NamedGraph
        : IUnaryAlgebra
    {
        public NamedGraph(INode graphName, IAlgebra innerAlgebra)
        {
            if (graphName == null) throw new ArgumentNullException("graphName");
            if (innerAlgebra == null) throw new ArgumentNullException("innerAlgebra");

            this.Graph = graphName;
            this.InnerAlgebra = innerAlgebra;
        }

        public INode Graph { get; private set; }

        public bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is NamedGraph)) return false;

            NamedGraph ng = (NamedGraph) other;
            return this.Graph.Equals(ng.Graph) && this.InnerAlgebra.Equals(ng.InnerAlgebra);
        }

        public void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<ISet> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }

        public IAlgebra InnerAlgebra { get; private set; }
    }
}
