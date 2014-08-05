using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Abstract base class for nullary expressions
    /// </summary>
    public abstract class BaseNullaryExpression
        : INullaryExpression
    {
        public abstract bool Equals(IExpression other);

        public abstract IValuedNode Evaluate(ISolution set, IExpressionContext context);

        public abstract IEnumerable<string> Variables { get; }

        public abstract string Functor { get; }

        public abstract bool CanParallelise { get; }

        public abstract bool IsDeterministic { get; }

        public abstract bool IsConstant { get; }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        public abstract string ToString(IAlgebraFormatter formatter);

        public string ToPrefixString()
        {
            return ToPrefixString(new AlgebraFormatter());
        }

        public abstract string ToPrefixString(IAlgebraFormatter formatter);

        public abstract IExpression Copy();
    }
}
