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

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Constructor
{
    /// <summary>
    /// Class representing the Sparql StrDt() function
    /// </summary>
    public class StrLangFunction
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new STRLANG() function expression
        /// </summary>
        /// <param name="stringExpr">String Expression</param>
        /// <param name="langExpr">Language Expression</param>
        public StrLangFunction(ISparqlExpression stringExpr, ISparqlExpression langExpr)
            : base(stringExpr, langExpr) { }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            INode s = this._leftExpr.Evaluate(context, bindingID);
            INode lang = this._rightExpr.Evaluate(context, bindingID);

            if (s != null)
            {
                if (lang != null)
                {
                    string langSpec;
                    if (lang.NodeType == NodeType.Literal)
                    {
                        ILiteralNode langLit = (ILiteralNode)lang;
                        if (langLit.DataType == null)
                        {
                            langSpec = langLit.Value;
                        }
                        else
                        {
                            if (langLit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                            {
                                langSpec = langLit.Value;
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot create a language specified literal when the language is a non-string literal");
                            }
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Cannot create a language specified literal when the language is a non-literal Node");
                    }
                    if (s.NodeType == NodeType.Literal)
                    {
                        ILiteralNode lit = (ILiteralNode)s;
                        if (lit.DataType == null)
                        {
                            if (lit.Language.Equals(string.Empty))
                            {
                                return new StringNode(lit.Value, langSpec);
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot create a language specified literal from a language specified literal");
                            }
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot create a language specified literal from a typed literal");
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Cannot create a language specified literal from a non-literal Node");
                    }
                }
                else
                {
                    throw new RdfQueryException("Cannot create a language specified literal from a null string");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot create a language specified literal from a null string");
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "STRLANG(" + this._leftExpr.ToString() + ", " + this._rightExpr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordStrLang;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new StrLangFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }
}
