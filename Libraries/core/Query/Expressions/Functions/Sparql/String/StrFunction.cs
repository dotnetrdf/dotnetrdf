using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Class representing the Sparql Str() function
    /// </summary>
    public class StrFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Str() function expression
        /// </summary>
        /// <param name="expr">Expression to apply the function to</param>
        public StrFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode result = this._expr.Evaluate(context, bindingID);
            if (result == null)
            {
                throw new RdfQueryException("Cannot return the lexical value of an NULL");
            }
            else
            {
                switch (result.NodeType)
                {
                    case NodeType.Literal:
                    case NodeType.Uri:
                        return new StringNode(null, result.AsString());

                    default:
                        throw new RdfQueryException("Cannot return the lexical value of Nodes which are not Literal/URI Nodes");

                }
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "STR(" + this._expr.ToString() + ")";
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
                return SparqlSpecsHelper.SparqlKeywordStr;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new StrFunction(transformer.Transform(this._expr));
        }
    }
}
