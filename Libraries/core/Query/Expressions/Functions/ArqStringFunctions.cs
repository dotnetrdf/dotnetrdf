/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Represents the ARQ afn:substring() function which is a sub-string with Java semantics
    /// </summary>
    public class ArqSubstringFunction : ISparqlExpression
    {
        private ISparqlExpression _expr, _start, _end;

        /// <summary>
        /// Creates a new ARQ substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Expression giving an index at which to start the substring</param>
        public ArqSubstringFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr)
            : this(stringExpr, startExpr, null) { }

        /// <summary>
        /// Creates a new ARQ substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Expression giving an index at which to start the substring</param>
        /// <param name="endExpr">Expression giving an index at which to end the substring</param>
        public ArqSubstringFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr, ISparqlExpression endExpr)
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
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            ILiteralNode input = this.CheckArgument(this._expr, context, bindingID);
            ILiteralNode start = this.CheckArgument(this._start, context, bindingID, XPathFunctionFactory.AcceptNumericArguments);

            if (this._end != null)
            {
                ILiteralNode end = this.CheckArgument(this._end, context, bindingID, XPathFunctionFactory.AcceptNumericArguments);

                if (input.Value.Equals(String.Empty)) return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.Value);
                    int e = Convert.ToInt32(end.Value);

                    if (s < 0) s = 0;
                    if (e < s)
                    {
                        //If no/negative characters are being selected the empty string is returned
                        return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    else if (s > input.Value.Length)
                    {
                        //If the start is after the end of the string the empty string is returned
                        return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    else
                    {
                        if (e > input.Value.Length)
                        {
                            //If the end is greater than the length of the string the string from the starts onwards is returned
                            return new LiteralNode(null, input.Value.Substring(s), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                        }
                        else
                        {
                            //Otherwise do normal substring
                            return new LiteralNode(null, input.Value.Substring(s, e-s), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
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
                if (input.Value.Equals(String.Empty)) return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.Value);
                    if (s < 0) s = 0;

                    return new LiteralNode(null, input.Value.Substring(s), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                catch
                {
                    throw new RdfQueryException("Unable to convert the Start argument to an Integer");
                }
            }
        }

        private ILiteralNode CheckArgument(ISparqlExpression expr, SparqlEvaluationContext context, int bindingID)
        {
            return this.CheckArgument(expr, context, bindingID, XPathFunctionFactory.AcceptStringArguments);
        }

        private ILiteralNode CheckArgument(ISparqlExpression expr, SparqlEvaluationContext context, int bindingID, Func<Uri, bool> argumentTypeValidator)
        {
            INode temp = expr.Value(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode)temp;
                    if (lit.DataType != null)
                    {
                        if (argumentTypeValidator(lit.DataType))
                        {
                            //Appropriately typed literals are fine
                            return lit;
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evaluate an ARQ substring as one of the argument expressions returned a typed literal with an invalid type");
                        }
                    }
                    else if (argumentTypeValidator(new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)))
                    {
                        //Untyped Literals are treated as Strings and may be returned when the argument allows strings
                        return lit;
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
        /// Gets the effective boolean value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
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
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                if (this._end != null)
                {
                    return new ISparqlExpression[] { this._expr, this._start, this._end };
                } 
                else 
                {
                    return new ISparqlExpression[] { this._end, this._start };
                }
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            if (this._end != null)
            {
                return new ArqSubstringFunction(transformer.Transform(this._expr), transformer.Transform(this._start), transformer.Transform(this._end));
            }
            else
            {
                return new ArqSubstringFunction(transformer.Transform(this._end), transformer.Transform(this._start));
            }
        }
    }

    /// <summary>
    /// Represents the ARQ afn:strjoin() function which is a string concatenation function with a separator
    /// </summary>
    public class ArqStringJoinFunction : ISparqlExpression
    {
        private ISparqlExpression _sep;
        private String _separator;
        private bool _fixedSeparator = false;
        private List<ISparqlExpression> _exprs = new List<ISparqlExpression>();

        /// <summary>
        /// Creates a new ARQ String Join function
        /// </summary>
        /// <param name="sepExpr">Separator Expression</param>
        /// <param name="expressions">Expressions to concatentate</param>
        public ArqStringJoinFunction(ISparqlExpression sepExpr, IEnumerable<ISparqlExpression> expressions)
        {
            if (sepExpr is NodeExpressionTerm)
            {
                INode temp = sepExpr.Value(null, 0);
                if (temp.NodeType == NodeType.Literal)
                {
                    this._separator = ((ILiteralNode)temp).Value;
                    this._fixedSeparator = true;
                }
                else
                {
                    this._sep = sepExpr;
                }
            }
            else
            {
                this._sep = sepExpr;
            }
            this._exprs.AddRange(expressions);
        }

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < this._exprs.Count; i++)
            {
                INode temp = this._exprs[i].Value(context, bindingID);
                if (temp == null) throw new RdfQueryException("Cannot evaluate the XPath concat() function when an argument evaluates to a Null");
                switch (temp.NodeType)
                {
                    case NodeType.Literal:
                        output.Append(((ILiteralNode)temp).Value);
                        break;
                    default:
                        throw new RdfQueryException("Cannot evaluate the XPath concat() function when an argument is not a Literal Node");
                }
                if (i < this._exprs.Count - 1)
                {
                    if (this._fixedSeparator)
                    {
                        output.Append(this._separator);
                    }
                    else
                    {
                        INode sep = this._sep.Value(context, bindingID);
                        if (sep == null) throw new RdfQueryException("Cannot evaluate the ARQ strjoin() function when the separator expression evaluates to a Null");
                        if (sep.NodeType == NodeType.Literal)
                        {
                            output.Append(((ILiteralNode)sep).Value);
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot evaluate the ARQ strjoin() function when the separator expression evaluates to a non-Literal Node");
                        }
                    }
                }
            }

            return new LiteralNode(null, output.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the effective value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                return (from expr in this._exprs
                        from v in expr.Variables
                        select v);
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(ArqFunctionFactory.ArqFunctionsNamespace);
            output.Append(ArqFunctionFactory.StrJoin);
            output.Append(">(");
            output.Append(this._sep.ToString());
            output.Append(",");
            for (int i = 0; i < this._exprs.Count; i++)
            {
                output.Append(this._exprs[i].ToString());
                if (i < this._exprs.Count - 1) output.Append(',');
            }
            output.Append(")");
            return output.ToString();
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
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.StrJoin;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return this._sep.AsEnumerable().Concat(this._exprs);
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new ArqStringJoinFunction(transformer.Transform(this._sep), this._exprs.Select(e => transformer.Transform(e)));
        }
    }
}
