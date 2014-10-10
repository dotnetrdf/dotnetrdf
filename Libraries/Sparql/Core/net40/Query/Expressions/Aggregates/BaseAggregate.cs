using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Grouping;

namespace VDS.RDF.Query.Expressions.Aggregates
{
    public abstract class BaseAggregate 
        : BaseNAryExpression, IAggregateExpression
    {
        protected BaseAggregate()
            : base(Enumerable.Empty<IExpression>()) {}

        protected BaseAggregate(IEnumerable<IExpression> args) 
            : base(args) {}

        public override IValuedNode Evaluate(ISolution set, IExpressionContext context)
        {
            throw new RdfQueryException("Aggregates cannot be invoked on a single solution");
        }

        public override bool CanParallelise
        {
            get { return false; }
        }

        public override bool IsDeterministic
        {
            get { return true; }
        }

        public override bool IsConstant
        {
            get { return false; }
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit((IAggregateExpression)this);
        }

        public abstract IAccumulator CreateAccumulator();
    }
}