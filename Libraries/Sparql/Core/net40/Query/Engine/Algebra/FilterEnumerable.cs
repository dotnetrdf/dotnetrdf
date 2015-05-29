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
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Collections;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Engine.Algebra
{
    public class FilterEnumerable
        : WrapperEnumerable<ISolution>
    {
        public FilterEnumerable(IEnumerable<ISolution> enumerable, IEnumerable<IExpression> expressions, IExecutionContext context)
            : base(enumerable)
        {
            if (expressions == null) throw new ArgumentNullException("expressions");
            if (context == null) throw new ArgumentNullException("context");
            this.Expressions = expressions.ToList().AsReadOnly();
            this.Context = context;
        }

        private IList<IExpression> Expressions { get; set; }

        private IExecutionContext Context { get; set; }

        public override IEnumerator<ISolution> GetEnumerator()
        {
            return new FilterEnumerator(this.InnerEnumerable.GetEnumerator(), this.Expressions, this.Context);
        }
    }

    public class FilterEnumerator
        : WrapperEnumerator<ISolution>
    {
        public FilterEnumerator(IEnumerator<ISolution> enumerator, IEnumerable<IExpression> expressions, IExecutionContext context)
            : base(enumerator)
        {
            if (expressions == null) throw new ArgumentNullException("expressions");
            if (context == null) throw new ArgumentNullException("context");
            this.Expressions = expressions.ToList().AsReadOnly();
            this.Context = context;
        }

        private IList<IExpression> Expressions { get; set; }

        private IExecutionContext Context { get; set; }

        protected override bool TryMoveNext(out ISolution item)
        {
            item = null;
            while (this.InnerEnumerator.MoveNext())
            {
                item = this.InnerEnumerator.Current;

                try
                {
                    // Check that the solution under consideration results in true for every expression
                    bool accept = true;
                    IExpressionContext context = this.Context.CreateExpressionContext();
                    foreach (IExpression expr in this.Expressions)
                    {
                        IValuedNode n = expr.Evaluate(item, context);

                        if (n.AsSafeBoolean()) continue;

                        // If evaluates to false then discard this solution
                        accept = false;
                        break;
                    }

                    // If this is set then one of the expressions evaluated to false so we discard the solution
                    if (!accept) continue;

                    // Otherwise everything evaluated to true so accept this solution
                    return true;
                }
                catch (RdfQueryException)
                {
                    // If an expression errors it is equivalent to false and we discard the solution under consideration
                }
            }
            return false;
        }
    }
}
