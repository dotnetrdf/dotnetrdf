using System;
using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Sorting
{
    public class SortCondition
        : ISortCondition
    {
        public SortCondition(IExpression expression, bool isAscending)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            this.Expression = expression;
            this.IsAscending = isAscending;
        }

        public SortCondition(IExpression expression)
            : this(expression, true) {}

        public bool IsAscending { get; private set; }

        public IExpression Expression { get; private set; }

        public bool Equals(ISortCondition other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return this.IsAscending == other.IsAscending && this.Expression.Equals(other.Expression);
        }

        public IComparer<ISolution> CreateComparer(IExpressionContext context)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        public string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            return this.IsAscending ? this.Expression.ToString(formatter) : String.Format("DESC({0})", this.Expression.ToString(formatter));
        }

        public string ToPrefixString()
        {
            return ToPrefixString(new AlgebraFormatter());
        }

        public string ToPrefixString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            return this.IsAscending ? this.Expression.ToPrefixString(formatter) : String.Format("(desc {0})", this.Expression.ToPrefixString(formatter));
        }
    }
}