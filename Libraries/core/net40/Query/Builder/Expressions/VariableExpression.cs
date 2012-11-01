using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class VariableExpression : SparqlExpression
    {
        public VariableExpression(string variable)
            : base(new VariableTerm(variable))
        {
        }

        public new VariableTerm Expression
        {
            get
            {
                return (VariableTerm)base.Expression;
            }
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

        public BooleanExpression Gt(LiteralExpression right)
        {
            return Gt(Expression, right);
        }

        public BooleanExpression Lt(LiteralExpression right)
        {
            return Lt(Expression, right);
        }

        public BooleanExpression Ge(LiteralExpression right)
        {
            return Ge(Expression, right);
        }

        public BooleanExpression Le(LiteralExpression right)
        {
            return Le(Expression, right);
        }
    }
}