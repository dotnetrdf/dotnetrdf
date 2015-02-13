using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Aggregation.Leviathan
{
    public class AllAccumulator
        : BaseShortCircuitExpressionAccumulator
    {
        public AllAccumulator(IExpression arg)
            : base(arg, new BooleanNode(true))
        {
            this.ActualResult = this.AccumulatedResult;
        }

        public override bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is AllAccumulator)) return false;

            AllAccumulator all = (AllAccumulator) other;
            return this.Expression.Equals(all.Expression);
        }

        protected internal override void Accumulate(IValuedNode value)
        {
            // If we see a true can't short circuit yet
            if (value != null && value.NodeType == NodeType.Literal && SparqlSpecsHelper.EffectiveBooleanValue(value)) return;

            // As soon as we've seen an invalid value or a false
            this.ShortCircuit = true;
            this.ShortCircuitResult = new BooleanNode(false);
        }
    }
}
