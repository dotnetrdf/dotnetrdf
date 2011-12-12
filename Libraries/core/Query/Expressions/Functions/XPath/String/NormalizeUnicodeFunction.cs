using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Nodes;

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
                return this.ValueInternal(stringLit);
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
                        //No Normalization
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
            if (this._arg != null)
            {
                return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.NormalizeUnicode + ">(" + this._expr.ToString() + "," + this._arg.ToString() + ")";
            }
            else
            {
                return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.NormalizeUnicode + ">(" + this._expr.ToString() + ")";
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
            if (this._arg != null)
            {
                return new NormalizeUnicodeFunction(transformer.Transform(this._expr), transformer.Transform(this._arg));
            }
            else
            {
                return new NormalizeUnicodeFunction(transformer.Transform(this._expr));
            }
        }
    }

#endif
}
