using System;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    public sealed class ExpressionBuilder
    {
        private readonly INamespaceMapper _prefixes;
        private bool _useSparql10;

        internal ExpressionBuilder(INamespaceMapper prefixes)
        {
            _prefixes = prefixes;
        }

        public bool UseSparql10
        {
            get { return _useSparql10; }
            set { _useSparql10 = value; }
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

        public RdfTermExpression Constant(Uri value)
        {
            return new RdfTermExpression(new ConstantTerm(new UriNode(null, value)));
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
            return Str(variable.Expression);
        }

        public SimpleLiteralExpression Str(LiteralExpression literal)
        {
            return Str(literal.Expression);
        }

        public SimpleLiteralExpression Str(IriExpression iriTerm)
        {
            return Str(iriTerm.Expression);
        }

        private SimpleLiteralExpression Str(ISparqlExpression expression)
        {
            return new SimpleLiteralExpression(new StrFunction(expression));
        }

        public SimpleLiteralExpression Lang(VariableExpression variable)
        {
            return new SimpleLiteralExpression(new LangFunction(variable.Expression));
        }

        public SimpleLiteralExpression Lang(LiteralExpression literal)
        {
            return new SimpleLiteralExpression(new LangFunction(literal.Expression));
        }

        public IriExpression Datatype(VariableExpression variable)
        {
            return Datatype(variable.Expression);
        }

        public IriExpression Datatype(LiteralExpression literal)
        {
            return Datatype(literal.Expression);
        }

        private IriExpression Datatype(ISparqlExpression expression)
        {
            var dataTypeFunction = _useSparql10
                                       ? new DataTypeFunction(expression)
                                       : new DataType11Function(expression);
            return new IriExpression(dataTypeFunction);
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