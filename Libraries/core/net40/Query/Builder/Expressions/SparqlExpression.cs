using System.Linq;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;
using VDS.RDF.Query.Expressions.Primary;

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

        protected static BooleanExpression Gt(ISparqlExpression left, ISparqlExpression right)
        {
            return new BooleanExpression(new GreaterThanExpression(left, right));
        }

        protected static BooleanExpression Lt(ISparqlExpression left, ISparqlExpression right)
        {
            return new BooleanExpression(new LessThanExpression(left, right));
        }

        protected static BooleanExpression Ge(ISparqlExpression left, ISparqlExpression right)
        {
            return new BooleanExpression(new GreaterThanOrEqualToExpression(left, right));
        }

        protected static BooleanExpression Le(ISparqlExpression left, ISparqlExpression right)
        {
            return new BooleanExpression(new LessThanOrEqualToExpression(left, right));
        }

        protected static ConstantTerm CreateConstantTerm<T>(T value)
        {
            return new ConstantTerm(value.ToLiteral());
        }
    }
}