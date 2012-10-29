using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder
{
    internal class ExpressionBuilder : IExpressionBuilder
    {
        private ISparqlExpression _expression;

        public ISparqlExpression Expression
        {
            get { return _expression; }
        }

        public void Regex(string regularExpression, string regexPattern)
        {
        }
    }
}