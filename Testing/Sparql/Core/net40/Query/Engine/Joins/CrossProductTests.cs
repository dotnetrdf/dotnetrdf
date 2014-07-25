using System.Collections.Generic;
using NUnit.Framework;
using VDS.RDF.Query.Engine.Joins.Strategies;

namespace VDS.RDF.Query.Engine.Joins
{
    [TestFixture]
    public class CrossProductTests 
        : AbstractJoinTests
    {
        protected override IEnumerable<ISolution> MakeJoinEnumerable(IEnumerable<ISolution> lhs, IEnumerable<string> lhsVars, IEnumerable<ISolution> rhs, IEnumerable<string> rhsVars)
        {
            return new JoinEnumerable(lhs, rhs, new CrossProductStrategy(), new QueryExecutionContext());
        }

        protected override IEnumerable<ISolution> MakeExpectedResults(IEnumerable<ISolution> lhs, IEnumerable<string> lhsVars, IEnumerable<ISolution> rhs, IEnumerable<string> rhsVars)
        {
            foreach (ISolution x in lhs)
            {
                foreach (ISolution y in rhs)
                {
                    yield return x.Join(y);
                }
            }
        }
    }
}