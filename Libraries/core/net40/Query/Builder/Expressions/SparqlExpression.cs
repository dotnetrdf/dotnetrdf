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
    }
}