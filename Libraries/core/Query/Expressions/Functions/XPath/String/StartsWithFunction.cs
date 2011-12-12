using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Represents the XPath fn:starts-with() function
    /// </summary>
    public class StartsWithFunction
        : BaseBinaryStringFunction
    {
        /// <summary>
        /// Creates a new XPath Starts With function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="prefixExpr">Prefix Expression</param>
        public StartsWithFunction(ISparqlExpression stringExpr, ISparqlExpression prefixExpr)
            : base(stringExpr, prefixExpr, false, XPathFunctionFactory.AcceptStringArguments) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal and Argument
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <param name="arg">Argument</param>
        /// <returns></returns>
        public override IValuedNode ValueInternal(ILiteralNode stringLit, ILiteralNode arg)
        {
            if (stringLit.Value.Equals(string.Empty))
            {
                if (arg.Value.Equals(string.Empty))
                {
                    //The Empty String starts with the Empty String
                    return new BooleanNode(null, true);
                }
                else
                {
                    //Empty String doesn't start with a non-empty string
                    return new BooleanNode(null, false);
                }
            }
            else if (arg.Value.Equals(string.Empty))
            {
                //Any non-empty string starts with the empty string
                return new BooleanNode(null, true);
            }
            else
            {
                //Otherwise evalute the StartsWith
                return new BooleanNode(null, stringLit.Value.StartsWith(arg.Value));
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.StartsWith + ">(" + this._expr.ToString() + "," + this._arg.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.StartsWith;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new StartsWithFunction(transformer.Transform(this._expr), transformer.Transform(this._arg));
        }
    }
}
