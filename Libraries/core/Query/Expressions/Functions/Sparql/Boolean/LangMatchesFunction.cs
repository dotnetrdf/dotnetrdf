using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Boolean
{
    /// <summary>
    /// Class representing the Sparql LangMatches() function
    /// </summary>
    public class LangMatchesFunction
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new LangMatches() function expression
        /// </summary>
        /// <param name="term">Expression to obtain the Language of</param>
        /// <param name="langRange">Expression representing the Language Range to match</param>
        public LangMatchesFunction(ISparqlExpression term, ISparqlExpression langRange)
            : base(term, langRange) { }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._leftExpr.Evaluate(context, bindingID);
            INode langRange = this._rightExpr.Evaluate(context, bindingID);

            if (result == null)
            {
                return new BooleanNode(null, false);
            }
            else if (result.NodeType == NodeType.Literal)
            {
                if (langRange == null)
                {
                    return new BooleanNode(null, false);
                }
                else if (langRange.NodeType == NodeType.Literal)
                {
                    string range = ((ILiteralNode)langRange).Value;
                    string lang = ((ILiteralNode)result).Value;

                    if (range.Equals("*"))
                    {
                        return new BooleanNode(null, !lang.Equals(string.Empty));
                    }
                    else
                    {
                        return new BooleanNode(null, lang.Equals(range, StringComparison.OrdinalIgnoreCase) || lang.StartsWith(range + "-", StringComparison.OrdinalIgnoreCase));
                    }
                }
                else
                {
                    return new BooleanNode(null, false);
                }
            }
            else
            {
                return new BooleanNode(null, false);
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "LANGMATCHES(" + this._leftExpr.ToString() + "," + this._rightExpr.ToString() + ")";
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
                return SparqlSpecsHelper.SparqlKeywordLangMatches;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LangMatchesFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }
}
