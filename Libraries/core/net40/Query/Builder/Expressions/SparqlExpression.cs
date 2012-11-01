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

        public BooleanExpression Gt(SparqlExpression rightExpression)
        {
            ISparqlExpression equalsExpression = new GreaterThanExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }

        public BooleanExpression Gt(RdfTermExpression rightExpression)
        {
            ISparqlExpression equalsExpression = new GreaterThanExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }

        public BooleanExpression Lt(SparqlExpression rightExpression)
        {
            ISparqlExpression equalsExpression = new LessThanExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }

        public BooleanExpression Ge(SparqlExpression rightExpression)
        {
            ISparqlExpression equalsExpression = new GreaterThanOrEqualToExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }

        public BooleanExpression Le(SparqlExpression rightExpression)
        {
            ISparqlExpression equalsExpression = new LessThanOrEqualToExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }
    }
}