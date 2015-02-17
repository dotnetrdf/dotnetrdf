using VDS.Common.Trees;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Sorting;

namespace VDS.RDF.Query.Aggregation.Leviathan
{
    public class MedianAccumulator
        : BaseExpressionAccumulator
    {
        private readonly AVLTree<IValuedNode, byte> _values = new AVLTree<IValuedNode, byte>(new SparqlOrderingComparer());

        public MedianAccumulator(IExpression arg) 
            : base(arg) {}

        public override bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is MedianAccumulator)) return false;

            MedianAccumulator median = (MedianAccumulator) other;
            return this.Expression.Equals(median.Expression);
        }

        protected internal override void Accumulate(IValuedNode value)
        {
            this._values.Add(value, 0);
        }

        public override IValuedNode AccumulatedResult
        {
            get
            {
                if (this._values.Root == null) return null;
                return this._values.Root.Key;
            }
            protected internal set { base.AccumulatedResult = value; }
        }
    }
}
