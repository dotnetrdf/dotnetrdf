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
    /// Interface for creating SELECT queries
    /// </summary>
    public interface ISelectBuilder : IQueryBuilder
    {
        /// <summary>
        /// Adds additional SELECT return <paramref name="variables"/>
        /// </summary>
        ISelectBuilder And(params SparqlVariable[] variables);
        /// <summary>
        /// Adds additional SELECT return <paramref name="variables"/>
        /// </summary>
        ISelectBuilder And(params string[] variables);
        /// <summary>
        /// Adds additional SELECT expression
        /// </summary>
        IAssignmentVariableNamePart<ISelectBuilder> And<TExpression>(Func<IExpressionBuilder, PrimaryExpression<TExpression>> buildAssignmentExpression);
        /// <summary>
        /// Applies the DISTINCT modifier if the Query is a SELECT, otherwise leaves query unchanged (since results from any other query are DISTINCT by default)
        /// </summary>
        ISelectBuilder Distinct();
    }
}