using System.Collections.Generic;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Expressions.Transforms
{
    public interface IExpressionTransform
    {
        IExpression Transform(INullaryExpression expression);

        IExpression Transform(IUnaryExpression expression, IExpression transformedInnerExpression);

        IExpression Transform(IBinaryExpression expression, IExpression transformedFirstArgument, IExpression transformedSecondArgument);

        IExpression Transform(ITernayExpression expression, IExpression transformedFirstArgument, IExpression transformedSecondArgument, IExpression transformedThirdArgument);

        IExpression Transform(INAryExpression expression, IEnumerable<IExpression> transformedArguments);

        IExpression Transform(IAggregateExpression expression, IEnumerable<IExpression> transformedArguments);

        IExpression Transform(IAlgebraExpression expression, IAlgebra transformedAlgebra);
    }
}
