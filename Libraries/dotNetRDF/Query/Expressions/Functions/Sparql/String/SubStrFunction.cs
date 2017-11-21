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

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Represents the SPARQL SUBSTR Function
    /// </summary>
    public class SubStrFunction
        : ISparqlExpression
    {
        private ISparqlExpression _expr, _start, _length;

        /// <summary>
        /// Creates a new XPath Substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Start</param>
        public SubStrFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr)
            : this(stringExpr, startExpr, null) { }

        /// <summary>
        /// Creates a new XPath Substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Start</param>
        /// <param name="lengthExpr">Length</param>
        public SubStrFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr, ISparqlExpression lengthExpr)
        {
            _expr = stringExpr;
            _start = startExpr;
            _length = lengthExpr;
        }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            ILiteralNode input = (ILiteralNode)CheckArgument(_expr, context, bindingID);
            IValuedNode start = CheckArgument(_start, context, bindingID, XPathFunctionFactory.AcceptNumericArguments);

            if (_length != null)
            {
                IValuedNode length = CheckArgument(_length, context, bindingID, XPathFunctionFactory.AcceptNumericArguments);

                if (input.Value.Equals(string.Empty)) return new StringNode(null, string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.AsInteger());
                    int l = Convert.ToInt32(length.AsInteger());

                    if (s < 1) s = 1;
                    if (l < 1)
                    {
                        // If no/negative characters are being selected the empty string is returned
                        if (input.DataType != null)
                        {
                            return new StringNode(null, string.Empty, input.DataType);
                        }
                        else
                        {
                            return new StringNode(null, string.Empty, input.Language);
                        }
                    }
                    else if ((s - 1) > input.Value.Length)
                    {
                        // If the start is after the end of the string the empty string is returned
                        if (input.DataType != null)
                        {
                            return new StringNode(null, string.Empty, input.DataType);
                        }
                        else
                        {
                            return new StringNode(null, string.Empty, input.Language);
                        }
                    }
                    else
                    {
                        if (((s - 1) + l) > input.Value.Length)
                        {
                            // If the start plus the length is greater than the length of the string the string from the starts onwards is returned
                            if (input.DataType != null)
                            {
                                return new StringNode(null, input.Value.Substring(s - 1), input.DataType);
                            }
                            else
                            {
                                return new StringNode(null, input.Value.Substring(s - 1), input.Language);
                            }
                        }
                        else
                        {
                            // Otherwise do normal substring
                            if (input.DataType != null)
                            {
                                return new StringNode(null, input.Value.Substring(s - 1, l), input.DataType);
                            }
                            else
                            {
                                return new StringNode(null, input.Value.Substring(s - 1, l), input.Language);
                            }
                        }
                    }
                }
                catch
                {
                    throw new RdfQueryException("Unable to convert the Start/Length argument to an Integer");
                }
            }
            else
            {
                if (input.Value.Equals(string.Empty)) return new StringNode(null, string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.AsInteger());
                    if (s < 1) s = 1;

                    if (input.DataType != null)
                    {
                        return new StringNode(null, input.Value.Substring(s - 1), input.DataType);
                    }
                    else
                    {
                        return new StringNode(null, input.Value.Substring(s - 1), input.Language);
                    }
                }
                catch
                {
                    throw new RdfQueryException("Unable to convert the Start argument to an Integer");
                }
            }
        }

        private IValuedNode CheckArgument(ISparqlExpression expr, SparqlEvaluationContext context, int bindingID)
        {
            return CheckArgument(expr, context, bindingID, XPathFunctionFactory.AcceptStringArguments);
        }

        private IValuedNode CheckArgument(ISparqlExpression expr, SparqlEvaluationContext context, int bindingID, Func<Uri, bool> argumentTypeValidator)
        {
            IValuedNode temp = expr.Evaluate(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode)temp;
                    if (lit.DataType != null)
                    {
                        if (argumentTypeValidator(lit.DataType))
                        {
                            // Appropriately typed literals are fine
                            return temp;
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evaluate a substring as one of the argument expressions returned a typed literal with an invalid type");
                        }
                    }
                    else if (argumentTypeValidator(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString)))
                    {
                        // Untyped Literals are treated as Strings and may be returned when the argument allows strings
                        return temp;
                    }
                    else
                    {
                        throw new RdfQueryException("Unable to evalaute a substring as one of the argument expressions returned an untyped literal");
                    }
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate a substring as one of the argument expressions returned a non-literal");
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate a substring as one of the argument expressions evaluated to null");
            }
        }

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                if (_length != null)
                {
                    return _expr.Variables.Concat(_start.Variables).Concat(_length.Variables);
                }
                else
                {
                    return _expr.Variables.Concat(_start.Variables);
                }
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_length != null)
            {
                return SparqlSpecsHelper.SparqlKeywordSubStr + "(" + _expr.ToString() + "," + _start.ToString() + "," + _length.ToString() + ")";
            }
            else
            {
                return SparqlSpecsHelper.SparqlKeywordSubStr + "(" + _expr.ToString() + "," + _start.ToString() + ")";
            }
        }

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
        public string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSubStr;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Function
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                if (_length != null)
                {
                    return new ISparqlExpression[] { _expr, _start, _length };
                }
                else
                {
                    return new ISparqlExpression[] { _expr, _start };
                }
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return _expr.CanParallelise && _start.CanParallelise && (_length == null || _length.CanParallelise);
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            if (_length != null)
            {
                return new SubStrFunction(transformer.Transform(_expr), transformer.Transform(_start), transformer.Transform(_length));
            }
            else
            {
                return new SubStrFunction(transformer.Transform(_expr), transformer.Transform(_start));
            }
        }
    }
}
