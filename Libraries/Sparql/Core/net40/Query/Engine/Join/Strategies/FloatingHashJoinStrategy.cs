using System.Collections.Generic;
using VDS.RDF.Query.Engine.Join.Workers;

namespace VDS.RDF.Query.Engine.Join.Strategies
{
    public class FloatingHashJoinStrategy
        : BaseVariableJoinStrategy
    {
        public FloatingHashJoinStrategy(IEnumerable<string> joinVars)
            : base(joinVars) { }

        public override IJoinWorker PrepareWorker(IEnumerable<ISet> rhs)
        {
            return new FloatingHashJoinWorker(this.JoinVariables, rhs);
        }
    }
}
