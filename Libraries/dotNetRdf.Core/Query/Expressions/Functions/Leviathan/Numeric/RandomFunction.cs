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
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric;

/// <summary>
/// Represents the Leviathan lfn:rnd() function.
/// </summary>
public class RandomFunction
    : BaseBinaryExpression
{
    private Random _rnd = new Random();
    private int _args = 0;

    /// <summary>
    /// Creates a new Leviathan Random Function.
    /// </summary>
    public RandomFunction()
        : base(new ConstantTerm(new DoubleNode(0)), new ConstantTerm(new DoubleNode(1))) { }

    /// <summary>
    /// Creates a new Leviathan Random Function.
    /// </summary>
    /// <param name="max">Maximum.</param>
    public RandomFunction(ISparqlExpression max)
        : base(new ConstantTerm(new DoubleNode(0)), max)
    {
        _args = 1;
    }

    /// <summary>
    /// Creates a new Leviathan Random Function.
    /// </summary>
    /// <param name="min">Minumum.</param>
    /// <param name="max">Maximum.</param>
    public RandomFunction(ISparqlExpression min, ISparqlExpression max)
        : base(min, max)
    {
        _args = 2;
    }

    /// <summary>
    /// Gets the String representation of the function.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        output.Append('<');
        output.Append(LeviathanFunctionFactory.LeviathanFunctionsNamespace);
        output.Append(LeviathanFunctionFactory.Random);
        output.Append(">(");
        switch (_args)
        {
            case 1:
                output.Append(_rightExpr);
                break;
            case 2:
                output.Append(_leftExpr);
                output.Append(',');
                output.Append(_rightExpr);
                break;
        }
        output.Append(')');
        return output.ToString();
    }

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessRandomFunction(this, context, binding);
    }

    /// <inheritdoc />
    public override T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitRandomFunction(this);
    }

    /// <summary>
    /// Gets the Functor of the Expression.
    /// </summary>
    public override string Functor
    {
        get
        {
            return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Random;
        }
    }

    /// <summary>
    /// Gets the type of the expression.
    /// </summary>
    public override SparqlExpressionType Type
    {
        get
        {
            return SparqlExpressionType.Function;
        }
    }

    /// <summary>
    /// Transforms the Expression using the given Transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public override ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        return new RandomFunction(transformer.Transform(_leftExpr), transformer.Transform(_rightExpr));
    }
}
