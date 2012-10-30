using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    public interface IExpressionBuilder
    {
        VariableTerm Variable(string variable);
    }
}