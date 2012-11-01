using System;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;

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

        public StringExpression Constant(string value)
        {
            return new StringExpression(value);
        }

        public NumericExpression<int> Constant(int value)
        {
            return new NumericExpression<int>(value);
        }

        public NumericExpression<decimal> Constant(decimal value)
        {
            return new NumericExpression<decimal>(value);
        }

        public NumericExpression<float> Constant(float value)
        {
            return new NumericExpression<float>(value);
        }

        public NumericExpression<double> Constant(double value)
        {
            return new NumericExpression<double>(value);
        }

        public NumericExpression<bool> Constant(bool value)
        {
            return new NumericExpression<bool>(value);
        }

        public NumericExpression<byte> Constant(byte value)
        {
            return new NumericExpression<byte>(value);
        }

        public NumericExpression<sbyte> Constant(sbyte value)
        {
            return new NumericExpression<sbyte>(value);
        }

        public NumericExpression<short> Constant(short value)
        {
            return new NumericExpression<short>(value);
        }

        public TypedLiteralExpression<DateTime> Constant(DateTime value)
        {
            return new DateTimeExpression(value);
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

        public BooleanExpression SameTerm(SparqlExpression left, SparqlExpression right)
        {
            var sameTerm = new SameTermFunction(left.Expression, right.Expression);
            return new BooleanExpression(sameTerm);
        }

        public BooleanExpression IsIRI(SparqlExpression term)
        {
            var isIri = new IsIriFunction(term.Expression);
            return new BooleanExpression(isIri);
        }

        public BooleanExpression IsBlank(SparqlExpression term)
        {
            var isBlank = new IsBlankFunction(term.Expression);
            return new BooleanExpression(isBlank);
        }

        public BooleanExpression IsLiteral(SparqlExpression term)
        {
            var isLiteral = new IsLiteralFunction(term.Expression);
            return new BooleanExpression(isLiteral);
        }

        public BooleanExpression IsNumeric(SparqlExpression term)
        {
            var isNumeric = new IsNumericFunction(term.Expression);
            return new BooleanExpression(isNumeric);
        }

        public SimpleLiteralExpression Str(VariableExpression variable)
        {
            return new SimpleLiteralExpression(variable.Expression);
        }

        public SimpleLiteralExpression Str(LiteralExpression literal)
        {
            return new SimpleLiteralExpression(literal.Expression);
        }

        public SimpleLiteralExpression Str(IriExpression iriTerm)
        {
            return new SimpleLiteralExpression(iriTerm.Expression);
        }

        public SimpleLiteralExpression Lang(VariableExpression variable)
        {
            return new SimpleLiteralExpression(variable.Expression);
        }

        public SimpleLiteralExpression Lang(LiteralExpression literal)
        {
            return new SimpleLiteralExpression(literal.Expression);
        }

        public IriExpression Datatype(VariableExpression variable)
        {
            return new IriExpression(variable.Expression);
        }

        public IriExpression Datatype(LiteralExpression literal)
        {
            return new IriExpression(literal.Expression);
        }

        public BlankNodeExpression BNode()
        {
            return new BlankNodeExpression(new BNodeFunction());
        }

        public BlankNodeExpression BNode(SimpleLiteralExpression simpleLiteral)
        {
            return new BlankNodeExpression(new BNodeFunction(simpleLiteral.Expression));
        }

        public BlankNodeExpression BNode(StringExpression stringLiteral)
        {
            return new BlankNodeExpression(new BNodeFunction(stringLiteral.Expression));
        }
    }
}