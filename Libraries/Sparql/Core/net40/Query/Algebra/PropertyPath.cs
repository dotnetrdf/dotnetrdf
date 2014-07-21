using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;

namespace VDS.RDF.Query.Algebra
{
    public class PropertyPath
        : BaseUnaryAlgebra
    {
        public PropertyPath(IAlgebra innerAlgebra) 
            : base(innerAlgebra) {}

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

        public override bool Equals(IAlgebra other)
        {
            throw new NotImplementedException();
        }
    }
}
