using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    class BlankNodeExpression : RdfTermExpression
    {
        public BlankNodeExpression(ISparqlExpression expression) : base(expression)
        {
        }
    }
}