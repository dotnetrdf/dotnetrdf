using System;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Aggregation.XPath
{
    public class StringJoinAccumulator
        : BaseShortCircuitExpressionAccumulator
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private long _count = 0;

        public StringJoinAccumulator(IExpression expr)
            : this(expr, new ConstantTerm(new StringNode(" "))) {}

        public StringJoinAccumulator(IExpression expr, IExpression separatorExpr)
            : base(expr, new StringNode(""))
        {
            ShortCircuit = false;
            try
            {
                IValuedNode sep = separatorExpr.Evaluate(new Solution(), null);
                this.Separator = sep.AsString();
            }
            catch
            {
                this.Separator = " ";
            }
        }

        public String Separator { get; private set; }

        public override bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is StringJoinAccumulator)) return false;

            StringJoinAccumulator groupConcat = (StringJoinAccumulator) other;
            return this.Expression.Equals(groupConcat.Expression) && this.Separator.Equals(groupConcat.Separator);
        }

        protected internal override void Accumulate(IValuedNode value)
        {
            this._count++;
            if (value == null || value.NodeType != NodeType.Literal)
            {
                this.ShortCircuit = true;
                return;
            }

            try
            {
                if (this._builder.Length > 0) this._builder.Append(this.Separator);
                this._builder.Append(value.AsString());
            }
            catch
            {
                this.ShortCircuit = true;
            }
        }

        protected override IValuedNode ActualResult
        {
            get
            {
                return this._count == 0 ? base.AccumulatedResult : new StringNode(this._builder.ToString(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            }
            set
            {
                throw new NotSupportedException("String Joins final value is calculated on the fly from the accumulated strings");
            }
        }
    }
}