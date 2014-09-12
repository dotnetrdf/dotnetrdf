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

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Represents the SPARQL STRAFTER Function
    /// </summary>
    public class StrAfterFunction
    : IExpression
    {
        private IExpression _stringExpr, _startsExpr;

        /// <summary>
        /// Creates a new STRAFTER Function
        /// </summary>
        /// <param name="stringExpr">String Expression</param>
        /// <param name="startsExpr">Starts Expression</param>
        public StrAfterFunction(IExpression stringExpr, IExpression startsExpr)
        {
            this._stringExpr = stringExpr;
            this._startsExpr = startsExpr;
        }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            INode input = this.CheckArgument(this._stringExpr, solution, context);
            INode starts = this.CheckArgument(this._startsExpr, solution, context);

            if (!this.IsValidArgumentPair(input, starts)) throw new RdfQueryException("The Literals provided as arguments to this SPARQL String function are not of valid forms (see SPARQL spec for acceptable combinations)");

            Uri datatype = input.DataType;//(input.DataType != null ? input.DataType : starts.DataType);
            string lang = input.Language;//(!input.Language.Equals(string.Empty) ? input.Language : starts.Language);

            if (input.Value.Contains(starts.Value))
            {
                int startIndex = input.Value.IndexOf(starts.Value) + starts.Value.Length;
                string resultValue = (startIndex >= input.Value.Length ? string.Empty : input.Value.Substring(startIndex));

                if (datatype != null)
                {
                    return new StringNode(resultValue, datatype);
                }
                else if (!lang.Equals(string.Empty))
                {
                    return new StringNode(resultValue, lang);
                }
                else
                {
                    return new StringNode(resultValue);
                }
            }
            else if (starts.Value.Equals(string.Empty))
            {
                if (datatype != null)
                {
                    return new StringNode(string.Empty, datatype);
                }
                else
                {
                    return new StringNode(string.Empty/*, lang*/);
                }
            }
            else
            {
                return new StringNode(string.Empty);
            }
        }

        private INode CheckArgument(IExpression expr, ISolution solution, IExpressionContext context)
        {
            return this.CheckArgument(expr, solution, context, XPathFunctionFactory.AcceptStringArguments);
        }

        private INode CheckArgument(IExpression expr, ISolution solution, IExpressionContext context, Func<Uri, bool> argumentTypeValidator)
        {
            INode temp = expr.Evaluate(solution, context);
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
                            return lit;
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evaluate as one of the argument expressions returned a typed literal with an invalid type");
                        }
                    }
                    else if (argumentTypeValidator(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString)))
                    {
                        //Untyped Literals are treated as Strings and may be returned when the argument allows strings
                        return lit;
                    }
                    else
                    {
                        throw new RdfQueryException("Unable to evalaute as one of the argument expressions returned an untyped literal");
                    }
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate as one of the argument expressions returned a non-literal");
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate as one of the argument expressions evaluated to null");
            }
        }

        /// <summary>
        /// Determines whether the Arguments are valid
        /// </summary>
        /// <param name="stringLit">String Literal</param>
        /// <param name="argLit">Argument Literal</param>
        /// <returns></returns>
        protected bool IsValidArgumentPair(INode stringLit, INode argLit)
        {
            if (stringLit.DataType != null)
            {
                //If 1st argument has a DataType must be an xsd:string or not valid
                if (!stringLit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString)) return false;

                if (argLit.DataType != null)
                {
                    //If 2nd argument also has a DataType must also be an xsd:string or not valid
                    if (!argLit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString)) return false;
                    return true;
                }
                else if (argLit.Language.Equals(string.Empty))
                {
                    //If 2nd argument does not have a DataType but 1st does then 2nd argument must have no
                    //Language Tag
                    return true;
                }
                else
                {
                    //2nd argument does not have a DataType but 1st does BUT 2nd has a Language Tag so invalid
                    return false;
                }
            }
            else if (!stringLit.Language.Equals(string.Empty))
            {
                if (argLit.DataType != null)
                {
                    //If 1st argument has a Language Tag and 2nd Argument is typed then must be xsd:string
                    //to be valid
                    return argLit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString);
                }
                else if (argLit.Language.Equals(string.Empty) || stringLit.Language.Equals(argLit.Language))
                {
                    //If 1st argument has a Language Tag then 2nd Argument must have same Language Tag 
                    //or no Language Tag in order to be valid
                    return true;
                }
                else
                {
                    //Otherwise Invalid
                    return false;
                }
            }
            else
            {
                if (argLit.DataType != null)
                {
                    //If 1st argument is plain literal then 2nd argument must be xsd:string if typed
                    return argLit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString);
                }
                else if (argLit.Language.Equals(string.Empty))
                {
                    //If 1st argument is plain literal then 2nd literal cannot have a language tag to be valid
                    return true;
                }
                else
                {
                    //If 1st argument is plain literal and 2nd has language tag then invalid
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                return this._startsExpr.Variables.Concat(this._stringExpr.Variables);
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
                return SparqlSpecsHelper.SparqlKeywordStrAfter;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Function
        /// </summary>
        public IEnumerable<IExpression> Arguments
        {
            get
            {
                return new IExpression[] { this._stringExpr, this._startsExpr };
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return this._stringExpr.CanParallelise && this._stringExpr.CanParallelise;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public IExpression Transform(IExpressionTransformer transformer)
        {
            return new StrAfterFunction(transformer.Transform(this._stringExpr), transformer.Transform(this._startsExpr));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordStrAfter + "(" + this._stringExpr.ToString() + ", " + this._startsExpr.ToString() + ")";
        }
    }
}
