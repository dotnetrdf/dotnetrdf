/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using System.Collections.Generic;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Ordering;

/// <summary>
/// Base Class for implementing Sparql ORDER BYs.
/// </summary>
public abstract class BaseOrderBy 
    : ISparqlOrderBy
{
    /// <summary>
    /// Gets/Sets the Child Order By.
    /// </summary>
    public ISparqlOrderBy Child { get; set; } = null;

    /// <summary>
    /// Sets the Ordering to Descending.
    /// </summary>
    public bool Descending { get; set; }
    
    /// <summary>
    /// Gets whether the Ordering is Simple.
    /// </summary>
    public abstract bool IsSimple
    {
        get;
    }

    /// <summary>
    /// Gets all the Variables used in the Ordering.
    /// </summary>
    public abstract IEnumerable<string> Variables
    {
        get;
    }

    /// <summary>
    /// Gets the Expression used in the Ordering.
    /// </summary>
    public abstract ISparqlExpression Expression
    {
        get;
    }

    /// <summary>
    /// Generates a Comparer than can be used to do Ordering based on the given Triple Pattern.
    /// </summary>
    /// <param name="pattern">Triple Pattern.</param>
    /// <param name="nodeComparer">The node comparer to use.</param>
    /// <returns></returns>
    public abstract IComparer<Triple> GetComparer(IMatchTriplePattern pattern, ISparqlNodeComparer nodeComparer);

    /// <summary>
    /// Gets the String representation of the Order By.
    /// </summary>
    /// <returns></returns>
    public abstract override string ToString();

}