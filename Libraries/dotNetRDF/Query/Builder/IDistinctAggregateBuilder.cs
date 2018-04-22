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

using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for creating aggregates expressions but only those allowing DISTINCT
    /// </summary>
    public interface IDistinctAggregateBuilder
    {
        /// <summary>
        /// Creates a SUM aggregate
        /// </summary>
        AggregateExpression Sum(VariableTerm variable);

        /// <summary>
        /// Creates a SUM aggregate
        /// </summary>
        AggregateExpression Sum(string variable);

        /// <summary>
        /// Creates a SUM aggregate
        /// </summary>
        AggregateExpression Sum(SparqlVariable variable);

        /// <summary>
        /// Creates a SUM aggregate
        /// </summary>
        AggregateExpression Sum(SparqlExpression expression);

        /// <summary>
        /// Creates a AVG aggregate
        /// </summary>
        AggregateExpression Avg(VariableTerm variable);

        /// <summary>
        /// Creates a AVG aggregate
        /// </summary>
        AggregateExpression Avg(string variable);

        /// <summary>
        /// Creates a AVG aggregate
        /// </summary>
        AggregateExpression Avg(SparqlVariable variable);

        /// <summary>
        /// Creates a AVG aggregate
        /// </summary>
        AggregateExpression Avg(SparqlExpression expression);

        /// <summary>
        /// Creates a MIN aggregate
        /// </summary>
        AggregateExpression Min(VariableTerm variable);

        /// <summary>
        /// Creates a MIN aggregate
        /// </summary>
        AggregateExpression Min(string variable);

        /// <summary>
        /// Creates a MIN aggregate
        /// </summary>
        AggregateExpression Min(SparqlVariable variable);

        /// <summary>
        /// Creates a MIN aggregate
        /// </summary>
        AggregateExpression Min(SparqlExpression expression);

        /// <summary>
        /// Creates a MAX aggregate
        /// </summary>
        AggregateExpression Max(VariableTerm variable);

        /// <summary>
        /// Creates a MAX aggregate
        /// </summary>
        AggregateExpression Max(string variable);

        /// <summary>
        /// Creates a MAX aggregate
        /// </summary>
        AggregateExpression Max(SparqlVariable variable);

        /// <summary>
        /// Creates a MAX aggregate
        /// </summary>
        AggregateExpression Max(SparqlExpression expression);

        /// <summary>
        /// Creates a GROUP_CONCAT aggregate
        /// </summary>
        AggregateExpression GroupConcat(VariableTerm variable, string separator = " ");

        /// <summary>
        /// Creates a GROUP_CONCAT aggregate
        /// </summary>
        AggregateExpression GroupConcat(string variable, string separator = " ");

        /// <summary>
        /// Creates a GROUP_CONCAT aggregate
        /// </summary>
        AggregateExpression GroupConcat(SparqlExpression expression, string separator = " ");

        /// <summary>
        /// Creates a COUNT(*) aggregate
        /// </summary>
        AggregateExpression Count();

        /// <summary>
        /// Creates a COUNT aggregate
        /// </summary>
        AggregateExpression Count(VariableTerm variable);

        /// <summary>
        /// Creates a COUNT aggregate
        /// </summary>
        AggregateExpression Count(string variable);

        /// <summary>
        /// Creates a COUNT aggregate
        /// </summary>
        AggregateExpression Count(SparqlVariable variable);

        /// <summary>
        /// Creates a COUNT aggregate
        /// </summary>
        AggregateExpression Count(SparqlExpression expression);
    }
}