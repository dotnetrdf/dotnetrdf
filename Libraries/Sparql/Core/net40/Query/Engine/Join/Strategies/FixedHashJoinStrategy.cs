using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine.Join.Workers;

namespace VDS.RDF.Query.Engine.Join.Strategies
{
    public class FixedHashJoinStrategy
        : BaseVariableJoinStrategy
    {
        public FixedHashJoinStrategy(IEnumerable<string> joinVars)
            : base(joinVars) { }

        public override IJoinWorker PrepareWorker(IEnumerable<ISet> rhs)
        {
            return new FixedHashJoinWorker(this.JoinVariables, rhs);
        }
    }
}
