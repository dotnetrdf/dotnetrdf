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
            foreach (KeyValuePair<String, IExpression> kvp in this.Assignments)
            {
                try
                {
                    IValuedNode n = kvp.Value.Evaluate(item, this.Context.CreateExpressionContext());
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