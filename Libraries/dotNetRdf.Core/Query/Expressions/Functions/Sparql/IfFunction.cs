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

namespace VDS.RDF.Query.Expressions.Functions.Sparql;

/// <summary>
/// Class representing the SPARQL IF function.
/// </summary>
public class IfElseFunction 
    : ISparqlExpression
{
    /// <summary>
    /// Creates a new IF function.
    /// </summary>
    /// <param name="condition">Condition.</param>
    /// <param name="ifBranch">Expression to evaluate if condition evaluates to true.</param>
    /// <param name="elseBranch">Expression to evalaute if condition evaluates to false/error.</param>
    public IfElseFunction(ISparqlExpression condition, ISparqlExpression ifBranch, ISparqlExpression elseBranch)
    {
        ConditionExpression = condition;
        TrueBranchExpression = ifBranch;
        FalseBranchExpression = elseBranch;
    }

    /// <summary>
    /// Get the condition to test.
    /// </summary>
    public ISparqlExpression ConditionExpression { get; }

    /// <summary>
    /// Get the expression to evaluate if <see cref="ConditionExpression"/> evaluates to true.
    /// </summary>
    public ISparqlExpression TrueBranchExpression { get; }

    /// <summary>
    /// Get the expression to evaluate if <see cref="ConditionExpression"/> evaluates to false.
    /// </summary>
    public ISparqlExpression FalseBranchExpression { get; }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessIfElseFunction(this, context, binding);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitIfElseFunction(this);
    }

    /// <summary>
    /// Gets the enumeration of variables used in the expression.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get 
        {
            return ConditionExpression.Variables.Concat(TrueBranchExpression.Variables).Concat(FalseBranchExpression.Variables);
        }
    }

    /// <summary>
    /// Gets the String representation of the function.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        output.Append("IF (");
        output.Append(ConditionExpression);
        output.Append(" , ");
        output.Append(TrueBranchExpression);
        output.Append(" , ");
        output.Append(FalseBranchExpression);
        output.Append(')');
        return output.ToString();
    }

    /// <summary>
    /// Gets the Expression Type.
    /// </summary>
    public SparqlExpressionType Type
    {
        get
        {
            return SparqlExpressionType.Function;
        }
    }

    /// <summary>
    /// Gets the Functor for the Expression.
    /// </summary>
    public string Functor
    {
        get
        {
            return SparqlSpecsHelper.SparqlKeywordIf;
        }
    }

    /// <summary>
    /// Gets the Arguments of the Expression.
    /// </summary>
    public IEnumerable<ISparqlExpression> Arguments
    {
        get
        {
            return new ISparqlExpression[] { ConditionExpression, TrueBranchExpression, FalseBranchExpression };
        }
    }

    /// <summary>
    /// Gets whether an expression can safely be evaluated in parallel.
    /// </summary>
    public virtual bool CanParallelise
    {
        get
        {
            return ConditionExpression.CanParallelise && TrueBranchExpression.CanParallelise && FalseBranchExpression.CanParallelise;
        }
    }

    /// <summary>
    /// Transforms the Expression using the given Transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        return new IfElseFunction(transformer.Transform(ConditionExpression), transformer.Transform(TrueBranchExpression), transformer.Transform(FalseBranchExpression));
    }
}
