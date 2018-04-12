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

using VDS.RDF.Query.Aggregates;
using VDS.RDF.Query.Aggregates.Sparql;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    internal partial class ExpressionBuilder : IAggregateBuilder
    {
        private bool _distinctAggregate;

        public IDistinctAggregateBuilder Distinct => new ExpressionBuilder(Prefixes)
        {
            _distinctAggregate = true,
        };

        public AggregateExpression Sum(VariableTerm variable)
        {
            var sumAggregate = new SumAggregate(variable, _distinctAggregate);

            return new AggregateExpression(sumAggregate);
        }

        public AggregateExpression Sum(string variable)
        {
            return Sum(new VariableTerm(variable));
        }

        public AggregateExpression Sum(SparqlVariable variable)
        {
            return Sum(new VariableTerm(variable.Name));
        }

        public AggregateExpression Sum(SparqlExpression expression)
        {
            var sumAggregate = new SumAggregate(expression.Expression, _distinctAggregate);

            return new AggregateExpression(sumAggregate);
        }

        public AggregateExpression Avg(VariableTerm variable)
        {
            var aggregate = new AverageAggregate(variable, _distinctAggregate);

            return new AggregateExpression(aggregate);
        }

        public AggregateExpression Avg(string variable)
        {
            return Avg(new VariableTerm(variable));
        }

        public AggregateExpression Avg(SparqlVariable variable)
        {
            return Avg(new VariableTerm(variable.Name));
        }

        public AggregateExpression Avg(SparqlExpression expression)
        {
            var aggregate = new AverageAggregate(expression.Expression, _distinctAggregate);

            return new AggregateExpression(aggregate);
        }

        public AggregateExpression Min(VariableTerm variable)
        {
            var aggregate = new MinAggregate(variable, _distinctAggregate);

            return new AggregateExpression(aggregate);
        }

        public AggregateExpression Min(string variable)
        {
            return Min(new VariableTerm(variable));
        }

        public AggregateExpression Min(SparqlVariable variable)
        {
            return Min(new VariableTerm(variable.Name));
        }

        public AggregateExpression Min(SparqlExpression expression)
        {
            var aggregate = new MinAggregate(expression.Expression, _distinctAggregate);

            return new AggregateExpression(aggregate);
        }

        public AggregateExpression Max(VariableTerm variable)
        {
            var aggregate = new MaxAggregate(variable, _distinctAggregate);

            return new AggregateExpression(aggregate);
        }

        public AggregateExpression Max(string variable)
        {
            return Max(new VariableTerm(variable));
        }

        public AggregateExpression Max(SparqlVariable variable)
        {
            return Max(new VariableTerm(variable.Name));
        }

        public AggregateExpression Max(SparqlExpression expression)
        {
            var aggregate = new MaxAggregate(expression.Expression, _distinctAggregate);

            return new AggregateExpression(aggregate);
        }

        public AggregateExpression GroupConcat(VariableTerm variable, string separator = " ")
        {
            GroupConcatAggregate aggregate;
            if (separator != " ")
            {
                aggregate = new GroupConcatAggregate(variable, Constant(separator).Expression, _distinctAggregate);
            }
            else
            {
                aggregate = new GroupConcatAggregate(variable, _distinctAggregate);
            }

            return new AggregateExpression(aggregate);
        }

        public AggregateExpression GroupConcat(string variable, string separator = " ")
        {
            return GroupConcat(new VariableTerm(variable), separator);
        }

        public AggregateExpression GroupConcat(SparqlExpression expression, string separator = " ")
        {
            GroupConcatAggregate aggregate;
            if (separator != " ")
            {
                aggregate = new GroupConcatAggregate(expression.Expression, Constant(separator).Expression, _distinctAggregate);
            }
            else
            {
                aggregate = new GroupConcatAggregate(expression.Expression, _distinctAggregate);
            }

            return new AggregateExpression(aggregate);
        }

        public AggregateExpression Sample(VariableTerm variable)
        {
            var aggregate = new SampleAggregate(variable);

            return new AggregateExpression(aggregate);
        }

        public AggregateExpression Sample(string variable)
        {
            return Sample(new VariableTerm(variable));
        }

        public AggregateExpression Sample(SparqlExpression expression)
        {
            var aggregate = new SampleAggregate(expression.Expression);

            return new AggregateExpression(aggregate);
        }

        public AggregateExpression Count()
        {
            var aggregate = _distinctAggregate 
                ? (ISparqlAggregate)new CountAllDistinctAggregate()
                : new CountAllAggregate();

            return new AggregateExpression(aggregate);
        }

        public AggregateExpression Count(VariableTerm variable)
        {
            var aggregate = _distinctAggregate
                ? (ISparqlAggregate)new CountDistinctAggregate(variable)
                : new CountAggregate(variable);

            return new AggregateExpression(aggregate);
        }

        public AggregateExpression Count(string variable)
        {
            return Count(new VariableTerm(variable));
        }

        public AggregateExpression Count(SparqlVariable variable)
        {
            return Count(new VariableTerm(variable.Name));
        }

        public AggregateExpression Count(SparqlExpression expression)
        {
            var aggregate = _distinctAggregate
                ? (ISparqlAggregate)new CountDistinctAggregate(expression.Expression)
                : new CountAggregate(expression.Expression);

            return new AggregateExpression(aggregate);
        }
    }
}