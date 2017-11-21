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

namespace VDS.RDF.Query.Expressions.Functions.Arq
{
    /// <summary>
    /// Represents the ARQ afn:substring() function which is a sub-string with Java semantics
    /// </summary>
    public class SubstringFunction 
        : ISparqlExpression
    {
        private ISparqlExpression _expr, _start, _end;

        /// <summary>
        /// Creates a new ARQ substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Expression giving an index at which to start the substring</param>
        public SubstringFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr)
            : this(stringExpr, startExpr, null) { }

        /// <summary>
        /// Creates a new ARQ substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Expression giving an index at which to start the substring</param>
        /// <param name="endExpr">Expression giving an index at which to end the substring</param>
        public SubstringFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr, ISparqlExpression endExpr)
        {
            _expr = stringExpr;
            _start = startExpr;
            _end = endExpr;
        }

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            ILiteralNode input = (ILiteralNode)CheckArgument(_expr, context, bindingID);
            IValuedNode start = CheckArgument(_start, context, bindingID, XPathFunctionFactory.AcceptNumericArguments);

            if (_end != null)
            {
                IValuedNode end = CheckArgument(_end, context, bindingID, XPathFunctionFactory.AcceptNumericArguments);

                if (input.Value.Equals(String.Empty)) return new StringNode(null, String.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.AsInteger());
                    int e = Convert.ToInt32(end.AsInteger());

                    if (s < 0) s = 0;
                    if (e < s)
                    {
                        // If no/negative characters are being selected the empty string is returned
                        return new StringNode(null, String.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    else if (s > input.Value.Length)
                    {
                        // If the start is after the end of the string the empty string is returned
                        return new StringNode(null, String.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    else
                    {
                        if (e > input.Value.Length)
                        {
                            // If the end is greater than the length of the string the string from the starts onwards is returned
                            return new StringNode(null, input.Value.Substring(s), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                        }
                        else
                        {
                            // Otherwise do normal substring
                            return new StringNode(null, input.Value.Substring(s, e - s), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                        }
                    }
                }
                catch
                {
                    throw new RdfQueryException("Unable to convert the Start/End argument to an Integer");
                }
            }
            else
            {
                if (input.Value.Equals(String.Empty)) return new StringNode(null, String.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.AsInteger());
                    if (s < 0) s = 0;

                    return new StringNode(null, input.Value.Substring(s), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
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
                            throw new RdfQueryException("Unable to evaluate an ARQ substring as one of the argument expressions returned a typed literal with an invalid type");
                        }
                    }
                    else if (argumentTypeValidator(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString)))
                    {
                        // Untyped Literals are treated as Strings and may be returned when the argument allows strings
                        return temp;
                    }
                    else
                    {
                        throw new RdfQueryException("Unable to evalaute an ARQ substring as one of the argument expressions returned an untyped literal");
                    }
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate an ARQ substring as one of the argument expressions returned a non-literal");
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate an ARQ substring as one of the argument expressions evaluated to null");
            }
        }

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                if (_end != null)
                {
                    return _expr.Variables.Concat(_start.Variables).Concat(_end.Variables);
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
            if (_end != null)
            {
                return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Substring + ">(" + _expr.ToString() + "," + _start.ToString() + "," + _end.ToString() + ")";
            }
            else
            {
                return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Substring + ">(" + _expr.ToString() + "," + _start.ToString() + ")";
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
        public String Functor
        {
            get
            {
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Substring;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                if (_end != null)
                {
                    return new ISparqlExpression[] { _expr, _start, _end };
                }
                else
                {
                    return new ISparqlExpression[] { _end, _start };
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
                return _expr.CanParallelise && _start.CanParallelise && (_end == null || _end.CanParallelise);
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            if (_end != null)
            {
                return new SubstringFunction(transformer.Transform(_expr), transformer.Transform(_start), transformer.Transform(_end));
            }
            else
            {
                return new SubstringFunction(transformer.Transform(_end), transformer.Transform(_start));
            }
        }
    }
}
