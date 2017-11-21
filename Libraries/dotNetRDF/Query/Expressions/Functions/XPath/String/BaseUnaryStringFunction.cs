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

using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Abstract Base Class for XPath Unary String functions
    /// </summary>
    public abstract class BaseUnaryStringFunction
        : ISparqlExpression
    {
        /// <summary>
        /// Expression the function applies over
        /// </summary>
        protected ISparqlExpression _expr;

        /// <summary>
        /// Creates a new XPath Unary String function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        public BaseUnaryStringFunction(ISparqlExpression stringExpr)
        {
            _expr = stringExpr;
        }

        /// <summary>
        /// Gets the Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = _expr.Evaluate(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode)temp;
                    if (lit.DataType != null)
                    {
                        if (lit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                        {
                            return ValueInternal(lit);
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evalaute an XPath String function on a non-string typed Literal");
                        }
                    }
                    else
                    {
                        return ValueInternal(lit);
                    }
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate an XPath String function on a non-Literal input");
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate an XPath String function on a null input");
            }
        }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        protected abstract IValuedNode ValueInternal(ILiteralNode stringLit);

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public virtual IEnumerable<string> Variables
        {
            get
            {
                return _expr.Variables;
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public abstract string Functor
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
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public abstract ISparqlExpression Transform(IExpressionTransformer transformer);
    }
}
