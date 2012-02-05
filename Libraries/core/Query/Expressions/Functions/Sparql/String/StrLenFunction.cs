using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Represents the SPARQL STRLEN Function
    /// </summary>
    public class StrLenFunction
        : XPath.String.BaseUnaryStringFunction
    {
        /// <summary>
        /// Creates a new STRLEN() function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public StrLenFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Determines the Length of the given String Literal
        /// </summary>
        /// <param name="stringLit">String Literal</param>
        /// <returns></returns>
        protected override IValuedNode ValueInternal(ILiteralNode stringLit)
        {
            return new LongNode(null, stringLit.Value.Length);
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordStrLen;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordStrLen + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new StrLenFunction(transformer.Transform(this._expr));
        }
    }
}
