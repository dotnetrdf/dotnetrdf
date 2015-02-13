using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Aggregation.Leviathan
{
    public class NoneAccumulator
        : BaseShortCircuitExpressionAccumulator
    {
        public NoneAccumulator(IExpression arg)
            : base(arg, new BooleanNode(true))
        {
            this.ActualResult = this.AccumulatedResult;
        }

        public override bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is NoneAccumulator)) return false;

            NoneAccumulator none = (NoneAccumulator) other;
            return this.Expression.Equals(none.Expression);
        }

        protected internal override void Accumulate(IValuedNode value)
        {
            // If we see an invalid value or a false can't short circuit yet
            if (value == null || value.NodeType != NodeType.Literal || !SparqlSpecsHelper.EffectiveBooleanValue(value)) return;

            // As soon as we've seen a true we can short circuit any further evaluation
            this.ShortCircuit = true;
            this.ShortCircuitResult = new BooleanNode(false);
        }
    }
}
