using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class RdfTermExpression : SparqlExpression
    {
        public RdfTermExpression(ISparqlExpression expression) : base(expression)
        {
        }
    }
}