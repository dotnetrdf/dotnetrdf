using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine.Joins.Workers;

namespace VDS.RDF.Query.Engine.Joins.Strategies
{
    public class LoopJoinStrategy
        : BaseVariableJoinStrategy
    {
        public LoopJoinStrategy(IEnumerable<String> joinVars) 
            : base(joinVars) { }

        public override IJoinWorker PrepareWorker(IEnumerable<ISolution> rhs)
        {
            return new LoopJoinWorker(this.JoinVariables, rhs);
        }
    }
}
