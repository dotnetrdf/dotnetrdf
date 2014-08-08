using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Expressions.Transforms
{
    /// <summary>
    /// An expression transform that substitutes one expression for another
    /// </summary>
    public class ExprTransformMultipleSubstitute
        : ExprTransformCopy
    {
        public ExprTransformMultipleSubstitute(IDictionary<IExpression, IExpression> substitutions)
        {
            if (substitutions == null) throw new ArgumentNullException("substitutions");
            if (substitutions.Any(kvp => kvp.Value == null)) throw new ArgumentNullException("substitutions", "No substitution replace may be null");
            if (substitutions.Any(kvp => kvp.Key.Equals(kvp.Value))) throw new ArgumentException("No substitutions may have same find and replace", "substitutions");
            this.Substitutions = substitutions;
        }

        private IDictionary<IExpression, IExpression> Substitutions { get; set; }

        private IExpression ShouldReplace(IExpression expr)
        {
            IExpression replace;
            this.Substitutions.TryGetValue(expr, out replace);
            return replace;
        }

        public override IExpression Transform(IAggregateExpression expression, IEnumerable<IExpression> transformedArguments)
        {
            return ShouldReplace(expression) ?? base.Transform(expression, transformedArguments);
        }

        public override IExpression Transform(IAlgebraExpression expression, IAlgebra transformedAlgebra)
        {
            return ShouldReplace(expression) ?? base.Transform(expression, transformedAlgebra);
        }

        public override IExpression Transform(IBinaryExpression expression, IExpression transformedFirstArgument, IExpression transformedSecondArgument)
        {
            return ShouldReplace(expression) ?? base.Transform(expression, transformedFirstArgument, transformedSecondArgument);
        }

        public override IExpression Transform(INAryExpression expression, IEnumerable<IExpression> transformedArguments)
        {
            return ShouldReplace(expression) ?? base.Transform(expression, transformedArguments);
        }

        public override IExpression Transform(INullaryExpression expression)
        {
            return ShouldReplace(expression) ?? base.Transform(expression);
        }

        public override IExpression Transform(ITernayExpression expression, IExpression transformedFirstArgument, IExpression transformedSecondArgument, IExpression transformedThirdArgument)
        {
            return ShouldReplace(expression) ?? base.Transform(expression, transformedFirstArgument, transformedSecondArgument, transformedThirdArgument);
        }

        public override IExpression Transform(IUnaryExpression expression, IExpression transformedInnerExpression)
        {
            return ShouldReplace(expression) ?? base.Transform(expression, transformedInnerExpression);
        }
    }
}
