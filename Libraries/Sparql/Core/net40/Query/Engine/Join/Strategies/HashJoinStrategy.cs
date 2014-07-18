using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Engine.Join.Strategies
{
    public class HashJoinStrategy
        : BaseVariableJoinStrategy
    {
        public HashJoinStrategy(IEnumerable<string> joinVars)
            : base(joinVars) { }

        public override IJoinWorker PrepareWorker(IEnumerable<ISet> rhs)
        {
            throw new NotImplementedException();
        }
    }
}
