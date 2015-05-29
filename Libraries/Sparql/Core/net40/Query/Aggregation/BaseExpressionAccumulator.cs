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
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregation
{
    public abstract class BaseExpressionAccumulator
        : IAccumulator
    {
        protected BaseExpressionAccumulator(IExpression expr)
        {
            if (expr == null) throw new ArgumentNullException("expr");
            this.Expression = expr;
        }

        protected BaseExpressionAccumulator(IExpression expr, IValuedNode initialValue)
            : this(expr)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            this.AccumulatedResult = initialValue;
        }

        public IExpression Expression { get; private set; }

        public abstract bool Equals(IAccumulator other);

        public virtual void Accumulate(ISolution solution, IExpressionContext context)
        {
            try
            {
                Accumulate(this.Expression.Evaluate(solution, context));
            }
            catch (RdfQueryException)
            {
                Accumulate(null);
            }
        }

        /// <summary>
        /// Accumulates the actual value of evaluating the expression
        /// </summary>
        /// <param name="value">Value, will be null if the expression evaluated to an error</param>
        protected internal abstract void Accumulate(IValuedNode value);

        /// <summary>
        /// Gets/Sets the accumulated result
        /// </summary>
        public virtual IValuedNode AccumulatedResult { get; protected internal set; }
    }
}