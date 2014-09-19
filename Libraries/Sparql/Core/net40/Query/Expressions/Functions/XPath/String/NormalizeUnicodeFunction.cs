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

using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Factories;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
#if !NO_NORM

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
        public NormalizeUnicodeFunction(IExpression stringExpr)
            : base(stringExpr, null, XPathFunctionFactory.AcceptStringArguments) { }

        /// <summary>
        /// Creates a new XPath Normalize Unicode function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="normalizationFormExpr">Normalization Form</param>
        public NormalizeUnicodeFunction(IExpression stringExpr, IExpression normalizationFormExpr)
            : base(stringExpr, normalizationFormExpr, XPathFunctionFactory.AcceptStringArguments) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        public override IValuedNode ValueInternal(INode stringLit)
        {
            return new StringNode(stringLit.Value.Normalize(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal and Argument
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <param name="arg">Argument</param>
        /// <returns></returns>
        public override IValuedNode ValueInternal(INode stringLit, INode arg)
        {
            if (arg == null)
            {
                return this.ValueInternal(stringLit);
            }
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
                    //No Normalization
                    break;
                default:
                    throw new RdfQueryException("'" + arg.Value + "' is not a valid Normalization Form as defined by the XPath specification");
            }

            return new StringNode(normalized, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        public override IExpression Copy(IExpression arg1, IExpression arg2)
        {
            return new NormalizeUnicodeFunction(arg1, arg2);
        }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is NormalizeUnicodeFunction)) return false;

            NormalizeUnicodeFunction func = (NormalizeUnicodeFunction) other;
            return this.FirstArgument.Equals(func.FirstArgument) && this.SecondArgument.Equals(func.SecondArgument);
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
    }

#endif
}
