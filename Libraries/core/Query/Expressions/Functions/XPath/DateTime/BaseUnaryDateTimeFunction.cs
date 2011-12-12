using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.XPath.DateTime
{
    /// <summary>
    /// Abstract Base Class for functions which are Unary functions applied to Date Time objects in the XPath function library
    /// </summary>
    public abstract class BaseUnaryDateTimeFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Unary XPath Date Time function
        /// </summary>
        /// <param name="expr"></param>
        public BaseUnaryDateTimeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the numeric value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = this._expr.Evaluate(context, bindingID);
            if (temp != null)
            {
                return this.ValueInternal(temp.AsDateTime());
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate an XPath Date Time function on a null argument");
            }
        }

        /// <summary>
        /// Abstract method which derived classes must implement to generate the actual numeric value for the function
        /// </summary>
        /// <param name="dateTime">Date Time</param>
        /// <returns></returns>
        protected abstract IValuedNode ValueInternal(DateTimeOffset dateTime);

        /// <summary>
        /// Gets the String representation of the Function
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

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
    }
}
