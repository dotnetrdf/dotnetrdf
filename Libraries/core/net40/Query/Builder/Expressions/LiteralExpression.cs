using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class LiteralExpression : RdfTermExpression
    {
        public LiteralExpression(ISparqlExpression expression) : base(expression)
        {
        }

        public static BooleanExpression operator ==(LiteralExpression left, string right)
        {
            return new BooleanExpression(new EqualsExpression(left.Expression, right.ToConstantTerm()));
        }

        public static BooleanExpression operator !=(LiteralExpression left, string right)
        {
            return !(left == right);
        }

        public static BooleanExpression operator ==(string left, LiteralExpression right)
        {
            return new BooleanExpression(new EqualsExpression(left.ToConstantTerm(), right.Expression));
        }

        public static BooleanExpression operator !=(string left, LiteralExpression right)
        {
            return !(left == right);
        }
    }
}