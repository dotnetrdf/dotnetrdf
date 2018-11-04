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
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for building graph patterns
    /// </summary>
    public interface IGraphPatternBuilder
    {
        /// <summary>
        /// Adds another child graph pattern builder.
        /// </summary>
        IGraphPatternBuilder Group(GraphPatternBuilder groupBuilder);
        /// <summary>
        /// Adds another child graph pattern builder.
        /// </summary>
        IGraphPatternBuilder Group(Action<IGraphPatternBuilder> buildTriplePatterns);
        /// <summary>
        /// Creates a UNION of multiple graph patterns. If <paramref name="unionedGraphPatternBuilders"/> is null or empty,
        /// acts as a call to the <see cref="Child(GraphPatternBuilder)"/> method.
        /// </summary>
        IGraphPatternBuilder Union(GraphPatternBuilder firstGraphPattern, params GraphPatternBuilder[] unionedGraphPatternBuilders);
        /// <summary>
        /// Creates a UNION of multiple graph patterns. If <paramref name="unionedGraphPatternBuilders"/> is null or empty,
        /// acts as a call to the <see cref="Child(Action{IGraphPatternBuilder})"/> method.
        /// </summary>
        IGraphPatternBuilder Union(Action<IGraphPatternBuilder> buildFirstGraphPattern, params Action<IGraphPatternBuilder>[] unionedGraphPatternBuilders);
        /// <summary>
        /// Adds triple patterns to the SPARQL query or graph pattern
        /// </summary>
        IGraphPatternBuilder Where(params ITriplePattern[] triplePatterns);
        /// <summary>
        /// Adds triple patterns to the SPARQL query or graph pattern
        /// </summary>
        IGraphPatternBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns);
        /// <summary>
        /// Adds an OPTIONAL graph pattern to the SPARQL query or graph pattern
        /// </summary>
        IGraphPatternBuilder Optional(Action<IGraphPatternBuilder> buildGraphPattern);
        /// <summary>
        /// Adds a FILTER to the SPARQL query or graph pattern
        /// </summary>
        IGraphPatternBuilder Filter(Func<INonAggregateExpressionBuilder, BooleanExpression> expr);
        /// <summary>
        /// Adds a FILTER expression to the SPARQL query or graph pattern
        /// </summary>
        IGraphPatternBuilder Filter(ISparqlExpression expr);
        /// <summary>
        /// Adds a MINUS graph pattern to the SPARQL query or graph pattern
        /// </summary>
        IGraphPatternBuilder Minus(Action<IGraphPatternBuilder> buildGraphPattern);
        /// <summary>
        /// Adds a GRAPH graph pattern to the graph pattern
        /// </summary>
        IGraphPatternBuilder Graph(Uri graphUri, Action<IGraphPatternBuilder> buildGraphPattern);
        /// <summary>
        /// Adds a GRAPH graph pattern to the graph pattern
        /// </summary>
        IGraphPatternBuilder Graph(string graphVariable, Action<IGraphPatternBuilder> buildGraphPattern);
        /// <summary>
        /// Adds a SERVICE graph pattern to the graph pattern
        /// </summary>
        IGraphPatternBuilder Service(Uri serviceUri, Action<IGraphPatternBuilder> buildGraphPattern);
        /// <summary>
        /// Adds a BIND variable assignment to the graph pattern
        /// </summary>
        IAssignmentVariableNamePart<IGraphPatternBuilder> Bind(Func<INonAggregateExpressionBuilder, SparqlExpression> buildAssignmentExpression);
        /// <summary>
        /// Adds a "normal" child graph pattern
        /// </summary>
        IGraphPatternBuilder Child(IQueryBuilder queryBuilder);
        /// <summary>
        /// Adds a "normal" child graph pattern
        /// </summary>
        IGraphPatternBuilder Child(GraphPatternBuilder buildGraphPattern);
        /// <summary>
        /// Adds a "normal" child graph pattern
        /// </summary>
        IGraphPatternBuilder Child(Action<IGraphPatternBuilder> buildGraphPattern);
        /// <summary>
        /// Adds a VALUES inline data block to the graph pattern
        /// </summary>
        IInlineDataBuilder InlineData(params string[] variables);
    }
}