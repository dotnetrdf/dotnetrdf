using System;
using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Abstract base class for Unary Expressions
    /// </summary>
    public abstract class BaseUnaryExpression 
        : IUnaryExpression
    {
        /// <summary>
        /// Creates a new Base Unary Expression
        /// </summary>
        /// <param name="argument">Argument</param>
        protected BaseUnaryExpression(IExpression argument)
        {
            this.Argument = argument;
        }

        /// <summary>
        /// The sub-expression of this Expression
        /// </summary>
        public IExpression Argument { get; set; }

        public abstract IExpression Copy(IExpression argument);

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public abstract IValuedNode Evaluate(ISolution solution, IExpressionContext context);

        public abstract bool Equals(IExpression other);

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets an enumeration of all the Variables used in this expression
        /// </summary>
        public virtual IEnumerable<string> Variables
        {
            get
            {
                return this.Argument.Variables;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public abstract String Functor
        {
            get;
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return this.Argument.CanParallelise;
            }
        }

        public virtual bool IsDeterministic { get { return this.Argument.IsDeterministic; } }

        public bool IsConstant { get { return false; } }
    }
}