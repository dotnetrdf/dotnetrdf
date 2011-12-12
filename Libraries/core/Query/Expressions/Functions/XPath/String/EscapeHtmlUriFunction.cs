using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NO_WEB
using System.Web;
#endif
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Represents the XPath fn:escape-html-uri() function
    /// </summary>
    public class EscapeHtmlUriFunction
        : BaseUnaryStringFunction
    {
        /// <summary>
        /// Creates a new XPath Escape HTML for URI function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        public EscapeHtmlUriFunction(ISparqlExpression stringExpr)
            : base(stringExpr) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        protected override IValuedNode ValueInternal(ILiteralNode stringLit)
        {
            return new StringNode(null, HttpUtility.UrlEncode(stringLit.Value), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.EscapeHtmlURI + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.EscapeHtmlURI;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new EscapeHtmlUriFunction(transformer.Transform(this._expr));
        }
    }
}
