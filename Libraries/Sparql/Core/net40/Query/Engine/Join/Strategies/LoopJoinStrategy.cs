using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Engine.Join.Workers;

namespace VDS.RDF.Query.Engine.Join.Strategies
{
    public class LoopJoinStrategy
        : BaseVariableJoinStrategy
    {
        public LoopJoinStrategy(IEnumerable<String> joinVars) 
            : base(joinVars) { }

        public override IJoinWorker PrepareWorker(IEnumerable<ISet> rhs)
        {
            return new LoopJoinWorker(this.JoinVariables, rhs);
        }
    }
}
