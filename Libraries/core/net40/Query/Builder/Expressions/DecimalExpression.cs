using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    class DecimalExpression : NumericExpression<decimal>
    {
        public DecimalExpression(ISparqlExpression expression)
            : base(expression)
        {
        }
    }
}