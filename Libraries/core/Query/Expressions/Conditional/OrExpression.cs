using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Conditional
{
    /// <summary>
    /// Class representing Conditional Or expressions
    /// </summary>
    public class OrExpression
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new Conditional Or Expression
        /// </summary>
        /// <param name="leftExpr">Left Hand Expression</param>
        /// <param name="rightExpr">Right Hand Expression</param>
        public OrExpression(ISparqlExpression leftExpr, ISparqlExpression rightExpr) : base(leftExpr, rightExpr) { }

        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            //Lazy Evaluation for efficiency
            try
            {
                bool leftResult = this._leftExpr.Evaluate(context, bindingID).AsBoolean();
                if (leftResult)
                {
                    //If the LHS is true it doesn't matter about any subsequent results
                    return new BooleanNode(null, true);
                }
                else
                {
                    //If the LHS is false then we have to evaluate the RHS
                    return new BooleanNode(null, this._rightExpr.Evaluate(context, bindingID).AsBoolean());
                }
            }
            catch (Exception ex)
            {
                //If there's an Error on the LHS we return true only if the RHS evaluates to true
                //Otherwise we throw the Error
                bool rightResult = this._rightExpr.Evaluate(context, bindingID).AsSafeBoolean();
                if (rightResult)
                {
                    return new BooleanNode(null, true);
                }
                else
                {
                    //Ensure the error we throw is a RdfQueryException so as not to cause issues higher up
                    if (ex is RdfQueryException)
                    {
                        throw;
                    }
                    else
                    {
                        throw new RdfQueryException("Error evaluating OR expression", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (this._leftExpr.Type == SparqlExpressionType.BinaryOperator)
            {
                output.Append("(" + this._leftExpr.ToString() + ")");
            }
            else
            {
                output.Append(this._leftExpr.ToString());
            }
            output.Append(" || ");
            if (this._rightExpr.Type == SparqlExpressionType.BinaryOperator)
            {
                output.Append("(" + this._rightExpr.ToString() + ")");
            }
            else
            {
                output.Append(this._rightExpr.ToString());
            }
            return output.ToString();
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.BinaryOperator;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return "||";
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new OrExpression(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }
}
