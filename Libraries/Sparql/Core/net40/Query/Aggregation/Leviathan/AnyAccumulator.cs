using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Aggregation.Leviathan
{
    public class AnyAccumulator
        : BaseShortCircuitExpressionAccumulator
    {
        public AnyAccumulator(IExpression arg)
            : base(arg, new BooleanNode(false))
        {
            this.ActualResult = this.AccumulatedResult;
        }

        public override bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is AnyAccumulator)) return false;

            AnyAccumulator any = (AnyAccumulator) other;
            return this.Expression.Equals(any.Expression);
        }

        protected internal override void Accumulate(IValuedNode value)
        {
            // If we see an invalid value or false can't short circuit yet
            if (value == null || value.NodeType != NodeType.Literal || !SparqlSpecsHelper.EffectiveBooleanValue(value)) return;

            // As soon as we've seen a true we can short circuit any further evaluation
            this.ShortCircuit = true;
            this.ShortCircuitResult = new BooleanNode(true);
        }
    }
}
