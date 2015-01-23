using System;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Aggregation
{
    public class GroupConcatAccumulator
        : BaseExpressionAccumulator
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private bool _error = false;
        private String _lang = null;
        private bool _allString = true, _allSameLang = true;
        private long _count = 0;

        public GroupConcatAccumulator(IExpression expr)
            : this(expr, new ConstantTerm(new StringNode(" "))) {}

        public GroupConcatAccumulator(IExpression expr, IExpression separatorExpr)
            : base(expr, new StringNode(""))
        {
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
            if (!(other is GroupConcatAccumulator)) return false;

            GroupConcatAccumulator groupConcat = (GroupConcatAccumulator) other;
            return this.Expression.Equals(groupConcat.Expression) && this.Separator.Equals(groupConcat.Separator);
        }

        public override void Accumulate(ISolution solution, IExpressionContext context)
        {
            // Can skip evaluation if already seen an error as the end result will be an error
            if (this._error) return;

            // Otherwise evalue expression and try and accumulate
            base.Accumulate(solution, context);
        }

        protected internal override void Accumulate(IValuedNode value)
        {
            this._count++;
            if (value == null || value.NodeType != NodeType.Literal)
            {
                this._error = true;
                return;
            }

            try
            {
                if (this._builder.Length > 0) this._builder.Append(this.Separator);
                this._builder.Append(value.AsString());

                if (value.HasLanguage)
                {
                    if (!this._allSameLang) return;

                    if (this._lang == null)
                    {
                        // First thing seen with a language tag so set initial language tag
                        this._lang = value.Language;
                    }
                    else
                    {
                        // Check if matches language already seen
                        if (!this._lang.Equals(value.Language))
                        {
                            this._allSameLang = false;
                        }
                    }
                }
                else if (value.HasDataType)
                {
                    if (!this._allString) return;

                    if (!value.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                    {
                        this._allString = false;
                    }
                }

                // Neither a language tag or data type so won't preserve either on output and setting these to false saves us doing some checks later
                this._allSameLang = false;
                this._allString = false;
            }
            catch
            {
                this._error = true;
            }
        }

        public override IValuedNode AccumulatedResult
        {
            get
            {
                if (this._error) return null;
                if (this._count == 0) return base.AccumulatedResult;
                if (this._allSameLang && this._lang != null)
                {
                    return new StringNode(this._builder.ToString(), this._lang);
                }
                return this._allString ? new StringNode(this._builder.ToString(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString)) : new StringNode(this._builder.ToString());
            }
            protected internal set { throw new NotSupportedException("Group concatentations final value is calculated on the fly from the accumulated strings"); }
        }
    }
}