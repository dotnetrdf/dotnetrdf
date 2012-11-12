using System;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for building SPARQL expressions
    /// </summary>
    public sealed class ExpressionBuilder
    {
        private readonly INamespaceMapper _prefixes;

        internal ExpressionBuilder(INamespaceMapper prefixes)
        {
            SparqlVersion = SparqlQuerySyntax.Sparql_1_1;
            _prefixes = prefixes;
        }

        internal INamespaceMapper Prefixes
        {
            get { return _prefixes; }
        }

        /// <summary>
        /// SPARQL syntax verions to use when creating expressions
        /// </summary>
        public SparqlQuerySyntax SparqlVersion { get; set; }

        /// <summary>
        /// Creates a SPARQL variable
        /// </summary>
        public VariableExpression Variable(string variable)
        {
            return new VariableExpression(variable);
        }

        /// <summary>
        /// Creates a string constant 
        /// </summary>
        public TypedLiteralExpression<string> Constant(string value)
        {
            return new NumericExpression<string>(value);
        }

        /// <summary>
        /// Creates a numeric constant 
        /// </summary>
        public NumericExpression<int> Constant(int value)
        {
            return new NumericExpression<int>(value);
        }

        /// <summary>
        /// Creates a numeric constant 
        /// </summary>
        public NumericExpression<decimal> Constant(decimal value)
        {
            return new NumericExpression<decimal>(value);
        }

        /// <summary>
        /// Creates a numeric constant 
        /// </summary>
        public NumericExpression<float> Constant(float value)
        {
            return new NumericExpression<float>(value);
        }

        /// <summary>
        /// Creates a numeric constant 
        /// </summary>
        public NumericExpression<double> Constant(double value)
        {
            return new NumericExpression<double>(value);
        }

        /// <summary>
        /// Creates a boolean constant 
        /// </summary>
        public TypedLiteralExpression<bool> Constant(bool value)
        {
            return new TypedLiteralExpression<bool>(value);
        }

        /// <summary>
        /// Creates a numeric constant 
        /// </summary>
        public NumericExpression<byte> Constant(byte value)
        {
            return new NumericExpression<byte>(value);
        }

        /// <summary>
        /// Creates a numeric constant 
        /// </summary>
        public NumericExpression<sbyte> Constant(sbyte value)
        {
            return new NumericExpression<sbyte>(value);
        }

        /// <summary>
        /// Creates a numeric constant 
        /// </summary>
        public NumericExpression<short> Constant(short value)
        {
            return new NumericExpression<short>(value);
        }

        /// <summary>
        /// Creates a datetime constant 
        /// </summary>
        public TypedLiteralExpression<DateTime> Constant(DateTime value)
        {
            return new NumericExpression<DateTime>(value);
        }

        /// <summary>
        /// Creates an IRI constant 
        /// </summary>
        public RdfTermExpression Constant(Uri value)
        {
            return new RdfTermExpression(new ConstantTerm(new UriNode(null, value)));
        }
    }
}