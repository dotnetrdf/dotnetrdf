using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;

namespace VDS.RDF.Query.Builder
{
    public static class ExpressionBuilderRegexStringExtensions
    {
        public static BooleanExpression Regex(this ExpressionBuilder eb, ISparqlExpression text, string pattern)
        {
            return new BooleanExpression(new RegexFunction(text, eb.StringConstant(pattern)));
        }
    }
}