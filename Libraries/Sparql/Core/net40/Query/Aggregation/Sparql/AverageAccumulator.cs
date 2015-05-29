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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Operators.Numeric;

namespace VDS.RDF.Query.Aggregation.Sparql
{
    public class AverageAccumulator
        : BaseExpressionAccumulator
    {
        private readonly AdditionOperator _adder = new AdditionOperator();
        private readonly DivisionOperator _divisor = new DivisionOperator();
        private readonly IValuedNode[] _args = new IValuedNode[2];
        private IValuedNode _sum = new LongNode(0);
        private long _count = 0;

        public AverageAccumulator(IExpression expr)
            : base(expr)
        {
            this._args[0] = this._sum;
        }

        public override bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is AverageAccumulator)) return false;

            AverageAccumulator sum = (AverageAccumulator) other;
            return this.Expression.Equals(sum.Expression);
        }

        protected internal override void Accumulate(IValuedNode value)
        {
            if (value == null) return;

            // Check that we can add this to the previous argument
            this._args[1] = value;
            if (!this._adder.IsApplicable(this._args)) return;

            // If so go ahead and accumulate it
            // Put the total back into the first entry in our arguments array for next time
            this._sum = this._adder.Apply(this._args);
            this._args[0] = this._sum;
            this._count++;
        }

        public override IValuedNode AccumulatedResult
        {
            get { return this._divisor.Apply(this._sum, new LongNode(this._count)); }
            protected internal set { base.AccumulatedResult = value; }
        }
    }
}
