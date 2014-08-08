using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Specifications;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions.Aggregates
{
    public class CountAllAggregate
        : BaseAggregate
    {
        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
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
            if (formatter == null) throw new ArgumentNullException("formatter");
            return String.Format("{0}(*)", this.Functor);
        }

        public override string ToPrefixString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            return this.Functor.ToLowerInvariant();
        }

        public override IAccumulator CreateAccumulator()
        {
            return new CountAllAccumulator();
        }

        public override IEnumerable<IExpression> Arguments
        {
            get { return Enumerable.Empty<IExpression>(); }
        }

        public override IExpression Copy()
        {
            return new CountAllAggregate();
        }

        public override IExpression Copy(IEnumerable<IExpression> arguments)
        {
            return Copy();
        }

        public override bool Equals(object other)
        {
            if (other is CountAllAggregate) return Equals((IExpression) other);
            return false;
        }

        public override int GetHashCode()
        {
            return this.Functor.GetHashCode();
        }
    }
}
