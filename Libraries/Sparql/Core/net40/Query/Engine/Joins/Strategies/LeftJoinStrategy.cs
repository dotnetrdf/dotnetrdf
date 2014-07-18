using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine.Joins.Workers;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Engine.Joins.Strategies
{
    /// <summary>
    /// A decorator which converts other join strategies into left joins
    /// </summary>
    public class LeftJoinStrategy
        : WrapperJoinStrategy
    {
        public LeftJoinStrategy(IJoinStrategy strategy, IEnumerable<IExpression> expressions)
            : base(strategy)
        {
            if (expressions == null) throw new ArgumentNullException("expressions");
            this.Expressions = expressions;
        }

        public IEnumerable<IExpression> Expressions { get; private set; }

        public override IJoinWorker PrepareWorker(IEnumerable<ISet> rhs)
        {
            return new LeftJoinWorker(base.PrepareWorker(rhs), this.Expressions);
        }
    }
}
