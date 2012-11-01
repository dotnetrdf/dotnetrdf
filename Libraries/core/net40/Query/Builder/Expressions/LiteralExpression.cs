using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class LiteralExpression : RdfTermExpression
    {
        protected static readonly NodeFactory NodeFactory = new NodeFactory();

        public LiteralExpression(ISparqlExpression expression) : base(expression)
        {
        }

        public BooleanExpression Gt(VariableExpression rightExpression)
        {
            ISparqlExpression equalsExpression = new GreaterThanExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }

        public BooleanExpression Lt(VariableExpression rightExpression)
        {
            ISparqlExpression equalsExpression = new LessThanExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }

        public BooleanExpression Ge(VariableExpression rightExpression)
        {
            ISparqlExpression equalsExpression = new GreaterThanOrEqualToExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }

        public BooleanExpression Le(VariableExpression rightExpression)
        {
            ISparqlExpression equalsExpression = new LessThanOrEqualToExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }
    }
}