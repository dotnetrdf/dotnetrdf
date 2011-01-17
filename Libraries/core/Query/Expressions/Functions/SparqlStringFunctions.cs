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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Functions
{
    public class EncodeForUriFunction : BaseUnaryXPathStringFunction
    {
        /// <summary>
        /// Creates a new Encode for URI function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        public EncodeForUriFunction(ISparqlExpression stringExpr)
            : base(stringExpr) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        public override INode ValueInternal(LiteralNode stringLit)
        {
            if (stringLit.DataType != null)
            {
                return new LiteralNode(null, Uri.EscapeUriString(stringLit.Value), stringLit.DataType);
            }
            else
            {
                return new LiteralNode(null, Uri.EscapeDataString(stringLit.Value), stringLit.Language);
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordEncodeForUri + "(" + this._expr.ToString() + ")";
        }

        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordEncodeForUri;
            }
        }
    }

    public class LCaseFunction : BaseUnaryXPathStringFunction
    {
        public LCaseFunction(ISparqlExpression expr)
            : base(expr) { }

        public override INode ValueInternal(LiteralNode stringLit)
        {
            if (stringLit.DataType != null)
            {
                return new LiteralNode(null, stringLit.Value.ToLower(), stringLit.DataType);
            }
            else
            {
                return new LiteralNode(null, stringLit.Value.ToLower(), stringLit.Language);
            }
        }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordLCase;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordLCase + "(" + this._expr.ToString() + ")";
        }
    }

    public class StrLenFunction : BaseUnaryXPathStringFunction
    {
        public StrLenFunction(ISparqlExpression expr)
            : base(expr) { }

        public override INode ValueInternal(LiteralNode stringLit)
        {
            return new LiteralNode(null, stringLit.Value.Length.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        }

        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordStrLen; 
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordStrLen + "(" + this._expr.ToString() + ")";
        }
    }

    public class SubStrFunction : ISparqlExpression
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
            this._expr = stringExpr;
            this._start = startExpr;
            this._length = lengthExpr;
        }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            LiteralNode input = this.CheckArgument(this._expr, context, bindingID);
            LiteralNode start = this.CheckArgument(this._start, context, bindingID, XPathFunctionFactory.AcceptNumericArguments);

            if (this._length != null)
            {
                LiteralNode length = this.CheckArgument(this._length, context, bindingID, XPathFunctionFactory.AcceptNumericArguments);

                if (input.Value.Equals(String.Empty)) return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.Value);
                    int l = Convert.ToInt32(length.Value);

                    if (s < 1) s = 1;
                    if (l < 1)
                    {
                        //If no/negative characters are being selected the empty string is returned
                        if (input.DataType != null)
                        {
                            return new LiteralNode(null, String.Empty, input.DataType);
                        }
                        else 
                        {
                            return new LiteralNode(null, String.Empty, input.Language);
                        }
                    }
                    else if ((s-1) > input.Value.Length)
                    {
                        //If the start is after the end of the string the empty string is returned
                        if (input.DataType != null)
                        {
                            return new LiteralNode(null, String.Empty, input.DataType);
                        }
                        else 
                        {
                            return new LiteralNode(null, String.Empty, input.Language);
                        }                    }
                    else
                    {
                        if (((s - 1) + l) > input.Value.Length)
                        {
                            //If the start plus the length is greater than the length of the string the string from the starts onwards is returned
                            if (input.DataType != null)
                            {
                                return new LiteralNode(null, input.Value.Substring(s - 1), input.DataType);
                            } 
                            else
                            {
                                return new LiteralNode(null, input.Value.Substring(s - 1), input.Language);
                            }
                        }
                        else
                        {
                            //Otherwise do normal substring
                            if (input.DataType != null)
                            {
                                return new LiteralNode(null, input.Value.Substring(s - 1, l), input.DataType);
                            }
                            else
                            {
                                return new LiteralNode(null, input.Value.Substring(s - 1, l), input.Language);
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
                if (input.Value.Equals(String.Empty)) return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.Value);
                    if (s < 1) s = 1;

                    if (input.DataType != null)
                    {
                        return new LiteralNode(null, input.Value.Substring(s - 1), input.DataType);
                    }
                    else
                    {
                        return new LiteralNode(null, input.Value.Substring(s - 1), input.Language);
                    }
                }
                catch
                {
                    throw new RdfQueryException("Unable to convert the Start argument to an Integer");
                }
            }
        }

        private LiteralNode CheckArgument(ISparqlExpression expr, SparqlEvaluationContext context, int bindingID)
        {
            return this.CheckArgument(expr, context, bindingID, XPathFunctionFactory.AcceptStringArguments);
        }

        private LiteralNode CheckArgument(ISparqlExpression expr, SparqlEvaluationContext context, int bindingID, Func<Uri, bool> argumentTypeValidator)
        {
            INode temp = expr.Value(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Literal)
                {
                    LiteralNode lit = (LiteralNode)temp;
                    if (lit.DataType != null)
                    {
                        if (argumentTypeValidator(lit.DataType))
                        {
                            //Appropriately typed literals are fine
                            return lit;
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evaluate a substring as one of the argument expressions returned a typed literal with an invalid type");
                        }
                    }
                    else if (argumentTypeValidator(new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)))
                    {
                        //Untyped Literals are treated as Strings and may be returned when the argument allows strings
                        return lit;
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
        /// Returns the Effective Boolean Value of the Expression as evaluated for a given Binding as a Literal Node
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
                if (this._length != null)
                {
                    return this._expr.Variables.Concat(this._start.Variables).Concat(this._length.Variables);
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
            if (this._length != null)
            {
                return SparqlSpecsHelper.SparqlKeywordSubStr + "(" + this._expr.ToString() + "," + this._start.ToString() + "," + this._length.ToString() + ")";
            }
            else
            {
                return SparqlSpecsHelper.SparqlKeywordSubStr + "(" + this._expr.ToString() + "," + this._start.ToString() + ")";
            }
        }

        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        public string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSubStr;
            }
        }

        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                if (this._length != null)
                {
                    return new ISparqlExpression[] { this._expr, this._start, this._length };
                }
                else
                {
                    return new ISparqlExpression[] { this._expr, this._start };
                }
            }
        }
    }

    public class UCaseFunction : BaseUnaryXPathStringFunction
    {
        public UCaseFunction(ISparqlExpression expr)
            : base(expr) { }

        public override INode ValueInternal(LiteralNode stringLit)
        {
            if (stringLit.DataType != null)
            {
                return new LiteralNode(null, stringLit.Value.ToUpper(), stringLit.DataType);
            }
            else
            {
                return new LiteralNode(null, stringLit.Value.ToUpper(), stringLit.Language);
            }
        }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordUCase; 
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordUCase + "(" + this._expr.ToString() + ")";
        }
    }
}
