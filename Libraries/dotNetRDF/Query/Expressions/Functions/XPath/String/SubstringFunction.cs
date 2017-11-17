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
    /// Represents the XPath fn:substring() function
    /// </summary>
    public class SubstringFunction
        : ISparqlExpression
    {
        private ISparqlExpression _expr, _start, _length;

        /// <summary>
        /// Creates a new XPath Substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Start</param>
        public SubstringFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr)
            : this(stringExpr, startExpr, null) { }

        /// <summary>
        /// Creates a new XPath Substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Start</param>
        /// <param name="lengthExpr">Length</param>
        public SubstringFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr, ISparqlExpression lengthExpr)
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

                int s = Convert.ToInt32(start.AsInteger());
                int l = Convert.ToInt32(length.AsInteger());

                if (s < 1) s = 1;
                if (l < 1)
                {
                    // If no/negative characters are being selected the empty string is returned
                    return new StringNode(null, string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                else if ((s - 1) > input.Value.Length)
                {
                    // If the start is after the end of the string the empty string is returned
                    return new StringNode(null, string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                else
                {
                    if (((s - 1) + l) > input.Value.Length)
                    {
                        // If the start plus the length is greater than the length of the string the string from the starts onwards is returned
                        return new StringNode(null, input.Value.Substring(s - 1), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    else
                    {
                        // Otherwise do normal substring
                        return new StringNode(null, input.Value.Substring(s - 1, l), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                }
            }
            else
            {
                if (input.Value.Equals(string.Empty)) return new StringNode(null, string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

                int s = Convert.ToInt32(start.AsInteger());
                if (s < 1) s = 1;

                return new StringNode(null, input.Value.Substring(s - 1), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
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
                            throw new RdfQueryException("Unable to evaluate an XPath substring as one of the argument expressions returned a typed literal with an invalid type");
                        }
                    }
                    else if (argumentTypeValidator(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString)))
                    {
                        // Untyped Literals are treated as Strings and may be returned when the argument allows strings
                        return temp;
                    }
                    else
                    {
                        throw new RdfQueryException("Unable to evalaute an XPath substring as one of the argument expressions returned an untyped literal");
                    }
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate an XPath substring as one of the argument expressions returned a non-literal");
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate an XPath substring as one of the argument expressions evaluated to null");
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
                return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Substring + ">(" + _expr.ToString() + "," + _start.ToString() + "," + _length.ToString() + ")";
            }
            else
            {
                return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Substring + ">(" + _expr.ToString() + "," + _start.ToString() + ")";
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
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Substring;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
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
                return new SubstringFunction(transformer.Transform(_expr), transformer.Transform(_start), transformer.Transform(_length));
            }
            else
            {
                return new SubstringFunction(transformer.Transform(_expr), transformer.Transform(_start));
            }
        }
    }
}
