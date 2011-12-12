using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Class representing the Sparql Lang() function
    /// </summary>
    public class LangFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Lang() function expression
        /// </summary>
        /// <param name="expr">Expression to apply the function to</param>
        public LangFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._expr.Evaluate(context, bindingID);
            if (result == null)
            {
                throw new RdfQueryException("Cannot return the Data Type URI of an NULL");
            }
            else
            {
                switch (result.NodeType)
                {
                    case NodeType.Literal:
                        return new StringNode(null, ((ILiteralNode)result).Language);

                    case NodeType.Uri:
                    case NodeType.Blank:
                    case NodeType.GraphLiteral:
                    default:
                        throw new RdfQueryException("Cannot return the Language Tag of Nodes which are not Literal Nodes");

                }
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "LANG(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordLang;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LangFunction(transformer.Transform(this._expr));
        }
    }
}
