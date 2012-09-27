/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Nodes;

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
