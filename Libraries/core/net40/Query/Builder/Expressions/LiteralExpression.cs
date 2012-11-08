using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class LiteralExpression : RdfTermExpression
    {
        protected internal static readonly NodeFactory NodeFactory = new NodeFactory();

        public LiteralExpression(ISparqlExpression expression) : base(expression)
        {
        }

        public static BooleanExpression operator ==(LiteralExpression left, string right)
        {
            return new BooleanExpression(new EqualsExpression(left.Expression, new ConstantTerm(right.ToLiteral(NodeFactory))));
        }

        public static BooleanExpression operator !=(LiteralExpression left, string right)
        {
            return !(left == right);
        }

        public static BooleanExpression operator ==(string left, LiteralExpression right)
        {
            return new BooleanExpression(new EqualsExpression(new ConstantTerm(left.ToLiteral(NodeFactory)), right.Expression));
        }

        public static BooleanExpression operator !=(string left, LiteralExpression right)
        {
            return !(left == right);
        }
    }
}