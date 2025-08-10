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

namespace VDS.RDF.Query.Filters;

/// <summary>
/// Abstract Base class for Unary Filters that operate on a single Expression.
/// </summary>
public abstract class BaseUnaryFilter 
    : ISparqlFilter 
{
    /// <summary>
    /// Expression which is the Argument to the Filter.
    /// </summary>
    protected ISparqlExpression _arg;

    /// <summary>
    /// Creates a new Base Unary Filter.
    /// </summary>
    /// <param name="arg">Argument to the Filter.</param>
    protected BaseUnaryFilter(ISparqlExpression arg)
    {
        _arg = arg;
    }

    /// <summary>
    /// Gets the String representation of the Filter.
    /// </summary>
    /// <returns></returns>
    public abstract override string ToString();

    /// <inheritdoc />
    public abstract TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context);

    /// <inheritdoc />
    public abstract T Accept<T>(ISparqlAlgebraVisitor<T> visitor);

    /// <summary>
    /// Gets the enumeration of Variables used in the Filter.
    /// </summary>
    public virtual IEnumerable<string> Variables
    {
        get
        {
            return _arg.Variables;
        }
    }

    /// <summary>
    /// Gets the inner expression this Filter uses.
    /// </summary>
    public virtual ISparqlExpression Expression
    {
        get
        {
            return _arg;
        }
    }
}