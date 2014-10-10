using System;
using System.Collections.Generic;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions
{
    public abstract class BaseAlgebraExpression 
        : IAlgebraExpression
    {
        protected BaseAlgebraExpression(IAlgebra algebra)
        {
            this.Algebra = algebra;
        }

        public abstract bool CanParallelise { get; }

        public abstract bool IsDeterministic { get; }

        public abstract bool IsConstant { get; }

        public IEnumerable<string> Variables
        {
            get { return this.Algebra.ProjectedVariables; }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public abstract string Functor { get; }

        public IAlgebra Algebra { get; private set; }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        public virtual string ToString(IAlgebraFormatter formatter)
        {
            throw new NotImplementedException("Printing algebra expressions in SPARQL syntax is not yet implemented");
        }

        public string ToPrefixString()
        {
            return ToPrefixString(new AlgebraFormatter());
        }

        public virtual string ToPrefixString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            StringBuilder builder = new StringBuilder();
            builder.Append('(');
            builder.Append(SparqlSpecsHelper.IsFunctionKeyword11(this.Functor) ? this.Functor.ToLowerInvariant() : String.Format("<{0}>", formatter.FormatUri(this.Functor)));
            builder.Append(this.Algebra.ToString(formatter));
            builder.Append(')');
            return builder.ToString();
        }

        public IExpression Copy()
        {
            return Copy(this.Algebra.Copy());
        }

        public abstract IValuedNode Evaluate(ISolution set, IExpressionContext context);

        public bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is IAlgebraExpression)) return false;

            IAlgebraExpression expr = (IAlgebraExpression) other;
            return this.Functor.Equals(expr.Functor) && this.Algebra.Equals(expr.Algebra);
        }

        public abstract IExpression Copy(IAlgebra algebra);
    }
}