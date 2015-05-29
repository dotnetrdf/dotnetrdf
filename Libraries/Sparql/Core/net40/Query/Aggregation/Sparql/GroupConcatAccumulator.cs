/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Aggregation.Sparql
{
    public class GroupConcatAccumulator
        : BaseShortCircuitExpressionAccumulator
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private String _lang = null;
        private bool _allString = true, _allSameLang = true;
        private long _count = 0;

        public GroupConcatAccumulator(IExpression expr)
            : this(expr, new ConstantTerm(new StringNode(" "))) {}

        public GroupConcatAccumulator(IExpression expr, IExpression separatorExpr)
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
            if (!(other is GroupConcatAccumulator)) return false;

            GroupConcatAccumulator groupConcat = (GroupConcatAccumulator) other;
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
                this.ShortCircuit = true;
            }
        }

        protected override IValuedNode ActualResult
        {
            get
            {
                if (this._count == 0) return base.AccumulatedResult;
                if (this._allSameLang && this._lang != null)
                {
                    return new StringNode(this._builder.ToString(), this._lang);
                }
                return this._allString ? new StringNode(this._builder.ToString(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString)) : new StringNode(this._builder.ToString());
            }
            set
            {
                throw new NotSupportedException("Group concatentations final value is calculated on the fly from the accumulated strings");
            }
        }
    }
}