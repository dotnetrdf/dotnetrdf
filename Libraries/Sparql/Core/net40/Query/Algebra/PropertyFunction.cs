using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public class PropertyFunction
        : BaseUnaryAlgebra
    {
        public PropertyFunction(IAlgebra innerAlgebra)
            : base(innerAlgebra) {}

        public override void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            throw new NotImplementedException();
        }

        public override bool Equals(IAlgebra other)
        {
            throw new NotImplementedException();
        }
    }
}
