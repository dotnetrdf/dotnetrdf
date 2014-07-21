﻿using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;

namespace VDS.RDF.Query.Algebra
{
    public class Union
        : BaseBinaryAlgebra
    {
        public Union(IAlgebra lhs, IAlgebra rhs) 
            : base(lhs, rhs) { }

        public override IEnumerable<string> ProjectedVariables
        {
            get { return this.Lhs.ProjectedVariables.Concat(this.Rhs.ProjectedVariables).Distinct(); }
        }

        public override IEnumerable<string> FixedVariables
        {
            get { return this.Lhs.FixedVariables.Intersect(this.Rhs.FixedVariables); }
        }

        public override IEnumerable<string> FloatingVariables
        {
            get { return this.ProjectedVariables.Except(this.FixedVariables); }
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
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is Union)) return false;

            Union u = (Union) other;
            return this.Lhs.Equals(u.Lhs) && this.Rhs.Equals(u.Rhs);
        }
    }
}