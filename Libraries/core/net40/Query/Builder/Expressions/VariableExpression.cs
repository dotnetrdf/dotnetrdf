using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class VariableExpression : TypedSparqlExpression<VariableTerm>
    {
        public VariableExpression(string variable)
            : base(new VariableTerm(variable))
        {
        }
    }
}