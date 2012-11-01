using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    class FloatExpression : NumericExpression<float>
    {
        public FloatExpression(ISparqlExpression expression)
            : base(expression)
        {
        }
    }
}