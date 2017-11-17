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
using System.Linq;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Abstract base class for Unary Expressions
    /// </summary>
    public abstract class BaseUnaryExpression 
        : ISparqlExpression
    {
        /// <summary>
        /// The sub-expression of this Expression
        /// </summary>
        protected ISparqlExpression _expr;

        /// <summary>
        /// Creates a new Base Unary Expression
        /// </summary>
        /// <param name="expr">Expression</param>
        public BaseUnaryExpression(ISparqlExpression expr)
        {
            _expr = expr;
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluuation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public abstract IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID);

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets an enumeration of all the Variables used in this expression
        /// </summary>
        public virtual IEnumerable<String> Variables
        {
            get
            {
                return _expr.Variables;
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public abstract SparqlExpressionType Type
        {
            get;
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public abstract String Functor
        {
            get;
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public virtual IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return _expr.AsEnumerable();
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return _expr.CanParallelise;
            }
        }

        /// <summary>
        /// Transforms the arguments of the expression using the given transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public abstract ISparqlExpression Transform(IExpressionTransformer transformer);
    }

    /// <summary>
    /// Abstract base class for Binary Expressions
    /// </summary>
    public abstract class BaseBinaryExpression
        : ISparqlExpression
    {
        /// <summary>
        /// The sub-expressions of this Expression
        /// </summary>
        protected ISparqlExpression _leftExpr, _rightExpr;

        /// <summary>
        /// Creates a new Base Binary Expression
        /// </summary>
        /// <param name="leftExpr">Left Expression</param>
        /// <param name="rightExpr">Right Expression</param>
        public BaseBinaryExpression(ISparqlExpression leftExpr, ISparqlExpression rightExpr)
        {
            _leftExpr = leftExpr;
            _rightExpr = rightExpr;
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public abstract IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID);

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets an enumeration of all the Variables used in this expression
        /// </summary>
        public virtual IEnumerable<String> Variables
        {
            get
            {
                return _leftExpr.Variables.Concat(_rightExpr.Variables);
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public abstract SparqlExpressionType Type
        {
            get;
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public abstract String Functor
        {
            get;
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return new ISparqlExpression[] { _leftExpr, _rightExpr };
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return _leftExpr.CanParallelise && _rightExpr.CanParallelise;
            }
        }

        /// <summary>
        /// Transforms the arguments of the expression using the given transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public abstract ISparqlExpression Transform(IExpressionTransformer transformer);
    }
}