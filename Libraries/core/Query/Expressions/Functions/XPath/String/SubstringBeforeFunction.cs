using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Represents the XPath fn:substring-before() function
    /// </summary>
    public class SubstringBeforeFunction
        : BaseBinaryStringFunction
    {
        /// <summary>
        /// Creates a new XPath Substring Before function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="findExpr">Search Expression</param>
        public SubstringBeforeFunction(ISparqlExpression stringExpr, ISparqlExpression findExpr)
            : base(stringExpr, findExpr, false, XPathFunctionFactory.AcceptStringArguments) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal and Argument
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <param name="arg">Argument</param>
        /// <returns></returns>
        public override IValuedNode ValueInternal(ILiteralNode stringLit, ILiteralNode arg)
        {
            if (arg.Value.Equals(string.Empty))
            {
                //The substring before the empty string is the empty string
                return new StringNode(null, string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            }
            else
            {
                //Does the String contain the search string?
                if (stringLit.Value.Contains(arg.Value))
                {
                    string result = stringLit.Value.Substring(0, stringLit.Value.IndexOf(arg.Value));
                    return new StringNode(null, result, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                else
                {
                    //If it doesn't contain the search string the empty string is returned
                    return new StringNode(null, string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.SubstringBefore + ">(" + this._expr.ToString() + "," + this._arg.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.SubstringBefore;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new SubstringBeforeFunction(transformer.Transform(this._expr), transformer.Transform(this._arg));
        }
    }
}
