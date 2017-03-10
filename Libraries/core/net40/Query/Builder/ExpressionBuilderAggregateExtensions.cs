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
    /// <summary>
    /// Provides methods for creating aggregates expressions
    /// </summary>
    public static class ExpressionBuilderAggregateExtensions
    {
        /// <summary>
        /// Creates a SUM aggregate
        /// </summary>
        public static AggregateExpression Sum(this ExpressionBuilder builder, VariableTerm variable)
        {
            var sumAggregate = new SumAggregate(variable);

            return new AggregateExpression(sumAggregate);
        }

        /// <summary>
        /// Creates a SUM aggregate
        /// </summary>
        public static AggregateExpression Sum(this ExpressionBuilder builder, string variable)
        {
            return Sum(builder, new VariableTerm(variable));
        }

        /// <summary>
        /// Creates a SUM aggregate
        /// </summary>
        public static AggregateExpression Sum(this ExpressionBuilder builder, SparqlExpression expression)
        {
            var sumAggregate = new SumAggregate(expression.Expression);

            return new AggregateExpression(sumAggregate);
        }

        /// <summary>
        /// Creates a AVG aggregate
        /// </summary>
        public static AggregateExpression Avg(this ExpressionBuilder builder, VariableTerm variable)
        {
            var aggregate = new AverageAggregate(variable);

            return new AggregateExpression(aggregate);
        }

        /// <summary>
        /// Creates a AVG aggregate
        /// </summary>
        public static AggregateExpression Avg(this ExpressionBuilder builder, string variable)
        {
            return Avg(builder, new VariableTerm(variable));
        }

        /// <summary>
        /// Creates a AVG aggregate
        /// </summary>
        public static AggregateExpression Avg(this ExpressionBuilder builder, SparqlExpression expression)
        {
            var aggregate = new AverageAggregate(expression.Expression);

            return new AggregateExpression(aggregate);
        }
    }
}