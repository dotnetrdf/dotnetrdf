using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class SimpleLiteralExpression:LiteralExpression
    {
        public SimpleLiteralExpression(ISparqlExpression expression) : base(expression)
        {
        }
    }
}