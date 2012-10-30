using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    public sealed class ExpressionBuilder
    {
        private BooleanExpression _expression;

        public BooleanExpression Expression
        {
            get { return _expression; }
            internal set { _expression = value; }
        }

        public VariableTerm Variable(string variable)
        {
            return new VariableTerm(variable);
        }

        internal ConstantTerm StringConstant(string str)
        {
            return new ConstantTerm(new StringNode(null, str));
        }

        public BooleanExpression Not(BooleanExpression innerExpression)
        {
            return new BooleanExpression(new NotExpression(innerExpression.Expression));
        }
    }
}