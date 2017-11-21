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
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Abstract Base class for XPath Binary String functions
    /// </summary>
    public abstract class BaseBinaryStringFunction
        : ISparqlExpression
    {
        /// <summary>
        /// Expression the function applies over
        /// </summary>
        protected ISparqlExpression _expr;
        /// <summary>
        /// Argument expression
        /// </summary>
        protected ISparqlExpression _arg;
        /// <summary>
        /// Whether the argument can be null
        /// </summary>
        protected bool _allowNullArgument = false;
        /// <summary>
        /// Type validation function for the argument
        /// </summary>
        protected Func<Uri, bool> _argumentTypeValidator;

        /// <summary>
        /// Creates a new XPath Binary String function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="argExpr">Argument</param>
        /// <param name="allowNullArgument">Whether the argument may be null</param>
        /// <param name="argumentTypeValidator">Type validator for the argument</param>
        public BaseBinaryStringFunction(ISparqlExpression stringExpr, ISparqlExpression argExpr, bool allowNullArgument, Func<Uri, bool> argumentTypeValidator)
        {
            _expr = stringExpr;
            _arg = argExpr;
            _allowNullArgument = allowNullArgument;
            if (_arg == null && !_allowNullArgument) throw new RdfParseException("Cannot create a XPath String Function which takes a String and a single argument since the expression for the argument is null");
            _argumentTypeValidator = argumentTypeValidator;
        }

        /// <summary>
        /// Gets the Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = _expr.Evaluate(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode)temp;
                    if (lit.DataType != null && !lit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                    {
                        throw new RdfQueryException("Unable to evalaute an XPath String function on a non-string typed Literal");
                    }
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate an XPath String function on a non-Literal input");
                }

                // Once we've got to here we've established that the First argument is an appropriately typed/untyped Literal
                if (_arg == null)
                {
                    return ValueInternal((ILiteralNode)temp);
                }
                else
                {
                    // Need to validate the argument
                    INode tempArg = _arg.Evaluate(context, bindingID);
                    if (tempArg != null)
                    {
                        if (tempArg.NodeType == NodeType.Literal)
                        {
                            ILiteralNode litArg = (ILiteralNode)tempArg;
                            if (_argumentTypeValidator(litArg.DataType))
                            {
                                return ValueInternal((ILiteralNode)temp, litArg);
                            }
                            else
                            {
                                throw new RdfQueryException("Unable to evaluate an XPath String function since the type of the argument is not supported by this function");
                            }
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evaluate an XPath String function where the argument is a non-Literal");
                        }
                    }
                    else if (_allowNullArgument)
                    {
                        // Null argument permitted so just invoke the non-argument version of the function
                        return ValueInternal((ILiteralNode)temp);
                    }
                    else
                    {
                        throw new RdfQueryException("Unable to evaluate an XPath String function since the argument expression evaluated to a null and a null argument is not permitted by this function");
                    }
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
        public virtual IValuedNode ValueInternal(ILiteralNode stringLit)
        {
            if (!_allowNullArgument)
            {
                throw new RdfQueryException("This XPath function requires a non-null argument in addition to an input string");
            }
            else
            {
                throw new RdfQueryException("Derived classes which are functions which permit a null argument must override this method");
            }
        }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal and Argument
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <param name="arg">Argument</param>
        /// <returns></returns>
        public abstract IValuedNode ValueInternal(ILiteralNode stringLit, ILiteralNode arg);

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public virtual IEnumerable<string> Variables
        {
            get
            {
                if (_arg == null)
                {
                    return _expr.Variables;
                }
                else
                {
                    return _expr.Variables.Concat(_arg.Variables);
                }
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
                return new ISparqlExpression[] { _expr, _arg };
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return _expr.CanParallelise && _arg.CanParallelise;
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
