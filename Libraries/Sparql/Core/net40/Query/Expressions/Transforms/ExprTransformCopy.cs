using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Expressions.Transforms
{
    public class ExprTransformCopy
        : IExpressionTransform
    {
        public virtual IExpression Transform(INullaryExpression expression)
        {
            return expression.Copy();
        }

        public virtual IExpression Transform(IUnaryExpression expression, IExpression transformedInnerExpression)
        {
            return expression.Copy(transformedInnerExpression);
        }

        public virtual IExpression Transform(IBinaryExpression expression, IExpression transformedFirstArgument, IExpression transformedSecondArgument)
        {
            return expression.Copy(transformedFirstArgument, transformedSecondArgument);
        }

        public virtual IExpression Transform(ITernayExpression expression, IExpression transformedFirstArgument, IExpression transformedSecondArgument, IExpression transformedThirdArgument)
        {
            return expression.Copy(transformedFirstArgument, transformedSecondArgument, transformedThirdArgument);
        }

        public virtual IExpression Transform(INAryExpression expression, IEnumerable<IExpression> transformedArguments)
        {
            return expression.Copy(transformedArguments);
        }

        public virtual IExpression Transform(IAggregateExpression expression, IEnumerable<IExpression> transformedArguments)
        {
            return expression.Copy(transformedArguments);
        }

        public virtual IExpression Transform(IAlgebraExpression expression, IAlgebra transformedAlgebra)
        {
            return expression.Copy(transformedAlgebra);
        }
    }
}
