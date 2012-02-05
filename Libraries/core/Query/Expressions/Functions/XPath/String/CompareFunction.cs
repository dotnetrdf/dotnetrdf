using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Represents the XPath fn:compare() function
    /// </summary>
    public class CompareFunction
        : BaseBinaryStringFunction
    {
        /// <summary>
        /// Creates a new XPath Compare function
        /// </summary>
        /// <param name="a">First Comparand</param>
        /// <param name="b">Second Comparand</param>
        public CompareFunction(ISparqlExpression a, ISparqlExpression b)
            : base(a, b, false, XPathFunctionFactory.AcceptStringArguments) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal and Argument
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <param name="arg">Argument</param>
        /// <returns></returns>
        public override IValuedNode ValueInternal(ILiteralNode stringLit, ILiteralNode arg)
        {
            return new LongNode(null, string.Compare(stringLit.Value, arg.Value));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Compare + ">(" + this._expr.ToString() + "," + this._arg.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Compare;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new CompareFunction(transformer.Transform(this._expr), transformer.Transform(this._arg));
        }
    }
}
