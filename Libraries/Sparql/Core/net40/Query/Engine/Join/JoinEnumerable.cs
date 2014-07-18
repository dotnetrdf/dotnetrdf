using System;
using System.Collections.Generic;
using VDS.Common.Tries;
using VDS.RDF.Collections;

namespace VDS.RDF.Query.Engine.Join
{
    public class JoinEnumerable
        : WrapperEnumerable<ISet>
    {
        public JoinEnumerable(IEnumerable<ISet> lhs, IEnumerable<ISet> rhs, IJoinStrategy strategy, IExecutionContext context)
            : base(lhs)
        {
            if (rhs == null) throw new ArgumentNullException("rhs");
            if (strategy == null) throw new ArgumentNullException("strategy");
            if (context == null) throw new ArgumentNullException("context");
            this.Rhs = rhs;
            this.Strategy = strategy;
            this.Context = context;
        }

        public IEnumerable<ISet> Lhs { get { return this.InnerEnumerable; } }

        public IEnumerable<ISet> Rhs { get; private set; }

        public IJoinStrategy Strategy { get; private set; }

        public IExecutionContext Context { get; private set; }

        public override IEnumerator<ISet> GetEnumerator()
        {
            return new JoinEnumerator(this.Lhs.GetEnumerator(), this.Rhs, this.Strategy, this.Context);
        }
    }

    public class JoinEnumerator
        : WrapperEnumerator<ISet>
    {
        public JoinEnumerator(IEnumerator<ISet> lhs, IEnumerable<ISet> rhs, IJoinStrategy strategy, IExecutionContext context)
            : base(lhs)
        {
            if (rhs == null) throw new ArgumentNullException("rhs");
            if (strategy == null) throw new ArgumentNullException("strategy");
            if (context == null) throw new ArgumentNullException("context");
            this.Rhs = rhs;
            this.Strategy = strategy;
            this.Context = context;
        }

        public IEnumerable<ISet> Rhs { get; private set; }

        private IEnumerator<ISet> RhsEnumerator { get; set; } 

        public IJoinStrategy Strategy { get; private set; }

        public IExecutionContext Context { get; private set; }

        private IJoinWorker Worker { get; set; }

        protected override bool TryMoveNext(out ISet item)
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
