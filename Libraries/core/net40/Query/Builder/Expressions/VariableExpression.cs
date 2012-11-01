using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class VariableExpression : SparqlExpression
    {
        public VariableExpression(string variable)
            : base(new VariableTerm(variable))
        {
        }

        public new VariableTerm Expression
        {
            get
            {
                return (VariableTerm)base.Expression;
            }
        }
    }
}