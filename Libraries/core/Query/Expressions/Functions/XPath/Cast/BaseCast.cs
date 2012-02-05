using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.XPath.Cast
{
    /// <summary>
    /// Abstract Expression class used as the base class for implementation of XPath Casting Function expressions
    /// </summary>
    public abstract class BaseCast
        : ISparqlExpression
    {
        /// <summary>
        /// Expression to be Cast by the Cast Function
        /// </summary>
        protected ISparqlExpression _expr;

        /// <summary>
        /// Creates a new Base XPath Cast Expression
        /// </summary>
        /// <param name="expr">Expression to be Cast</param>
        public BaseCast(ISparqlExpression expr)
        {
            this._expr = expr;
        }

        /// <summary>
        /// Gets the value of casting the result of the inner expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public abstract IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID);

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets the enumeration of Variables involved in this expression
        /// </summary>
        public virtual IEnumerable<string> Variables
        {
            get
            {
                return this._expr.Variables;
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public abstract string Functor
        {
            get;
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return this._expr.AsEnumerable();
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public abstract ISparqlExpression Transform(IExpressionTransformer transformer);
    }
}
