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
using VDS.RDF.Collections;

namespace VDS.RDF.Query.Engine.Joins
{
    public class JoinEnumerable
        : WrapperEnumerable<ISolution>
    {
        public JoinEnumerable(IEnumerable<ISolution> lhs, IEnumerable<ISolution> rhs, IJoinStrategy strategy, IExecutionContext context)
            : base(lhs)
        {
            if (rhs == null) throw new ArgumentNullException("rhs");
            if (strategy == null) throw new ArgumentNullException("strategy");
            if (context == null) throw new ArgumentNullException("context");
            this.Rhs = rhs;
            this.Strategy = strategy;
            this.Context = context;
        }

        public IEnumerable<ISolution> Lhs { get { return this.InnerEnumerable; } }

        public IEnumerable<ISolution> Rhs { get; private set; }

        public IJoinStrategy Strategy { get; private set; }

        public IExecutionContext Context { get; private set; }

        public override IEnumerator<ISolution> GetEnumerator()
        {
            return new JoinEnumerator(this.Lhs.GetEnumerator(), this.Rhs, this.Strategy, this.Context);
        }
    }

    public class JoinEnumerator
        : WrapperEnumerator<ISolution>
    {
        public JoinEnumerator(IEnumerator<ISolution> lhs, IEnumerable<ISolution> rhs, IJoinStrategy strategy, IExecutionContext context)
            : base(lhs)
        {
            if (rhs == null) throw new ArgumentNullException("rhs");
            if (strategy == null) throw new ArgumentNullException("strategy");
            if (context == null) throw new ArgumentNullException("context");
            this.Rhs = rhs;
            this.Strategy = strategy;
            this.Context = context;
        }

        public IEnumerable<ISolution> Rhs { get; private set; }

        private IEnumerator<ISolution> RhsEnumerator { get; set; } 

        public IJoinStrategy Strategy { get; private set; }

        public IExecutionContext Context { get; private set; }

        private IJoinWorker Worker { get; set; }

        protected override bool TryMoveNext(out ISolution item)
        {
            item = null;

            // If we currently have a RHS enumerator see if there are further RHS sets present
            if (this.RhsEnumerator != null)
            {
                if (this.RhsEnumerator.MoveNext())
                {
                    item = this.Strategy.Join(this.InnerEnumerator.Current, this.RhsEnumerator.Current);
                    return true;
                }
                // No further RHS sets
                this.RhsEnumerator = null;
            }

            while (true)
            {
                // Can we move to the next LHS set?
                if (!this.InnerEnumerator.MoveNext()) return false;

                // Prepare a new join worker if necessary
                if (this.Worker == null || !this.Worker.CanReuse(this.InnerEnumerator.Current, this.Context))
                {
                    this.Worker = this.Strategy.PrepareWorker(this.Rhs);
                    this.RhsEnumerator = null;
                }
                // Crete a new RHS enumerator if necessary
                if (this.RhsEnumerator == null)
                {
                    this.RhsEnumerator = this.Worker.Find(this.InnerEnumerator.Current, this.Context).GetEnumerator();
                }

                // Is there a further RHS set available?
                if (this.RhsEnumerator.MoveNext())
                {
                    item = this.Strategy.Join(this.InnerEnumerator.Current, this.RhsEnumerator.Current);
                    return true;
                }
                this.RhsEnumerator = null;
            }
        }

        protected override void ResetInternal()
        {
            this.Worker = null;
        }
    }
}
