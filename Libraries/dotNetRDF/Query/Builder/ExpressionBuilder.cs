/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    internal partial class ExpressionBuilder : IExpressionBuilder
    {
        internal ExpressionBuilder(INamespaceMapper prefixes)
        {
            Prefixes = prefixes;
            SparqlVersion = SparqlQuerySyntax.Sparql_1_1;
        }

        public SparqlQuerySyntax SparqlVersion { get; set; }

        private INamespaceMapper Prefixes { get; }

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

        public TypedLiteralExpression<bool> Constant(bool value)
        {
            return new TypedLiteralExpression<bool>(value);
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

        public VariableExpression Variable(SparqlVariable variable)
        {
            return new VariableExpression(variable.Name);
        }

        public RdfTermExpression Constant(Uri value)
        {
            return new RdfTermExpression(new ConstantTerm(new UriNode(null, value)));
        }

        public SparqlCastBuilder Cast(SparqlExpression castedExpression)
        {
            return new SparqlCastBuilder(castedExpression);
        }
    }
}