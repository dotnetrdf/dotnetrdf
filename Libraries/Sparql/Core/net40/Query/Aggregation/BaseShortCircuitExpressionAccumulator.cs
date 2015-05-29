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

using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregation
{
    /// <summary>
    /// An abstract expression based accumulator that supports short circuiting
    /// </summary>
    public abstract class BaseShortCircuitExpressionAccumulator 
        : BaseExpressionAccumulator {

        protected BaseShortCircuitExpressionAccumulator(IExpression expr, IValuedNode initialValue) 
            : base(expr, initialValue) {}

        /// <summary>
        /// Gets/Sets the short circuited result assuming evaluation was short circuited
        /// </summary>
        protected virtual IValuedNode ShortCircuitResult { get; set; }

        /// <summary>
        /// Gets/Sets whether to short circuit evaluation
        /// </summary>
        /// <remarks>
        /// This can be used by derived implementations to indicate that they have already determined the result and accumulating further solutions will not change that result.  This allows evaluation to be sped up considerably in some cases
        /// </remarks>
        protected internal bool ShortCircuit { get; set; }

        /// <summary>
        /// Gets/Sets the actual accumulated result assuming evaluation was not short circuited
        /// </summary>
        protected virtual IValuedNode ActualResult { get; set; }

        public sealed override IValuedNode AccumulatedResult
        {
            get
            {
                return this.ShortCircuit ? this.ShortCircuitResult : this.ActualResult;
            }
            protected internal set { base.AccumulatedResult = value; }
        }

        public sealed override void Accumulate(ISolution solution, IExpressionContext context)
        {
            // Can skip evaluation if short circuiting as the final result is already known
            if (this.ShortCircuit) return;

            // Otherwise evaluate expression and try and accumulate
            base.Accumulate(solution, context);
        }
    }
}