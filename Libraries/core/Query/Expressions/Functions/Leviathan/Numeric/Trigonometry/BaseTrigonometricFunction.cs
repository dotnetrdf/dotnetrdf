using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric.Trigonometry
{
    /// <summary>
    /// Abstract Base Class for Unary Trigonometric Functions in the Leviathan Function Library
    /// </summary>
    public abstract class BaseTrigonometricFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Trigonometric function
        /// </summary>
        protected Func<double, double> _func;

        /// <summary>
        /// Creates a new Unary Trigonometric Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public BaseTrigonometricFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Creates a new Unary Trigonometric Function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="func">Trigonometric Function</param>
        public BaseTrigonometricFunction(ISparqlExpression expr, Func<double, double> func)
            : base(expr)
        {
            this._func = func;
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = this._expr.Evaluate(context, bindingID);
            if (temp == null) throw new RdfQueryException("Cannot apply a trigonometric function to a null");

            if (temp.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot apply a trigonometric function to a non-numeric argument");

            return new DoubleNode(null, this._func(temp.AsDouble()));
        }

        /// <summary>
        /// Gets the expression type
        /// </summary>
        public override SparqlExpressionType Type
        {
            get 
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the string representation of the Function
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }
}
