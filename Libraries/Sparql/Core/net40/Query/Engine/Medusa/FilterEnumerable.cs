using System;
using System.Collections.Generic;
using VDS.RDF.Collections;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Engine.Medusa
{
    public class FilterEnumerable
        : WrapperEnumerable<ISet>
    {
        public FilterEnumerable(IEnumerable<ISet> enumerable, IEnumerable<IExpression> expressions, IExecutionContext context)
            : base(enumerable)
        {
            if (expressions == null) throw new ArgumentNullException("expressions");
            if (context == null) throw new ArgumentNullException("context");
            this.Expressions = new List<IExpression>(expressions);
            this.Context = context;
        }

        private IList<IExpression> Expressions { get; set; }

        private IExecutionContext Context { get; set; }

        public override IEnumerator<ISet> GetEnumerator()
        {
            return new FilterEnumerator(this.InnerEnumerable.GetEnumerator(), this.Expressions, this.Context);
        }
    }

    public class FilterEnumerator
        : WrapperEnumerator<ISet>
    {
        public FilterEnumerator(IEnumerator<ISet> enumerator, IEnumerable<IExpression> expressions, IExecutionContext context)
            : base(enumerator)
        {
            if (expressions == null) throw new ArgumentNullException("expressions");
            if (context == null) throw new ArgumentNullException("context");
            this.Expressions = new List<IExpression>(expressions);
            this.Context = context;
        }

        private IList<IExpression> Expressions { get; set; }

        private IExecutionContext Context { get; set; }

        protected override bool TryMoveNext(out ISet item)
        {
            item = null;
            while (this.InnerEnumerator.MoveNext())
            {
                item = this.InnerEnumerator.Current;

                try
                {
                    // Check that the set under consideration results in true for every expression
                    bool accept = true;
                    foreach (IExpression expr in this.Expressions)
                    {
                        IValuedNode n = expr.Evaluate(item, this.Context.CreateExpressionContext());

                        if (n.AsSafeBoolean()) continue;

                        // If evaluates to false then discard this set
                        accept = false;
                        break;
                    }

                    // If this is set then one of the expressions evaluated to false so we discard the set
                    if (!accept) continue;

                    // Otherwise everything evaluated to true so accept this set
                    return true;
                }
                catch (RdfQueryException)
                {
                    // If an expression errors it is equivalent to false and we discard the set under consideration
                }
            }
            return false;
        }
    }
}
