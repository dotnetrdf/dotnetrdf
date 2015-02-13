using System.Collections.Generic;
using System.Text;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Aggregation.Sparql;
using VDS.RDF.Specifications;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions.Aggregates.Sparql
{
    public class CountAllDistinctAggregate
        : BaseAggregate
    {
        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordCount; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return new CountAllDistinctAccumulator();
        }

        public override IExpression Copy()
        {
            return new CountAllDistinctAggregate();
        }

        public override IExpression Copy(IEnumerable<IExpression> arguments)
        {
            return Copy();
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.Functor.ToLowerInvariant());
            builder.Append("(DISTINCT *)");
            return builder.ToString();
        }

        public override string ToPrefixString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('(');
            builder.Append(this.Functor.ToLowerInvariant());
            builder.Append("(distinct *)");
            return builder.ToString();
        }
    }
}
