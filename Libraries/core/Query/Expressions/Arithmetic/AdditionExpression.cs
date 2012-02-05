using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Arithmetic
{
    /// <summary>
    /// Class representing Arithmetic Addition expressions
    /// </summary>
    public class AdditionExpression
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new Addition Expression
        /// </summary>
        /// <param name="leftExpr">Left Hand Expression</param>
        /// <param name="rightExpr">Right Hand Expression</param>
        public AdditionExpression(ISparqlExpression leftExpr, ISparqlExpression rightExpr) 
            : base(leftExpr, rightExpr) { }

        /// <summary>
        /// Calculates the Numeric Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode a = this._leftExpr.Evaluate(context, bindingID);
            IValuedNode b = this._rightExpr.Evaluate(context, bindingID);
            if (a == null || b == null) throw new RdfQueryException("Cannot apply addition when one/both arguments are null");

            SparqlNumericType type = (SparqlNumericType)Math.Max((int)a.NumericType, (int)b.NumericType);

            switch (type)
            {
                case SparqlNumericType.Integer:
                    return new LongNode(null, a.AsInteger() + b.AsInteger());
                case SparqlNumericType.Decimal:
                    return new DecimalNode(null, a.AsDecimal() + b.AsDecimal());
                case SparqlNumericType.Float:
                    return new FloatNode(null, a.AsFloat() + b.AsFloat());
                case SparqlNumericType.Double:
                    return new DoubleNode(null, a.AsDouble() + b.AsDouble());
                default:
                    throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
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
            output.Append(" + ");
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
                return "+";
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new AdditionExpression(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }
}
