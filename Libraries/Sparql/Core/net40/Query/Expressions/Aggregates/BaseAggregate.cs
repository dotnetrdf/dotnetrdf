using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions.Aggregates
{
    public abstract class BaseAggregate 
        : IAggregateExpression
    {
        public IValuedNode Evaluate(ISolution set, IExpressionContext context)
        {
            throw new RdfQueryException("Aggregates cannot be invoked on a single solution");
        }

        public abstract IEnumerable<string> Variables { get; }

        public abstract string Functor { get; }

        public virtual bool CanParallelise
        {
            get { return false; }
        }

        public virtual bool IsDeterministic
        {
            get { return true; }
        }

        public virtual bool IsConstant
        {
            get { return false; }
        }

        public abstract void Accept(IExpressionVisitor visitor);

        public abstract bool Equals(IExpression other);

        public override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        public abstract string ToString(IAlgebraFormatter formatter);

        public string ToPrefixString()
        {
            return ToString(new AlgebraFormatter());
        }

        public abstract string ToPrefixString(IAlgebraFormatter formatter);

        public virtual IExpression Copy()
        {
            return Copy(this.Arguments.Select(arg => arg.Copy()));
        }

        public abstract IAccumulator CreateAccumulator();

        public abstract IEnumerable<IExpression> Arguments { get; }

        public abstract IExpression Copy(IEnumerable<IExpression> arguments);

        public abstract override bool Equals(Object other);

        public abstract override int GetHashCode();
    }
}