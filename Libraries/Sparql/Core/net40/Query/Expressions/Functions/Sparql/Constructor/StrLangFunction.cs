/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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

using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;

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
        public StrLangFunction(IExpression stringExpr, IExpression langExpr)
            : base(stringExpr, langExpr) {}

        public override IExpression Copy(IExpression arg1, IExpression arg2)
        {
            return new StrLangFunction(arg1, arg2);
        }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            INode s = this.FirstArgument.Evaluate(solution, context);
            INode lang = this.SecondArgument.Evaluate(solution, context);

            if (s == null) throw new RdfQueryException("Cannot create a language specified literal from a null string");
            if (lang == null) throw new RdfQueryException("Cannot create a language specified literal from a null string");
            string langSpec;
            if (lang.NodeType == NodeType.Literal)
            {
                INode langLit = lang;
                if (!langLit.HasDataType && !langLit.HasLanguage)
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
            if (s.NodeType != NodeType.Literal) throw new RdfQueryException("Cannot create a language specified literal from a non-literal Node");
            INode lit = s;
            if (lit.HasLanguage) throw new RdfQueryException("Cannot create a language specified literal from a language specified literal");
            if (lit.HasDataType) throw new RdfQueryException("Cannot create a language specified literal from a typed literal");
            return new StringNode(lit.Value, langSpec);
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordStrLang; }
        }
    }
}