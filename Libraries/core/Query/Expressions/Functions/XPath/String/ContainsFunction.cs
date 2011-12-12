using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Represents the XPath fn:contains() function
    /// </summary>
    public class ContainsFunction
        : BaseBinaryStringFunction
    {
        /// <summary>
        /// Creates a new XPath Contains function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="searchExpr">Search Expression</param>
        public ContainsFunction(ISparqlExpression stringExpr, ISparqlExpression searchExpr)
            : base(stringExpr, searchExpr, false, XPathFunctionFactory.AcceptStringArguments) { }

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
                //Empty string cannot contain anything
                return new BooleanNode(null, false);
            }
            else if (arg.Value.Equals(string.Empty))
            {
                //Any non-empty string contains the empty string
                return new BooleanNode(null, true);
            }
            else
            {
                //Evalute the Contains
                return new BooleanNode(null, stringLit.Value.Contains(arg.Value));
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Contains + ">(" + this._expr.ToString() + "," + this._arg.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Contains;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new ContainsFunction(transformer.Transform(this._expr), transformer.Transform(this._arg));
        }
    }
}
