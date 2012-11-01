using System;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;

namespace VDS.RDF.Query.Builder
{
    public sealed class ExpressionBuilder
    {
        private readonly INamespaceMapper _prefixes;

        internal ExpressionBuilder(INamespaceMapper prefixes)
        {
            _prefixes = prefixes;
        }

        public VariableExpression Variable(string variable)
        {
            return new VariableExpression(variable);
        }

        public TypedLiteralExpression<string> Constant(string str)
        {
            return new StringExpression(str);
        }

        public BooleanExpression Not(BooleanExpression innerExpression)
        {
            return new BooleanExpression(new NotExpression(innerExpression.Expression));
        }

        public BooleanExpression Exists(Action<IGraphPatternBuilder> buildExistsPattern)
        {
            GraphPatternBuilder builder  = new GraphPatternBuilder(_prefixes);
            buildExistsPattern(builder);
            var existsFunction = new ExistsFunction(builder.BuildGraphPattern(), true);
            return new BooleanExpression(existsFunction);
        }
    }
}