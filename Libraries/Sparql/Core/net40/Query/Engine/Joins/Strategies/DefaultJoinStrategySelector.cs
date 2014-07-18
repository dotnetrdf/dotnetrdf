using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Engine.Joins.Strategies
{
    /// <summary>
    /// Default implementation of a join strategy selector
    /// </summary>
    public class DefaultJoinStrategySelector
        : IJoinStrategySelector
    {
        public virtual IJoinStrategy Select(IAlgebra lhs, IAlgebra rhs)
        {
            List<String> lhsVars = lhs.ProjectedVariables.ToList();
            List<String> rhsVars = rhs.ProjectedVariables.ToList();

            List<String> joinVars = lhsVars.Intersect(rhsVars).ToList();

            // If no common variables use a product strategy
            if (joinVars.Count == 0) return CreateProduct();

            // TODO If we want to be memory efficient we should use a LoopJoin to avoid materializing the RHS

            // Some common variables so want to use a Hash Join
            // Need to decide which kind of hash join is appropriate
            HashSet<String> lhsFixed = new HashSet<string>(lhs.FixedVariables);
            HashSet<String> rhsFixed = new HashSet<string>(rhs.FixedVariables);

            // We can use a fixed hash join if all join variables are guaranteed to be fixed on both sides
            if (joinVars.All(v => lhsFixed.Contains(v) && rhsFixed.Contains(v))) return CreateFixedHash(joinVars);

            // Otherwise use a floating hash
            return CreateFloatingHash(joinVars);
        }

        protected virtual IJoinStrategy CreateFloatingHash(IEnumerable<string> joinVars)
        {
            return new FloatingHashJoinStrategy(joinVars);
        }

        protected virtual IJoinStrategy CreateFixedHash(IEnumerable<string> joinVars)
        {
            return new FixedHashJoinStrategy(joinVars);
        }

        protected virtual IJoinStrategy CreateProduct()
        {
            return new MaterializedJoinStrategy(new ProductJoinStrategy());
        }
    }
}
