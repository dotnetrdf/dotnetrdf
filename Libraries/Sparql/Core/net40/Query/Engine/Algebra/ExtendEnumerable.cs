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
    public class ExtendEnumerable
        : WrapperEnumerable<ISolution>
    {
        public ExtendEnumerable(IEnumerable<ISolution> enumerable, IEnumerable<KeyValuePair<String, IExpression>> assignments, IExecutionContext context)
            : base(enumerable)
        {
            if (assignments == null) throw new ArgumentNullException("assignments");
            if (context == null) throw new ArgumentNullException("context");
            this.Assignments = assignments.ToList().AsReadOnly();
            this.Context = context;
        }

        private IList<KeyValuePair<String, IExpression>> Assignments { get; set; }

        private IExecutionContext Context { get; set; }

        public override IEnumerator<ISolution> GetEnumerator()
        {
            return new ExtendEnumerator(this.InnerEnumerable.GetEnumerator(), this.Assignments, this.Context);
        }
    }

    public class ExtendEnumerator
        : WrapperEnumerator<ISolution>
    {
        public ExtendEnumerator(IEnumerator<ISolution> enumerator, IEnumerable<KeyValuePair<String, IExpression>> assignments, IExecutionContext context)
            : base(enumerator)
        {
            if (assignments == null) throw new ArgumentNullException("assignments");
            if (context == null) throw new ArgumentNullException("context");
            this.Assignments = assignments.ToList().AsReadOnly();
            this.Context = context;
        }

        private IList<KeyValuePair<String, IExpression>> Assignments { get; set; }

        private IExecutionContext Context { get; set; }

        protected override bool TryMoveNext(out ISolution item)
        {
            item = null;
            if (!this.InnerEnumerator.MoveNext()) return false;

            // Evaluate the expressions and add the generated values to the solution
            item = new Solution(this.InnerEnumerator.Current);
            IExpressionContext context = this.Context.CreateExpressionContext();
            foreach (KeyValuePair<String, IExpression> kvp in this.Assignments)
            {
                try
                {
                    IValuedNode n = kvp.Value.Evaluate(item, context);
                    item.Add(kvp.Key, n);
                }
                catch (RdfQueryException)
                {
                    // If an expression errors it is equivalent to unbound
                    item.Add(kvp.Key, null);
                }
            }
            
            return true;
        }
    }
}