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

using System;
using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.Query.Expressions.Primary;

/// <summary>
/// Class for representing a triple node term.
/// </summary>
public class TripleNodeTerm : ISparqlExpression
{
    /// <summary>
    /// The triple node this term represents.
    /// </summary>
    public ITripleNode Node { get; }

    /// <summary>
    /// Create a new term.
    /// </summary>
    /// <param name="n">The triple node this term represents.</param>
    public TripleNodeTerm(ITripleNode n)
    {
        Node = n;
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessTripleNodeTerm(this, context, binding);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IEnumerable<string> Variables {
        get
        {
            return Node.Triple.Nodes.OfType<IVariableNode>().Select(vn => vn.VariableName).Distinct();
        }

    }

    /// <inheritdoc />
    public SparqlExpressionType Type => SparqlExpressionType.Primary;

    /// <inheritdoc />
    public string Functor => string.Empty;

    /// <inheritdoc />
    public IEnumerable<ISparqlExpression> Arguments => Enumerable.Empty<ISparqlExpression>();

    /// <inheritdoc />
    public ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        return transformer.Transform(this);
    }

    /// <inheritdoc />
    public bool CanParallelise => true;
}
