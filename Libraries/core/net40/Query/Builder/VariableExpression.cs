using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    public class VariableExpression : SparqlExpression<VariableTerm>
    {
        public VariableExpression(string variable)
            : base(new VariableTerm(variable))
        {
        }
    }
}