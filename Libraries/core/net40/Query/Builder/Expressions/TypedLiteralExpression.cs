using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public abstract class TypedLiteralExpression<T> : LiteralExpression
    {
        protected TypedLiteralExpression(ILiteralNode expression)
            : base(new ConstantTerm(expression))
        {
        }

        protected TypedLiteralExpression(ISparqlExpression expression)
            : base(expression)
        {
        }
    }
}