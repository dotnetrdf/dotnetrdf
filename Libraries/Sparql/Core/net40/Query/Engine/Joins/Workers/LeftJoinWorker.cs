using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Collections;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Engine.Joins.Workers
{
    public class LeftJoinWorker
        : WrapperJoinWorker
    {
        public LeftJoinWorker(IJoinWorker worker, IEnumerable<IExpression> expressions)
            : base(worker)
        {
            if (expressions == null) throw new ArgumentNullException();
            this.Expressions = expressions.ToList().AsReadOnly();
        }

        public IList<IExpression> Expressions { get; private set; }

        public override IEnumerable<ISet> Find(ISet lhs, IExecutionContext context)
        {
            IEnumerable<ISet> rhs = base.Find(lhs, context);
            // TODO Do we need a specific enumerable to handle left join filtering?
            if (this.Expressions.Count > 0) rhs = new FilterEnumerable(rhs.Select(s => lhs.Join(s)), this.Expressions, context);
            return rhs.AddIfEmpty(new Set());
        }
    }
}
