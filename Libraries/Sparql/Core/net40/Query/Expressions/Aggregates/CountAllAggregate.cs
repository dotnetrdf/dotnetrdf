using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Specifications;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions.Aggregates
{
    public class CountAllAggregate
        : BaseAggregate, INullaryExpression
    {
        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit((IAggregateExpression)this);
        }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return false;
            if (other == null) return false;
            return other is CountAllAggregate;
        }

        public override IEnumerable<string> Variables
        {
            get { return Enumerable.Empty<String>(); }
        }

        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordCount; }
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            throw new NotImplementedException();
        }

        public override string ToPrefixString(IAlgebraFormatter formatter)
        {
            throw new NotImplementedException();
        }

        public IExpression Copy()
        {
            throw new NotImplementedException();
        }

        public override IAccumulator CreateAccumulator()
        {
            return new CountAllAccumulator();
        }
    }
}
