using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Represents the XPath fn:substring-after() function
    /// </summary>
    public class SubstringAfterFunction
        : BaseBinaryStringFunction
    {
        /// <summary>
        /// Creates a new XPath Substring After function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="findExpr">Search Expression</param>
        public SubstringAfterFunction(ISparqlExpression stringExpr, ISparqlExpression findExpr)
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
                //The substring after the empty string is the input string
                return new StringNode(null, stringLit.Value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            }
            else
            {
                //Does the String contain the search string?
                if (stringLit.Value.Contains(arg.Value))
                {
                    string result = stringLit.Value.Substring(stringLit.Value.IndexOf(arg.Value) + arg.Value.Length);
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
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.SubstringAfter + ">(" + this._expr.ToString() + "," + this._arg.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.SubstringAfter;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new SubstringAfterFunction(transformer.Transform(this._expr), transformer.Transform(this._arg));
        }
    }
}
