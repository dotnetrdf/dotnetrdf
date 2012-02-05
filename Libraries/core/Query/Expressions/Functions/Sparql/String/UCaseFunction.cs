using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Functions.XPath.String;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Represents the SPARQL UCASE Function
    /// </summary>
    public class UCaseFunction
        : BaseUnaryStringFunction
    {
        /// <summary>
        /// Creates a new UCASE() function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public UCaseFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Converts the given String Literal to upper case
        /// </summary>
        /// <param name="stringLit">String Literal</param>
        /// <returns></returns>
        protected override IValuedNode ValueInternal(ILiteralNode stringLit)
        {
            if (stringLit.DataType != null)
            {
                return new StringNode(null, stringLit.Value.ToUpper(), stringLit.DataType);
            }
            else
            {
                return new StringNode(null, stringLit.Value.ToUpper(), stringLit.Language);
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordUCase;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordUCase + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new UCaseFunction(transformer.Transform(this._expr));
        }

    }
}
