/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
            return new MaterializedJoinStrategy(new CrossProductStrategy());
        }
    }
}
