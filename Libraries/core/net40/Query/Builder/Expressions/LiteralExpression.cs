using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class LiteralExpression : RdfTermExpression
    {
        protected static readonly NodeFactory NodeFactory = new NodeFactory();

        public LiteralExpression(ISparqlExpression expression) : base(expression)
        {
        }
    }
}