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

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric;

/// <summary>
/// Represents the Leviathan lfn:cartesian() function.
/// </summary>
public class CartesianFunction
    : ISparqlExpression
{
    /// <summary>
    /// Creates a new 2D Cartesian Function.
    /// </summary>
    /// <param name="x1">Expression for X Coordinate of 1st point.</param>
    /// <param name="y1">Expression for Y Coordinate of 1st point.</param>
    /// <param name="x2">Expression for X Coordinate of 2nd point.</param>
    /// <param name="y2">Expression for Y Coordinate of 2nd point.</param>
    public CartesianFunction(ISparqlExpression x1, ISparqlExpression y1, ISparqlExpression x2, ISparqlExpression y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

    /// <summary>
    /// Creates a new 3D Cartesian Function.
    /// </summary>
    /// <param name="x1">Expression for X Coordinate of 1st point.</param>
    /// <param name="y1">Expression for Y Coordinate of 1st point.</param>
    /// <param name="z1">Expression for Z Coordinate of 1st point.</param>
    /// <param name="x2">Expression for X Coordinate of 2nd point.</param>
    /// <param name="y2">Expression for Y Coordinate of 2nd point.</param>
    /// <param name="z2">Expression for Z Coordinate of 2nd point.</param>
    public CartesianFunction(ISparqlExpression x1, ISparqlExpression y1, ISparqlExpression z1, ISparqlExpression x2, ISparqlExpression y2, ISparqlExpression z2)
    {
        X1 = x1;
        Y1 = y1;
        Z1 = z1;
        X2 = x2;
        Y2 = y2;
        Z2 = z2;
        Is3D = true;
    }

    /// <summary>
    /// Return true if the function has three dimensions, false if it has two.
    /// </summary>
    public bool Is3D { get; }
    /// <summary>
    /// The x-coordinate of the first point.
    /// </summary>
    public ISparqlExpression X1 { get; }
    /// <summary>
    /// The x-coordinate of the second point.
    /// </summary>
    public ISparqlExpression X2 { get; }
    /// <summary>
    /// The y-coordinate of the first point.
    /// </summary>
    public ISparqlExpression Y1 { get; }
    /// <summary>
    /// The y-coordinate of the second point.
    /// </summary>
    public ISparqlExpression Y2 { get; }
    /// <summary>
    /// The z-coordinate of the first point. Null if the function is a 2D function.
    /// </summary>
    public ISparqlExpression Z1 { get; }
    /// <summary>
    /// The z-coordinate of the second point. Null if the function is a 2D function.
    /// </summary>
    public ISparqlExpression Z2 { get; }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessCartesianFunction(this, context, binding);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitCartesianFunction(this);
    }

    /// <summary>
    /// Gets the Variables used in the function.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            return Is3D 
                ? X1.Variables.Concat(Y1.Variables).Concat(Z1.Variables).Concat(X2.Variables).Concat(Y2.Variables).Concat(Z2.Variables) 
                : X1.Variables.Concat(Y1.Variables).Concat(X2.Variables).Concat(Y2.Variables);
        }
    }

    /// <summary>
    /// Gets the String representation of the function.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        output.Append("<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Cartesian + ">(");
        output.Append(X1);
        output.Append(',');
        output.Append(Y1);
        output.Append(',');
        if (Is3D)
        {
            output.Append(Z1);
            output.Append(',');
        }
        output.Append(X2);
        output.Append(',');
        output.Append(Y2);
        if (Is3D)
        {
            output.Append(',');
            output.Append(Z2);
        }
        output.Append(')');
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
            return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Cartesian;
        }
    }

    /// <summary>
    /// Gets the Arguments of the Expression.
    /// </summary>
    public IEnumerable<ISparqlExpression> Arguments
    {
        get
        {
            return Is3D 
                ? new[] { X1, Y1, Z1, X2, Y2, Z2 } 
                : new[] { X1, Y1, X2, Y2 };
        }
    }

    /// <summary>
    /// Gets whether an expression can safely be evaluated in parallel.
    /// </summary>
    public virtual bool CanParallelise
    {
        get
        {
            if (Is3D)
            {
                return X1.CanParallelise && Y1.CanParallelise && Z1.CanParallelise && X1.CanParallelise && Y2.CanParallelise && Z2.CanParallelise;
            }
            else
            {
                return X1.CanParallelise && Y1.CanParallelise && X2.CanParallelise && Y2.CanParallelise;
            }
        }
    }

    /// <summary>
    /// Transforms the Expression using the given Transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        return Is3D 
            ? new CartesianFunction(
                transformer.Transform(X1), transformer.Transform(Y1), transformer.Transform(Z1),
                transformer.Transform(X2), transformer.Transform(Y2), transformer.Transform(Z2)) 
            : new CartesianFunction(
                transformer.Transform(X1), transformer.Transform(Y1),
                transformer.Transform(X2), transformer.Transform(Y2));
    }
}
