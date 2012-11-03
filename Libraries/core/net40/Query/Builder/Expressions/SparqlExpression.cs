using System.Linq;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;

namespace VDS.RDF.Query.Builder.Expressions
{
    public abstract class SparqlExpression
    {
        protected SparqlExpression(ISparqlExpression expression)
        {
            Expression = expression;
        }

        public ISparqlExpression Expression { get; protected set; }

        public BooleanExpression In(params SparqlExpression[] expressions)
        {
            var inFunction = new InFunction(Expression, expressions.Select(v => v.Expression));
            return new BooleanExpression(inFunction);
        }

        public BooleanExpression Eq(SparqlExpression rightExpression)
        {
            ISparqlExpression equalsExpression = new EqualsExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }

        protected BooleanExpression Gt(ISparqlExpression left, SparqlExpression right)
        {
            return new BooleanExpression(new GreaterThanExpression(left, right.Expression));
        }

        protected BooleanExpression Lt(ISparqlExpression left, SparqlExpression right)
        {
            return new BooleanExpression(new LessThanExpression(left, right.Expression));
        }

        protected BooleanExpression Ge(ISparqlExpression left, SparqlExpression right)
        {
            return new BooleanExpression(new GreaterThanOrEqualToExpression(left, right.Expression));
        }

        protected BooleanExpression Le(ISparqlExpression left, SparqlExpression right)
        {
            return new BooleanExpression(new LessThanOrEqualToExpression(left, right.Expression));
        }
    }
}