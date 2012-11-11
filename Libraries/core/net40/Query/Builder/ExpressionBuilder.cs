using System;
using VDS.RDF.Parsing;
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
        private SparqlQuerySyntax _sparqlVersion;

        internal ExpressionBuilder(INamespaceMapper prefixes)
        {
            _sparqlVersion = SparqlQuerySyntax.Sparql_1_1;
            _prefixes = prefixes;
        }

        internal INamespaceMapper Prefixes
        {
            get { return _prefixes; }
        }

        public SparqlQuerySyntax SparqlVersion
        {
            get { return _sparqlVersion; }
            set { _sparqlVersion = value; }
        }

        public VariableExpression Variable(string variable)
        {
            return new VariableExpression(variable);
        }

        public TypedLiteralExpression<string> Constant(string value)
        {
            return new NumericExpression<string>(value);
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
            return new NumericExpression<DateTime>(value);
        }

        public RdfTermExpression Constant(Uri value)
        {
            return new RdfTermExpression(new ConstantTerm(new UriNode(null, value)));
        }
    }
}