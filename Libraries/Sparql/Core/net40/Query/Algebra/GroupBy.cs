using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public class GroupBy
        : BaseUnaryAlgebra
    {
        public GroupBy(IAlgebra innerAlgebra) 
            : base(innerAlgebra) {}

        public override IAlgebra Copy(IAlgebra innerAlgebra)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> ProjectedVariables
        {
            get { return base.ProjectedVariables; }
        }

        public override IEnumerable<string> FixedVariables
        {
            get { return base.FixedVariables; }
        }

        public override IEnumerable<string> FloatingVariables
        {
            get { return base.FloatingVariables; }
        }

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
            throw new NotImplementedException();
        }

        public override bool Equals(IAlgebra other)
        {
            throw new NotImplementedException();
        }
    }
}
