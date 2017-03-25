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
using System.Linq;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;

namespace VDS.RDF.Query.Builder
{
    internal partial class ExpressionBuilder
    {
        public BooleanExpression Bound(VariableExpression var)
        {
            return new BooleanExpression(new BoundFunction(var.Expression));
        }

        public BooleanExpression Bound(string var)
        {
            return Bound(Variable(var));
        }

        public IfThenPart If(BooleanExpression ifExpression)
        {
            return new IfThenPart(ifExpression.Expression);
        }

        public IfThenPart If(VariableExpression ifExpression)
        {
            return new IfThenPart(ifExpression.Expression);
        }

        public RdfTermExpression Coalesce(params SparqlExpression[] expressions)
        {
            var coalesce = new CoalesceFunction(expressions.Select(e => e.Expression));
            return new RdfTermExpression(coalesce);
        }

        public BooleanExpression Exists(Action<IGraphPatternBuilder> buildExistsPattern)
        {
            GraphPatternBuilder builder = new GraphPatternBuilder();
            buildExistsPattern(builder);
            var existsFunction = new ExistsFunction(builder.BuildGraphPattern(Prefixes), true);
            return new BooleanExpression(existsFunction);
        }

        public BooleanExpression SameTerm(SparqlExpression left, SparqlExpression right)
        {
            var sameTerm = new SameTermFunction(left.Expression, right.Expression);
            return new BooleanExpression(sameTerm);
        }

        public BooleanExpression SameTerm(string left, SparqlExpression right)
        {
            return SameTerm(Variable(left), right);
        }

        public BooleanExpression SameTerm(SparqlExpression left, string right)
        {
            return SameTerm(left, Variable(right));
        }

        public BooleanExpression SameTerm(string left, string right)
        {
            return SameTerm(Variable(left), Variable(right));
        }
    }

    /// <summary>
    /// Provides methods to supply the "then" expression for the IF function call
    /// </summary>
    public sealed class IfThenPart
    {
        private readonly ISparqlExpression _ifExpression;

        internal IfThenPart(ISparqlExpression ifExpression)
        {
            _ifExpression = ifExpression;
        }

        /// <summary>
        /// Sets the second parameter of the IF function call
        /// </summary>
        public IfElsePart Then(SparqlExpression thenExpression)
        {
            return new IfElsePart(_ifExpression, thenExpression.Expression);
        }
    }

    /// <summary>
    /// Provides methods to supply the "else" expression for the IF function call
    /// </summary>
    public sealed class IfElsePart
    {
        private readonly ISparqlExpression _ifExpression;
        private readonly ISparqlExpression _thenExpression;

        internal IfElsePart(ISparqlExpression ifExpression, ISparqlExpression thenExpression)
        {
            _ifExpression = ifExpression;
            _thenExpression = thenExpression;
        }

        /// <summary>
        /// Sets the third parameter of the IF function call
        /// </summary>
        public RdfTermExpression Else(SparqlExpression elseExpression)
        {
            var ifElseFunc = new IfElseFunction(_ifExpression, _thenExpression, elseExpression.Expression);
            return new RdfTermExpression(ifElseFunc);
        }
    }
}