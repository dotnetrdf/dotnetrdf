using System.Collections.Generic;
using VDS.RDF.Query.Engine.Joins.Workers;

namespace VDS.RDF.Query.Engine.Joins.Strategies
{
    public class FixedHashJoinStrategy
        : BaseVariableJoinStrategy
    {
        public FixedHashJoinStrategy(IEnumerable<string> joinVars)
            : base(joinVars) { }

        public override IJoinWorker PrepareWorker(IEnumerable<ISolution> rhs)
        {
            return new FixedHashJoinWorker(this.JoinVariables, rhs);
        }
    }
}
