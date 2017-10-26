/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Linq;
using Xunit;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Functions.XPath.Cast;

namespace VDS.RDF.Query.Builder.Expressions
{

    public class SparqlCastBuilderTests
    {
        private SparqlCastBuilder _castCall;
        private SparqlExpression _variable;

        public SparqlCastBuilderTests()
        {
            _variable = new VariableExpression("var");
            _castCall = new SparqlCastBuilder(_variable);
        }

        [Fact]
        public void ShouldAllowCastingAsInteger()
        {
            // when
            NumericExpression<int> cast = _castCall.AsInteger();

            // then
            Assert.True(cast.Expression is IntegerCast);
            Assert.Same(_variable.Expression, cast.Expression.Arguments.ElementAt(0));
        }

        [Fact]
        public void ShouldAllowCastingAsFloat()
        {
            // when
            NumericExpression<float> cast = _castCall.AsFloat();

            // then
            Assert.True(cast.Expression is FloatCast);
            Assert.Same(_variable.Expression, cast.Expression.Arguments.ElementAt(0));
        }

        [Fact]
        public void ShouldAllowCastingAsDateTime()
        {
            // when
            LiteralExpression cast = _castCall.AsDateTime();

            // then
            Assert.True(cast.Expression is DateTimeCast);
            Assert.Same(_variable.Expression, cast.Expression.Arguments.ElementAt(0));
        }

        [Fact]
        public void ShouldAllowCastingAsDecimal()
        {
            // when
            NumericExpression<decimal> cast = _castCall.AsDecimal();

            // then
            Assert.True(cast.Expression is DecimalCast);
            Assert.Same(_variable.Expression, cast.Expression.Arguments.ElementAt(0));
        }

        [Fact]
        public void ShouldAllowCastingAsDouble()
        {
            // when
            NumericExpression<double> cast = _castCall.AsDouble();

            // then
            Assert.True(cast.Expression is DoubleCast);
            Assert.Same(_variable.Expression, cast.Expression.Arguments.ElementAt(0));
        }

        [Fact]
        public void ShouldAllowCastingAsString()
        {
            // when
            LiteralExpression cast = _castCall.AsString();

            // then
            Assert.True(cast.Expression is StringCast);
            Assert.Same(_variable.Expression, cast.Expression.Arguments.ElementAt(0));
        }

        [Fact]
        public void ShouldAllowCastingAsBoolean()
        {
            // when
            BooleanExpression cast = _castCall.AsBoolean();

            // then
            Assert.True(cast.Expression is BooleanCast);
            Assert.Same(_variable.Expression, cast.Expression.Arguments.ElementAt(0));
        }
    }
}