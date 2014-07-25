using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Query.Engine.Joins.Strategies;

namespace VDS.RDF.Query.Engine.Joins
{
    [TestFixture]
    public class HashJoinTests
        : AbstractJoinTests
    {
        protected override IEnumerable<ISolution> MakeJoinEnumerable(IEnumerable<ISolution> lhs, IEnumerable<string> lhsVars, IEnumerable<ISolution> rhs, IEnumerable<string> rhsVars)
        {
            IList<String> joinVars = lhsVars.Intersect(rhsVars).ToList();
            return new JoinEnumerable(lhs, rhs, joinVars.Count > 0 ? (IJoinStrategy) SelectHashJoinStrategy(lhs, rhs, joinVars) : new CrossProductStrategy(), new QueryExecutionContext());
        }

        protected IJoinStrategy SelectHashJoinStrategy(IEnumerable<ISolution> lhs, IEnumerable<ISolution> rhs, IList<String> joinVars)
        {
            if (joinVars.All(v => lhs.All(s => s[v] != null) && rhs.All(s => s[v] != null))) return new FixedHashJoinStrategy(joinVars);
            return new FloatingHashJoinStrategy(joinVars);
        }

        protected override IEnumerable<ISolution> MakeExpectedResults(IEnumerable<ISolution> lhs, IEnumerable<string> lhsVars, IEnumerable<ISolution> rhs, IEnumerable<string> rhsVars)
        {
            IList<String> joinVars = lhsVars.Intersect(rhsVars).ToList();
            foreach (ISolution x in lhs)
            {
                foreach (ISolution y in rhs)
                {
                    if (x.IsCompatibleWith(y, joinVars)) yield return x.Join(y);
                }
            }
        }
    }

    [TestFixture]
    public class MinusHashJoinTests
        : AbstractJoinTests
    {
        protected override IEnumerable<ISolution> MakeJoinEnumerable(IEnumerable<ISolution> lhs, IEnumerable<string> lhsVars, IEnumerable<ISolution> rhs, IEnumerable<string> rhsVars)
        {
            IList<String> joinVars = lhsVars.Intersect(rhsVars).ToList();
            return new JoinEnumerable(lhs, rhs, new NonExistenceJoinStrategy(joinVars.Count > 0 ? (IJoinStrategy)SelectHashJoinStrategy(lhs, rhs, joinVars) : new CrossProductStrategy()), new QueryExecutionContext());
        }

        protected IJoinStrategy SelectHashJoinStrategy(IEnumerable<ISolution> lhs, IEnumerable<ISolution> rhs, IList<String> joinVars)
        {
            if (joinVars.All(v => lhs.All(s => s[v] != null) && rhs.All(s => s[v] != null))) return new FixedHashJoinStrategy(joinVars);
            return new FloatingHashJoinStrategy(joinVars);
        }

        protected override IEnumerable<ISolution> MakeExpectedResults(IEnumerable<ISolution> lhs, IEnumerable<string> lhsVars, IEnumerable<ISolution> rhs, IEnumerable<string> rhsVars)
        {
            if (!rhs.Any()) return lhs;
            return MakeExpectedResultsEnumerable(lhs, lhsVars, rhs, rhsVars);
        }

        private IEnumerable<ISolution> MakeExpectedResultsEnumerable(IEnumerable<ISolution> lhs, IEnumerable<string> lhsVars, IEnumerable<ISolution> rhs, IEnumerable<string> rhsVars)
        {
            IList<String> joinVars = lhsVars.Intersect(rhsVars).ToList();
            foreach (ISolution x in lhs)
            {
                foreach (ISolution y in rhs)
                {
                    if (!x.IsCompatibleWith(y, joinVars)) yield return x;
                }
            }
        }
    }
}
