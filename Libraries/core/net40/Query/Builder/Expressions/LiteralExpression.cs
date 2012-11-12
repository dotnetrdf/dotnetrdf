using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a literal expression
    /// </summary>
    public class LiteralExpression : RdfTermExpression
    {
        internal LiteralExpression(ISparqlExpression expression) : base(expression)
        {
        }

#pragma warning disable 1591
        public static BooleanExpression operator ==(LiteralExpression left, string right)
        {
            return new BooleanExpression(new EqualsExpression(left.Expression, right.ToConstantTerm()));
        }

        public static BooleanExpression operator !=(LiteralExpression left, string right)
        {
            return new BooleanExpression(new NotEqualsExpression(left.Expression, right.ToConstantTerm()));
        }

        public static BooleanExpression operator ==(string left, LiteralExpression right)
        {
            return new BooleanExpression(new EqualsExpression(left.ToConstantTerm(), right.Expression));
        }

        public static BooleanExpression operator !=(string left, LiteralExpression right)
        {
            return new BooleanExpression(new NotEqualsExpression(left.ToConstantTerm(), right.Expression));
#pragma warning restore 1591
        }
    }
}