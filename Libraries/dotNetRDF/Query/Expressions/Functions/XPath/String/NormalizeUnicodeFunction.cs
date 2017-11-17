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
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Represents the XPath fn:normalize-unicode() function
    /// </summary>
    public class NormalizeUnicodeFunction
        : BaseBinaryStringFunction
    {
        /// <summary>
        /// Creates a new XPath Normalize Unicode function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        public NormalizeUnicodeFunction(ISparqlExpression stringExpr)
            : base(stringExpr, null, true, XPathFunctionFactory.AcceptStringArguments) { }

        /// <summary>
        /// Creates a new XPath Normalize Unicode function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="normalizationFormExpr">Normalization Form</param>
        public NormalizeUnicodeFunction(ISparqlExpression stringExpr, ISparqlExpression normalizationFormExpr)
            : base(stringExpr, normalizationFormExpr, true, XPathFunctionFactory.AcceptStringArguments) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        public override IValuedNode ValueInternal(ILiteralNode stringLit)
        {
            return new StringNode(null, stringLit.Value.Normalize(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal and Argument
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <param name="arg">Argument</param>
        /// <returns></returns>
        public override IValuedNode ValueInternal(ILiteralNode stringLit, ILiteralNode arg)
        {
            if (arg == null)
            {
                return ValueInternal(stringLit);
            }
            else
            {
                string normalized = stringLit.Value;

                switch (arg.Value)
                {
                    case XPathFunctionFactory.XPathUnicodeNormalizationFormC:
                        normalized = normalized.Normalize();
                        break;
                    case XPathFunctionFactory.XPathUnicodeNormalizationFormD:
                        normalized = normalized.Normalize(NormalizationForm.FormD);
                        break;
                    case XPathFunctionFactory.XPathUnicodeNormalizationFormFull:
                        throw new RdfQueryException(".Net does not support Fully Normalized Unicode Form");
                    case XPathFunctionFactory.XPathUnicodeNormalizationFormKC:
                        normalized = normalized.Normalize(NormalizationForm.FormKC);
                        break;
                    case XPathFunctionFactory.XPathUnicodeNormalizationFormKD:
                        normalized = normalized.Normalize(NormalizationForm.FormKD);
                        break;
                    case "":
                        // No Normalization
                        break;
                    default:
                        throw new RdfQueryException("'" + arg.Value + "' is not a valid Normalization Form as defined by the XPath specification");
                }

                return new StringNode(null, normalized, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_arg != null)
            {
                return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.NormalizeUnicode + ">(" + _expr.ToString() + "," + _arg.ToString() + ")";
            }
            else
            {
                return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.NormalizeUnicode + ">(" + _expr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.NormalizeUnicode;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            if (_arg != null)
            {
                return new NormalizeUnicodeFunction(transformer.Transform(_expr), transformer.Transform(_arg));
            }
            else
            {
                return new NormalizeUnicodeFunction(transformer.Transform(_expr));
            }
        }
    }
}
