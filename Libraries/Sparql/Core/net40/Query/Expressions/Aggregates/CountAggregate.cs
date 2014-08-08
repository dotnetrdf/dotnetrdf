using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Specifications;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions.Aggregates
{
    public class CountAggregate
        : BaseAggregate
    {
        private readonly IExpression _argument;

        public CountAggregate(IExpression arg)
        {
            if (arg == null) throw new ArgumentNullException("arg");
            this._argument = arg;
        }

        public override IEnumerable<string> Variables
        {
            get { return this._argument.Variables; }
        }

        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordCount; }
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            StringBuilder builder = new StringBuilder();
            builder.Append(this.Functor);
            builder.Append('(');
            builder.Append(this._argument.ToString(formatter));
            builder.Append(')');
            return builder.ToString();
        }

        public override string ToPrefixString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            StringBuilder builder = new StringBuilder();
            builder.Append(this.Functor.ToLowerInvariant());
            builder.Append(' ');
            builder.Append(this._argument.ToPrefixString(formatter));
            return builder.ToString();
        }

        public override IAccumulator CreateAccumulator()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IExpression> Arguments
        {
            get { return this._argument.AsEnumerable(); }
        }

        public override IExpression Copy(IEnumerable<IExpression> arguments)
        {
            return new CountAggregate(arguments.FirstOrDefault());
        }

        public override bool Equals(object other)
        {
            if (other is CountAggregate) return Equals((IExpression) other);
            return false;
        }

        public override int GetHashCode()
        {
            return Tools.CombineHashCodes(this.Functor.GetHashCode(), this._argument.GetHashCode());
        }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is CountAggregate)) return false;

            CountAggregate agg = (CountAggregate)other;
            return this._argument.Equals(agg.Arguments.First());
        }
    }
}
