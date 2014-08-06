using System.Collections.Generic;
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

        public bool CanParallelise
        {
            get { return false; }
        }

        public bool IsDeterministic
        {
            get { return true; }
        }

        public bool IsConstant
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

        public abstract IAccumulator CreateAccumulator();
    }
}