using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregation.Leviathan
{
    public class ModeAccumulator
        : BaseExpressionAccumulator
    {
        private readonly Dictionary<IValuedNode, long> _counts = new Dictionary<IValuedNode, long>();

        public ModeAccumulator(IExpression arg)
            : base(arg) { }

        public override bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is ModeAccumulator)) return false;

            ModeAccumulator mode = (ModeAccumulator) other;
            return this.Expression.Equals(mode.Expression);
        }

        protected internal override void Accumulate(IValuedNode value)
        {
            long currentValue = 0;
            if (this._counts.TryGetValue(value, out currentValue))
            {
                this._counts[value] = currentValue + 1;
            }
            else
            {
                this._counts.Add(value, 0);
            }
        }

        public override IValuedNode AccumulatedResult
        {
            get { return this._counts.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).FirstOrDefault(); }
            protected internal set { base.AccumulatedResult = value; }
        }
    }
}
