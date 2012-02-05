using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.XPath.Cast
{
    /// <summary>
    /// Class representing an XPath String Cast Function
    /// </summary>
    public class StringCast
        : BaseCast
    {
        /// <summary>
        /// Creates a new XPath String Cast Function Expression
        /// </summary>
        /// <param name="expr">Expression to be cast</param>
        public StringCast(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Casts the results of the inner expression to a Literal Node typed xsd:string
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode n = this._expr.Evaluate(context, bindingID);

            if (n == null)
            {
                throw new RdfQueryException("Cannot cast a Null to a xsd:string");
            }

            return new StringNode(null, n.AsString(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XmlSpecsHelper.XmlSchemaDataTypeString + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XmlSpecsHelper.XmlSchemaDataTypeString;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new StringCast(transformer.Transform(this._expr));
        }
    }
}
