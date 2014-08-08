using System;
using System.Collections.Generic;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Expressions.Transforms
{
    /// <summary>
    /// An expression transform that substitutes one expression for another
    /// </summary>
    public class ExprTransformSubstitute
        : ExprTransformCopy
    {
        public ExprTransformSubstitute(IExpression find, IExpression replace)
        {
            if (find == null) throw new ArgumentNullException("find");
            if (replace == null) throw new ArgumentNullException("replace");
            if (find.Equals(replace)) throw new ArgumentException("find cannot be the same as replace");
            this.Find = find;
            this.Replace = replace;
        }

        private IExpression Find { get; set; }

        private IExpression Replace { get; set; }

        private bool ShouldReplace(IExpression expr)
        {
            return this.Find.Equals(expr);
        }

        public override IExpression Transform(IAggregateExpression expression, IEnumerable<IExpression> transformedArguments)
        {
            return ShouldReplace(expression) ? this.Replace : base.Transform(expression, transformedArguments);
        }

        public override IExpression Transform(IAlgebraExpression expression, IAlgebra transformedAlgebra)
        {
            return ShouldReplace(expression) ? this.Replace : base.Transform(expression, transformedAlgebra);
        }

        public override IExpression Transform(IBinaryExpression expression, IExpression transformedFirstArgument, IExpression transformedSecondArgument)
        {
            return ShouldReplace(expression) ? this.Replace : base.Transform(expression, transformedFirstArgument, transformedSecondArgument);
        }

        public override IExpression Transform(INAryExpression expression, IEnumerable<IExpression> transformedArguments)
        {
            return ShouldReplace(expression) ? this.Replace : base.Transform(expression, transformedArguments);
        }

        public override IExpression Transform(INullaryExpression expression)
        {
            return ShouldReplace(expression) ? this.Replace : base.Transform(expression);
        }

        public override IExpression Transform(ITernayExpression expression, IExpression transformedFirstArgument, IExpression transformedSecondArgument, IExpression transformedThirdArgument)
        {
            return ShouldReplace(expression) ? this.Replace : base.Transform(expression, transformedFirstArgument, transformedSecondArgument, transformedThirdArgument);
        }

        public override IExpression Transform(IUnaryExpression expression, IExpression transformedInnerExpression)
        {
            return ShouldReplace(expression) ? this.Replace : base.Transform(expression, transformedInnerExpression);
        }
    }
}
