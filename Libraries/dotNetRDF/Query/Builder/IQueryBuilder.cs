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
using VDS.RDF.Query.Builder.Expressions;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Interface for building SPARQL queries 
    /// </summary>
    public interface IQueryBuilder
    {
        /// <summary>
        /// Gets the query type of the generated SPARQL query.
        /// </summary>
        SparqlQueryType QueryType { get; }
        /// <summary>
        /// Gets the builder associated with the root graph pattern.
        /// </summary>
        [Obsolete("Please use Root property")]
        GraphPatternBuilder RootGraphPatternBuilder { get; }
        /// <summary>
        /// Gets the builder associated with the root graph pattern.
        /// </summary>
        IGraphPatternBuilder Root { get; }
        /// <summary>
        /// Gets the prefix manager, which allows adding prefixes to the query or graph pattern
        /// </summary>
        INamespaceMapper Prefixes { get; set; }
        /// <summary>
        /// Applies a LIMIT
        /// </summary>
        /// <param name="limit">Limit value. Pass negative to disable LIMIT</param>
        IQueryBuilder Limit(int limit);
        /// <summary>
        /// Applies an OFFSET
        /// </summary>
        IQueryBuilder Offset(int offset);
        /// <summary>
        /// Adds ascending ordering by a variable to the query
        /// </summary>
        IQueryBuilder OrderBy(SparqlVariable variable);
        /// <summary>
        /// Adds ascending ordering by a variable to the query
        /// </summary>
        IQueryBuilder OrderBy(string variableName);
        /// <summary>
        /// Adds descending ordering by a variable to the query
        /// </summary>
        IQueryBuilder OrderByDescending(SparqlVariable variable);
        /// <summary>
        /// Adds descending ordering by a variable to the query
        /// </summary>
        IQueryBuilder OrderByDescending(string variableName);
        /// <summary>
        /// Adds ascending ordering by an expression to the query
        /// </summary>
        IQueryBuilder OrderBy(Func<IExpressionBuilder, SparqlExpression> buildOrderExpression);
        /// <summary>
        /// Adds descending ordering by an expression to the query
        /// </summary>
        IQueryBuilder OrderByDescending(Func<IExpressionBuilder, SparqlExpression> buildOrderExpression);
        /// <summary>
        /// Adds a GROUP BY clause to the query.
        /// </summary>
        IQueryBuilder GroupBy(SparqlVariable variable);
        /// <summary>
        /// Adds a GROUP BY clause to the query.
        /// </summary>
        IQueryBuilder GroupBy(string variableName);
        /// <summary>
        /// Adds a GROUP BY clause to the query.
        /// </summary>
        IQueryBuilder GroupBy(Func<INonAggregateExpressionBuilder, SparqlExpression> buildGroupingExpression);
        /// <summary>
        /// Adds a HAVING clause to the query.
        /// </summary>
        IQueryBuilder Having(Func<IExpressionBuilder, BooleanExpression> buildHavingConstraint);
        /// <summary>
        /// Builds and returns a <see cref="SparqlQuery"/>
        /// </summary>
        SparqlQuery BuildQuery();
        /// <summary>
        /// Adds a BIND variable assignment to the root graph pattern
        /// </summary>
        IAssignmentVariableNamePart<IQueryBuilder> Bind(Func<INonAggregateExpressionBuilder, SparqlExpression> buildAssignmentExpression);
        /// <summary>
        /// Adds a VALUES inline data block to the entire query (outside graph patterns)
        /// </summary>
        IInlineDataBuilder InlineDataOverQuery(params string[] variables);
    }
}