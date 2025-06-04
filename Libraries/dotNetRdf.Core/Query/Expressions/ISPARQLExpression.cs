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

namespace VDS.RDF.Query.Expressions;




/// <summary>
/// SPARQL Expression Types.
/// </summary>
public enum SparqlExpressionType
{
    /// <summary>
    /// The Expression is a Primary Expression which is a leaf in the expression tree
    /// </summary>
    Primary,
    /// <summary>
    /// The Expression is a Unary Operator which has a single argument
    /// </summary>
    UnaryOperator,
    /// <summary>
    /// The Expression is a Binary Operator which has two arguments
    /// </summary>
    BinaryOperator,
    /// <summary>
    /// The Expression is a Function which has zero/more arguments
    /// </summary>
    Function,
    /// <summary>
    /// The Expression is an Aggregate Function which has one/more arguments
    /// </summary>
    Aggregate,
    /// <summary>
    /// The Expression is a Set Operator where the first argument forms the LHS and all remaining arguments form a set on the RHS
    /// </summary>
    SetOperator,
    /// <summary>
    /// The Expression is a Unary Operator that applies to a Graph Pattern
    /// </summary>
    GraphOperator,
}

/// <summary>
/// Interface for SPARQL Expression Terms that can be used in Expression Trees while evaluating Sparql Queries.
/// </summary>
public interface ISparqlExpression
{
    /// <summary>
    /// Accept a <see cref="ISparqlExpressionProcessor{TResult,TContext,TBinding}"/> by calling the appropriate method on its interface for this expression.
    /// </summary>
    /// <typeparam name="TResult">Type of result that the process method returns.</typeparam>
    /// <typeparam name="TContext">The type of the context parameter to the process method.</typeparam>
    /// <typeparam name="TBinding">The type of the binding parameter of the process method.</typeparam>
    /// <param name="processor">The processor to be invoked.</param>
    /// <param name="context">The context object to use for processing.</param>
    /// <param name="binding">The binding to be processed.</param>
    /// <returns></returns>
    TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding);

    /// <summary>
    /// Accept a <see cref="ISparqlExpressionVisitor{T}"/> by calling the appropriate method on its interface for this expression.
    /// </summary>
    /// <typeparam name="T">Type of result that the called method returns.</typeparam>
    /// <param name="visitor">The visitor to be invoked.</param>
    /// <returns>The result of calling the method on the visitor.</returns>
    T Accept<T>(ISparqlExpressionVisitor<T> visitor);

    /// <summary>
    /// Gets an enumeration of all the Variables used in an expression.
    /// </summary>
    IEnumerable<string> Variables
    {
        get;
    }

    /// <summary>
    /// Gets the SPARQL Expression Type.
    /// </summary>
    SparqlExpressionType Type
    {
        get;
    }

    /// <summary>
    /// Gets the Function Name or Operator Symbol - function names may be URIs of Keywords or the empty string in the case of primary expressions.
    /// </summary>
    string Functor
    {
        get;
    }

    /// <summary>
    /// Gets the Arguments of this Expression.
    /// </summary>
    IEnumerable<ISparqlExpression> Arguments
    {
        get;
    }

    /// <summary>
    /// Transforms the arguments of the expression using the given transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    ISparqlExpression Transform(IExpressionTransformer transformer);

    /// <summary>
    /// Gets whether an expression can safely be evaluated in parallel.
    /// </summary>
    bool CanParallelise
    {
        get;
    }
}