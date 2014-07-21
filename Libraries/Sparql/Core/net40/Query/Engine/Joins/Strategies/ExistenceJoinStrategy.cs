using System.Collections.Generic;
using VDS.RDF.Query.Engine.Joins.Workers;

namespace VDS.RDF.Query.Engine.Joins.Strategies
{
    public class ExistenceJoinStrategy
        : WrapperJoinStrategy
    {
        public ExistenceJoinStrategy(IJoinStrategy strategy) 
            : base(strategy) {}

        public override IJoinWorker PrepareWorker(IEnumerable<ISolution> rhs)
        {
            return new ExistenceJoinWorker(base.PrepareWorker(rhs));
        }

        public override ISolution Join(ISolution lhs, ISolution rhs)
        {
            return lhs;
        }
    }

    public class NonExistenceJoinStrategy
    : WrapperJoinStrategy
    {
        public NonExistenceJoinStrategy(IJoinStrategy strategy)
            : base(strategy) { }

        public override IJoinWorker PrepareWorker(IEnumerable<ISolution> rhs)
        {
            return new NonExistenceJoinWorker(base.PrepareWorker(rhs));
        }

        public override ISolution Join(ISolution lhs, ISolution rhs)
        {
            return lhs;
        }
    }
}
