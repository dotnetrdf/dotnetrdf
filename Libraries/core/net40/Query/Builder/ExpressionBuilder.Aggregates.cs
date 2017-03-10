/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2017 dotNetRDF Project

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

using VDS.RDF.Query.Aggregates.Sparql;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    public partial class ExpressionBuilder
    {
        private bool _distinctAggregate;

        public IAggregateBuilder Distinct => new ExpressionBuilder(Prefixes)
        {
            _distinctAggregate = true
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

        public AggregateExpression Avg(SparqlExpression expression)
        {
            var aggregate = new AverageAggregate(expression.Expression, _distinctAggregate);

            return new AggregateExpression(aggregate);
        }
    }
}