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

using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregation.Leviathan
{
    public class ModeAccumulator
        : BaseExpressionAccumulator
    {
        private readonly Dictionary<IValuedNode, long> _counts = new Dictionary<IValuedNode, long>();

        public ModeAccumulator(IExpression arg)
            : base(arg) { }

        public override bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is ModeAccumulator)) return false;

            ModeAccumulator mode = (ModeAccumulator) other;
            return this.Expression.Equals(mode.Expression);
        }

        protected internal override void Accumulate(IValuedNode value)
        {
            long currentValue = 0;
            if (this._counts.TryGetValue(value, out currentValue))
            {
                this._counts[value] = currentValue + 1;
            }
            else
            {
                this._counts.Add(value, 0);
            }
        }

        public override IValuedNode AccumulatedResult
        {
            get { return this._counts.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).FirstOrDefault(); }
            protected internal set { base.AccumulatedResult = value; }
        }
    }
}
