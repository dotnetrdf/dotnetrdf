using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    public sealed class ExpressionBuilder : IExpressionBuilder
    {
        private ISparqlExpression _expression;

        public ISparqlExpression Expression
        {
            get { return _expression; }
        }

        #region Implementation of IExpressionBuilder

        public VariableTerm Variable(string variable)
        {
            return new VariableTerm(variable);
        }

        #endregion

        static internal ConstantTerm StringConstant(string str)
        {
            return new ConstantTerm(new StringNode(null, str));
        }
    }

    public static class ExpressionBuilderRegexStringExtensions
    {
        public static ISparqlExpression Regex(this IExpressionBuilder eb, ISparqlExpression text, string pattern)
        {
            return new RegexFunction(text, ExpressionBuilder.StringConstant(pattern));
        }
    }
}