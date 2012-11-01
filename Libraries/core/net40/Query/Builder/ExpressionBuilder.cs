using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Conditional;

namespace VDS.RDF.Query.Builder
{
    public sealed class ExpressionBuilder
    {
        public VariableExpression Variable(string variable)
        {
            return new VariableExpression(variable);
        }

        public TypedLiteralExpression<string> Constant(string str)
        {
            return new StringExpression(str);
        }

        public BooleanExpression Not(BooleanExpression innerExpression)
        {
            return new BooleanExpression(new NotExpression(innerExpression.Expression));
        }
    }
}