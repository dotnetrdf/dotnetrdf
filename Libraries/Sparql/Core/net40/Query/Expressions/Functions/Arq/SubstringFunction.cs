/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Factories;

namespace VDS.RDF.Query.Expressions.Functions.Arq
{
    /// <summary>
    /// Represents the ARQ afn:substring() function which is a sub-string with Java semantics
    /// </summary>
    public class SubstringFunction 
        : IExpression
    {
        private IExpression _expr, _start, _end;

        /// <summary>
        /// Creates a new ARQ substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Expression giving an index at which to start the substring</param>
        public SubstringFunction(IExpression stringExpr, IExpression startExpr)
            : this(stringExpr, startExpr, null) { }

        /// <summary>
        /// Creates a new ARQ substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Expression giving an index at which to start the substring</param>
        /// <param name="endExpr">Expression giving an index at which to end the substring</param>
        public SubstringFunction(IExpression stringExpr, IExpression startExpr, IExpression endExpr)
        {
            this._expr = stringExpr;
            this._start = startExpr;
            this._end = endExpr;
        }

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            INode input = this.CheckArgument(this._expr, solution, context);
            IValuedNode start = this.CheckArgument(this._start, solution, context, XPathFunctionFactory.AcceptNumericArguments);

            if (this._end != null)
            {
                IValuedNode end = this.CheckArgument(this._end, solution, context, XPathFunctionFactory.AcceptNumericArguments);

                if (input.Value.Equals(String.Empty)) return new StringNode(String.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.AsInteger());
                    int e = Convert.ToInt32(end.AsInteger());

                    if (s < 0) s = 0;
                    if (e < s)
                    {
                        //If no/negative characters are being selected the empty string is returned
                        return new StringNode(String.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    else if (s > input.Value.Length)
                    {
                        //If the start is after the end of the string the empty string is returned
                        return new StringNode(String.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    else
                    {
                        if (e > input.Value.Length)
                        {
                            //If the end is greater than the length of the string the string from the starts onwards is returned
                            return new StringNode(input.Value.Substring(s), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                        }
                        else
                        {
                            //Otherwise do normal substring
                            return new StringNode(input.Value.Substring(s, e - s), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
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
                if (input.Value.Equals(String.Empty)) return new StringNode(String.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.AsInteger());
                    if (s < 0) s = 0;

                    return new StringNode(input.Value.Substring(s), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                catch
                {
                    throw new RdfQueryException("Unable to convert the Start argument to an Integer");
                }
            }
        }

        private IValuedNode CheckArgument(IExpression expr, ISolution solution, IExpressionContext context)
        {
            return this.CheckArgument(expr, solution, context, XPathFunctionFactory.AcceptStringArguments);
        }

        private IValuedNode CheckArgument(IExpression expr, ISolution solution, IExpressionContext context, Func<Uri, bool> argumentTypeValidator)
        {
            IValuedNode temp = expr.Evaluate(solution, context);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Literal)
                {
                    INode lit = temp;
                    if (lit.DataType != null)
                    {
                        if (argumentTypeValidator(lit.DataType))
                        {
                            //Appropriately typed literals are fine
                            return temp;
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evaluate an ARQ substring as one of the argument expressions returned a typed literal with an invalid type");
                        }
                    }
                    else if (argumentTypeValidator(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString)))
                    {
                        //Untyped Literals are treated as Strings and may be returned when the argument allows strings
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
                if (this._end != null)
                {
                    return this._expr.Variables.Concat(this._start.Variables).Concat(this._end.Variables);
                }
                else
                {
                    return this._expr.Variables.Concat(this._start.Variables);
                }
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._end != null)
            {
                return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Substring + ">(" + this._expr.ToString() + "," + this._start.ToString() + "," + this._end.ToString() + ")";
            }
            else
            {
                return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Substring + ">(" + this._expr.ToString() + "," + this._start.ToString() + ")";
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
        public IEnumerable<IExpression> Arguments
        {
            get
            {
                if (this._end != null)
                {
                    return new IExpression[] { this._expr, this._start, this._end };
                }
                else
                {
                    return new IExpression[] { this._end, this._start };
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
                return this._expr.CanParallelise && this._start.CanParallelise && (this._end == null || this._end.CanParallelise);
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public IExpression Transform(IExpressionTransformer transformer)
        {
            if (this._end != null)
            {
                return new SubstringFunction(transformer.Transform(this._expr), transformer.Transform(this._start), transformer.Transform(this._end));
            }
            else
            {
                return new SubstringFunction(transformer.Transform(this._end), transformer.Transform(this._start));
            }
        }
    }
}
