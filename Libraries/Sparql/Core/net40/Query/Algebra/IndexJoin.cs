using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;

namespace VDS.RDF.Query.Algebra
{
    public class IndexJoin
        : BaseBinaryAlgebra
    {
        public IndexJoin(IAlgebra lhs, IAlgebra rhs) 
            : base(lhs, rhs) {}

        public override IEnumerable<string> ProjectedVariables
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<string> FixedVariables
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<string> FloatingVariables
        {
            get { throw new NotImplementedException(); }
        }

        public override void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override IEnumerable<ISet> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }

        public override bool Equals(IAlgebra other)
        {
            throw new NotImplementedException();
        }
    }
}
