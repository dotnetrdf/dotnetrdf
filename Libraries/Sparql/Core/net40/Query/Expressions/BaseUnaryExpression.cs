using System;
using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;
using VDS.RDF.Writing.Formatting;

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

        public virtual IExpression Copy()
        {
            return Copy(this.Argument.Copy());
        }

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
        public sealed override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        public string ToString(IAlgebraFormatter formatter)
        {
            String f = SparqlSpecsHelper.IsFunctionKeyword11(this.Functor) ? this.Functor.ToLowerInvariant() : formatter.FormatUri(this.Functor);
            return String.Format("{0}({1})", f, this.Argument.ToString(formatter));
        }

        public string ToPrefixString()
        {
            return ToPrefixString(new AlgebraFormatter());
        }

        public string ToPrefixString(IAlgebraFormatter formatter)
        {
            String f = SparqlSpecsHelper.IsFunctionKeyword11(this.Functor) ? this.Functor.ToLowerInvariant() : formatter.FormatUri(this.Functor);
            return String.Format("({0} {1})", f, this.Argument.ToPrefixString(formatter));
        }

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
        public abstract bool CanParallelise { get; }

        public abstract bool IsDeterministic { get; }

        public abstract bool IsConstant { get; }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(Object other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is BaseUnaryExpression)) return false;

            return this.Equals((BaseUnaryExpression) other);
        }

        public abstract override int GetHashCode();
    }
}