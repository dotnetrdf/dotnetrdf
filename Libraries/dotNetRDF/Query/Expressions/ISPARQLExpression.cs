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
using System.Collections.Generic;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Numeric Types for Sparql Numeric Expressions
    /// </summary>
    /// <remarks>All Numeric expressions in Sparql are typed as Integer/Decimal/Double</remarks>
    public enum SparqlNumericType : int
    {
        /// <summary>
        /// Not a Number
        /// </summary>
        NaN = -1,
        /// <summary>
        /// An Integer
        /// </summary>
        Integer = 0,
        /// <summary>
        /// A Decimal
        /// </summary>
        Decimal = 1,
        /// <summary>
        /// A Single precision Floating Point
        /// </summary>
        Float = 2,
        /// <summary>
        /// A Double precision Floating Point
        /// </summary>
        Double = 3
    }

    /// <summary>
    /// SPARQL Expression Types
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
        GraphOperator
    }

    /// <summary>
    /// Interface for SPARQL Expression Terms that can be used in Expression Trees while evaluating Sparql Queries
    /// </summary>
    public interface ISparqlExpression
    {
        /// <summary>
        /// Evalutes a SPARQL Expression for the given binding in a given context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Newly introduced in Version 0.6.0 to replace the variety of functions that were used previously for numeric vs non-numeric versions to allow our code to be simplified and improve performance
        /// </para>
        /// </remarks>
        IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID);

        /// <summary>
        /// Gets an enumeration of all the Variables used in an expression
        /// </summary>
        IEnumerable<String> Variables
        {
            get;
        }

        /// <summary>
        /// Gets the SPARQL Expression Type
        /// </summary>
        SparqlExpressionType Type
        {
            get;
        }

        /// <summary>
        /// Gets the Function Name or Operator Symbol - function names may be URIs of Keywords or the empty string in the case of primary expressions
        /// </summary>
        String Functor
        {
            get;
        }

        /// <summary>
        /// Gets the Arguments of this Expression
        /// </summary>
        IEnumerable<ISparqlExpression> Arguments
        {
            get;
        }

        /// <summary>
        /// Transforms the arguments of the expression using the given transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        ISparqlExpression Transform(IExpressionTransformer transformer);

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        bool CanParallelise
        {
            get;
        }
    }
}