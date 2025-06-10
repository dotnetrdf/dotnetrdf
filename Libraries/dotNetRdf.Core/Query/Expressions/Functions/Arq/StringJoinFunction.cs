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
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions.Functions.Arq;

/// <summary>
/// Represents the ARQ afn:strjoin() function which is a string concatenation function with a separator.
/// </summary>
public class StringJoinFunction 
    : ISparqlExpression
{
    private readonly List<ISparqlExpression> _exprs = new List<ISparqlExpression>();

    /// <summary>
    /// Get the fixed separator string to use.
    /// </summary>
    public string Separator { get; }
    /// <summary>
    /// Return true if a <see cref="FixedSeparator"/> string is to be used, false if a <see cref="SeparatorExpression"/> is to be evaluated when joining strings.
    /// </summary>
    public bool FixedSeparator { get; } = false;

    /// <summary>
    /// The expression to evaluate when joining strings.
    /// </summary>
    public ISparqlExpression SeparatorExpression { get; }
    
    /// <summary>
    /// The expressions whose values are to be joined.
    /// </summary>
    public IList<ISparqlExpression> Expressions { get => _exprs; }

    /// <summary>
    /// Creates a new ARQ String Join function.
    /// </summary>
    /// <param name="sepExpr">Separator Expression.</param>
    /// <param name="expressions">Expressions to concatenate.</param>
    public StringJoinFunction(ISparqlExpression sepExpr, IEnumerable<ISparqlExpression> expressions)
    {
        if (sepExpr is ConstantTerm ct && ct.Node.NodeType == NodeType.Literal)
        {
            Separator = ct.Node.AsString();
            FixedSeparator = true;
        }
        else
        {
            SeparatorExpression = sepExpr;
        }

        _exprs.AddRange(expressions);
    }


    /// <inheritdoc />
    public TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessStringJoinFunction(this, context, binding);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitStringJoinFunction(this);
    }

    /// <summary>
    /// Gets the Variables used in the function.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            return (from expr in _exprs
                    from v in expr.Variables
                    select v);
        }
    }

    /// <summary>
    /// Gets the String representation of the function.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        output.Append('<');
        output.Append(ArqFunctionFactory.ArqFunctionsNamespace);
        output.Append(ArqFunctionFactory.StrJoin);
        output.Append(">(");
        output.Append(SeparatorExpression);
        output.Append(",");
        for (var i = 0; i < _exprs.Count; i++)
        {
            output.Append(_exprs[i]);
            if (i < _exprs.Count - 1) output.Append(',');
        }
        output.Append(")");
        return output.ToString();
    }

    /// <summary>
    /// Gets the Type of the Expression.
    /// </summary>
    public SparqlExpressionType Type
    {
        get
        {
            return SparqlExpressionType.Function;
        }
    }

    /// <summary>
    /// Gets the Functor of the Expression.
    /// </summary>
    public string Functor
    {
        get
        {
            return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.StrJoin;
        }
    }

    /// <summary>
    /// Gets the Arguments of the Expression.
    /// </summary>
    public IEnumerable<ISparqlExpression> Arguments
    {
        get
        {
            return SeparatorExpression.AsEnumerable().Concat(_exprs);
        }
    }

    /// <summary>
    /// Gets whether an expression can safely be evaluated in parallel.
    /// </summary>
    public virtual bool CanParallelise
    {
        get
        {
            return SeparatorExpression.CanParallelise && _exprs.All(e => e.CanParallelise);
        }
    }

    /// <summary>
    /// Transforms the Expression using the given Transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        return new StringJoinFunction(transformer.Transform(SeparatorExpression), _exprs.Select(e => transformer.Transform(e)));
    }
}
