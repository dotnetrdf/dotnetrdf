using System.Linq;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;

namespace VDS.RDF.Query.Builder.Expressions
{
    public abstract class SparqlExpression
    {
        protected SparqlExpression(ISparqlExpression expression)
        {
            Expression = expression;
        }

        public ISparqlExpression Expression { get; set; }

        public BooleanExpression In(params SparqlExpression[] expressions)
        {
            var inFunction = new InFunction(Expression, expressions.Select(v => v.Expression));
            return new BooleanExpression(inFunction);
        }

        public static BooleanExpression operator ==(SparqlExpression left, SparqlExpression right)
        {
            return new BooleanExpression(new EqualsExpression(left.Expression, right.Expression));
        }

        public static BooleanExpression operator !=(SparqlExpression left, SparqlExpression right)
        {
            return !(left == right);
        }

        protected static BooleanExpression Gt(ISparqlExpression left, SparqlExpression right)
        {
            return new BooleanExpression(new GreaterThanExpression(left, right.Expression));
        }

        protected static BooleanExpression Lt(ISparqlExpression left, SparqlExpression right)
        {
            return new BooleanExpression(new LessThanExpression(left, right.Expression));
        }

        protected static BooleanExpression Ge(ISparqlExpression left, SparqlExpression right)
        {
            return new BooleanExpression(new GreaterThanOrEqualToExpression(left, right.Expression));
        }

        protected static BooleanExpression Le(ISparqlExpression left, SparqlExpression right)
        {
            return new BooleanExpression(new LessThanOrEqualToExpression(left, right.Expression));
        }
    }
}