using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a literal expression
    /// </summary>
#pragma warning disable 660,661
    public class LiteralExpression : RdfTermExpression
#pragma warning restore 660,661
    {
        /// <summary>
        /// Wraps the <paramref name="expression"/> as a literal expression
        /// </summary>
        public LiteralExpression(ISparqlExpression expression) : base(expression)
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

        public LiteralExpression ToSimpleLiteral()
        {
            ConstantTerm constant = (ConstantTerm) Expression;
            return new LiteralExpression(constant.Node.AsString().ToSimpleLiteral());
        }
    }
}