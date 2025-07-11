/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System.Collections.Generic;
using System.Linq;
using VDS.Common.Collections;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Abstract Base Class for representing Multisets.
/// </summary>
public abstract class BaseMultiset
{
    /// <summary>
    /// List of IDs that is used to return the Sets in order if the Multiset has been sorted.
    /// </summary>
    protected List<int> _orderedIDs = null;


    private int _virtualCount = -1;
    /// <summary>
    /// The number of results that would be returned without any limit clause to a query or -1 if not supported. Defaults to the same value as the Count member.
    /// </summary>
    public int VirtualCount
    {
        get => (_virtualCount==-1 ? Count : _virtualCount);
        internal set => _virtualCount = value;
    }

    /// <summary>
    /// Get whether this multiset will use LINQ parallel operators for evaluating set operations.
    /// </summary>
    public bool UsePLinqEvaluation { get; } = true;

    /// <summary>
    /// Create a new multiset.
    /// </summary>
    /// <param name="usePLinqEvaluation">If true, then LINQ parallel operators will be used when evaluating set operations using this multiset.</param>
    public BaseMultiset(bool usePLinqEvaluation = true)
    {
        UsePLinqEvaluation = usePLinqEvaluation;
    }

    /// <summary>
    /// Joins this Multiset to another Multiset.
    /// </summary>
    /// <param name="other">Other Multiset.</param>
    /// <returns></returns>
    public virtual BaseMultiset Join(BaseMultiset other)
    {
        // If the Other is the Identity Multiset the result is this Multiset
        if (other is IdentityMultiset) return this;
        // If the Other is the Null Multiset the result is the Null Multiset
        if (other is NullMultiset) return other;
        // If the Other is Empty then the result is the Null Multiset
        if (other.IsEmpty) return new NullMultiset();

        // Find the First Variable from this Multiset which is in both Multisets
        // If there is no Variable from this Multiset in the other Multiset then this
        // should be a Product operation instead of a Join
        var joinVars = Variables.Where(v => other.Variables.Contains(v)).ToList();
        if (joinVars.Count == 0) return Product(other);

        // Start building the Joined Set
        var joinedSet = new Multiset();

        // This is the new Join algorithm which is O(2n) so much faster and scalable
        // Downside is that it does require more memory than the old algorithm
        var values = new List<MultiDictionary<INode, List<int>>>();
        var nulls = new List<List<int>>();
        foreach (var var in joinVars)
        {
            joinedSet.AddVariable(var);
            values.Add(new MultiDictionary<INode, List<int>>(new FastVirtualNodeComparer()));
            nulls.Add(new List<int>());
        }

        // First do a pass over the LHS Result to find all possible values for joined variables
        foreach (ISet x in Sets)
        {
            var i = 0;
            foreach (var var in joinVars)
            {
                INode value = x[var];
                if (value != null)
                {
                    if (values[i].TryGetValue(value, out List<int> ids))
                    {
                        ids.Add(x.ID);
                    }
                    else
                    {
                        values[i].Add(value, new List<int> { x.ID });
                    }
                }
                else
                {
                    nulls[i].Add(x.ID);
                }
                i++;
            }
        }

        if (UsePLinqEvaluation)
        {
            // Use a parallel join
            other.Sets.AsParallel().ForAll(y => EvalJoin(y, joinVars, values, nulls, joinedSet));
        }
        else
        {
            // Use a serial join
            // Then do a pass over the RHS and work out the intersections
            foreach (ISet y in other.Sets)
            {
                EvalJoin(y, joinVars, values, nulls, joinedSet);
            }
        }
        return joinedSet;
    }

    private void EvalJoin(ISet y, List<string> joinVars, List<MultiDictionary<INode, List<int>>> values, List<List<int>> nulls, BaseMultiset joinedSet)
    {
        IEnumerable<int> possMatches = null;
        var i = 0;
        foreach (var var in joinVars)
        {
            INode value = y[var];
            if (value != null)
            {
                if (values[i].ContainsKey(value))
                {
                    possMatches = (possMatches == null ? values[i][value].Concat(nulls[i]) : possMatches.Intersect(values[i][value].Concat(nulls[i])));
                }
                else
                {
                    possMatches = Enumerable.Empty<int>();
                    break;
                }
            }
            else
            {
                // Don't forget that a null will be potentially compatible with everything
                possMatches = (possMatches == null ? SetIDs : possMatches.Intersect(SetIDs));
            }
            i++;
        }
        if (possMatches == null) return;

        // Now do the actual joins for the current set
        foreach (var poss in possMatches)
        {
            if (this[poss].IsCompatibleWith(y, joinVars))
            {
                joinedSet.Add(this[poss].Join(y));
            }
        }
    }

    /// <summary>
    /// Does a Left Join of this Multiset to another Multiset where the Join is predicated on the given Expression.
    /// </summary>
    /// <param name="other">Other Multiset.</param>
    /// <param name="expr">Expression.</param>
    /// <param name="baseContext">The base evaluation context providing access to the query evaluation options to use when evaluating the join.</param>
    /// <param name="expressionProcessor">The processor to use to evaluate <paramref name="expr"/>.</param>
    /// <returns></returns>
    public virtual BaseMultiset LeftJoin(BaseMultiset other, ISparqlExpression expr, SparqlEvaluationContext baseContext, ISparqlExpressionProcessor<IValuedNode, SparqlEvaluationContext, int> expressionProcessor)
    {
        // If the Other is the Identity/Null Multiset the result is this Multiset
        if (other is IdentityMultiset) return this;
        if (other is NullMultiset) return this;
        if (other.IsEmpty) return this;

        var joinedSet = new Multiset();
        var binder = new LeviathanLeftJoinBinder(joinedSet);
        var subcontext = new SparqlEvaluationContext(binder, baseContext.Options);

        // Find the First Variable from this Multiset which is in both Multisets
        // If there is no Variable from this Multiset in the other Multiset then this
        // should be a Join operation instead of a LeftJoin
        var joinVars = Variables.Where(v => other.Variables.Contains(v)).ToList();
        if (joinVars.Count == 0)
        {
            if (subcontext.Options.UsePLinqEvaluation && expr.CanParallelise)
            {
                var partitionedSet = new PartitionedMultiset(Count, other.Count + 1);
                Sets.AsParallel().ForAll(x => EvalLeftJoinProduct(x, other, partitionedSet, expr, baseContext, expressionProcessor));
                return partitionedSet;
            }
            // Do a serial Left Join Product

            // Calculate a Product filtering as we go
            foreach (ISet x in Sets)
            {
                var standalone = false;
                var matched = false;
                foreach (ISet y in other.Sets)
                {
                    ISet z = x.Join(y);
                    try
                    {
                        joinedSet.Add(z);
                        if (!expr.Accept(expressionProcessor, subcontext, z.ID).AsSafeBoolean())
                        {
                            joinedSet.Remove(z.ID);
                            standalone = true;
                        }
                        else
                        {
                            matched = true;
                        }
                    }
                    catch
                    {
                        joinedSet.Remove(z.ID);
                        standalone = true;
                    }
                }
                if (standalone && !matched) joinedSet.Add(x.Copy());
            }
        }
        else
        {
            // This is the new Join algorithm which is also correct but is O(2n) so much faster and scalable
            // Downside is that it does require more memory than the old algorithm
            var values = new List<MultiDictionary<INode, List<int>>>();
            var nulls = new List<List<int>>();
            foreach (var var in joinVars)
            {
                joinedSet.AddVariable(var);
                values.Add(new MultiDictionary<INode, List<int>>(new FastVirtualNodeComparer()));
                nulls.Add(new List<int>());
            }

            // First do a pass over the RHS Result to find all possible values for joined variables
            foreach (ISet y in other.Sets)
            {
                var i = 0;
                foreach (var var in joinVars)
                {
                    INode value = y[var];
                    if (value != null)
                    {
                        if (values[i].TryGetValue(value, out List<int> ids))
                        {
                            ids.Add(y.ID);
                        }
                        else
                        {
                            values[i].Add(value, new List<int> { y.ID });
                        }
                    }
                    else
                    {
                        nulls[i].Add(y.ID);
                    }
                    i++;
                }
            }

            // Then do a pass over the LHS and work out the intersections
            if (subcontext.Options.UsePLinqEvaluation && expr.CanParallelise)
            {
                Sets.AsParallel().ForAll(x => EvalLeftJoin(x, other, joinVars, values, nulls, joinedSet, subcontext, expr, expressionProcessor));
            }
            else
            {
                // Use a Serial Left Join
                foreach (ISet x in Sets)
                {
                    EvalLeftJoin(x, other, joinVars, values, nulls, joinedSet, subcontext, expr, expressionProcessor);
                }
            }
        }
        return joinedSet;
    }

    private void EvalLeftJoinProduct(ISet x, BaseMultiset other, PartitionedMultiset partitionedSet, ISparqlExpression expr, SparqlEvaluationContext baseContext, ISparqlExpressionProcessor<IValuedNode, SparqlEvaluationContext, int> expressionProcessor)
    {
        var binder = new LeviathanLeftJoinBinder(partitionedSet);
        var subcontext = new SparqlEvaluationContext(binder, baseContext.Options);
        bool standalone = false, matched = false;

        var id = partitionedSet.GetNextBaseID();
        foreach (ISet y in other.Sets)
        {
            id++;
            ISet z = x.Join(y);
            z.ID = id;
            try
            {
                partitionedSet.Add(z);
                if (!expr.Accept(expressionProcessor, subcontext, z.ID).AsSafeBoolean())
                {
                    partitionedSet.Remove(z.ID);
                    standalone = true;
                }
                else
                {
                    matched = true;
                }
            }
            catch
            {
                partitionedSet.Remove(z.ID);
                standalone = true;
            }
        }
        if (standalone && !matched) {
            id++;
            ISet z = x.Copy();
            z.ID = id;
            partitionedSet.Add(z);
        }
    }

    private void EvalLeftJoin(ISet x, BaseMultiset other, List<string> joinVars, List<MultiDictionary<INode, List<int>>> values, List<List<int>> nulls, BaseMultiset joinedSet, SparqlEvaluationContext subcontext, ISparqlExpression expr, ISparqlExpressionProcessor<IValuedNode, SparqlEvaluationContext, int> expressionProcessor)
    {
        IEnumerable<int> possMatches = null;
        var i = 0;
        foreach (var var in joinVars)
        {
            INode value = x[var];
            if (value != null)
            {
                if (values[i].ContainsKey(value))
                {
                    possMatches = (possMatches == null ? values[i][value].Concat(nulls[i]) : possMatches.Intersect(values[i][value].Concat(nulls[i])));
                }
                else
                {
                    possMatches = Enumerable.Empty<int>();
                    break;
                }
            }
            else
            {
                // Don't forget that a null will be potentially compatible with everything
                possMatches = (possMatches == null ? other.SetIDs : possMatches.Intersect(other.SetIDs));
            }
            i++;
        }

        // If no possible matches just copy LHS across
        if (possMatches == null)
        {
            joinedSet.Add(x.Copy());
            return;
        }

        // Now do the actual joins for the current set
        var standalone = false;
        var matched = false;
        foreach (var poss in possMatches)
        {
            if (other[poss].IsCompatibleWith(x, joinVars))
            {
                ISet z = x.Join(other[poss]);
                joinedSet.Add(z);
                try
                {
                    if (!expr.Accept(expressionProcessor, subcontext, z.ID).AsSafeBoolean())
                    {
                        joinedSet.Remove(z.ID);
                    }
                    else
                    {
                        matched = true;
                    }
                }
                catch
                {
                    joinedSet.Remove(z.ID);
                    standalone = true;
                }
            }
        }

        if (standalone || !matched) joinedSet.Add(x.Copy());
    }

    /// <summary>
    /// Does an Exists Join of this Multiset to another Multiset where the Join is predicated on the existence/non-existence of a joinable solution on the RHS.
    /// </summary>
    /// <param name="other">Other Multiset.</param>
    /// <param name="mustExist">Whether a solution must exist in the Other Multiset for the join to be made.</param>
    /// <returns></returns>
    public virtual BaseMultiset ExistsJoin(BaseMultiset other, bool mustExist)
    {
        // For EXISTS and NOT EXISTS if the other is the Identity then it has no effect
        if (other is IdentityMultiset) return this;
        if (mustExist)
        {
            // If an EXISTS then Null/Empty Other results in Null
            if (other is NullMultiset) return other;
            if (other.IsEmpty) return new NullMultiset();
        }
        else
        {
            // If a NOT EXISTS then Null/Empty results in this
            if (other is NullMultiset) return this;
            if (other.IsEmpty) return this;
        }

        // Find the Variables that are to be used for Joining
        var joinVars = Variables.Where(v => other.Variables.Contains(v)).ToList();
        if (joinVars.Count == 0)
        {
            // All Disjoint Solutions are compatible
            if (mustExist)
            {
                // If an EXISTS and disjoint then result is this
                return this;
            }
            else
            {
                // If a NOT EXISTS and disjoint then result is null
                return new NullMultiset();
            }
        }

        // Start building the Joined Set
        var joinedSet = new Multiset();

        // This is the new algorithm which is also correct but is O(3n) so much faster and scalable
        // Downside is that it does require more memory than the old algorithm
        var values = new List<MultiDictionary<INode, List<int>>>();
        var nulls = new List<List<int>>();
        foreach (var var in joinVars)
        {
            joinedSet.AddVariable(var);
            values.Add(new MultiDictionary<INode, List<int>>(new FastVirtualNodeComparer()));
            nulls.Add(new List<int>());
        }

        // First do a pass over the LHS Result to find all possible values for joined variables
        foreach (ISet x in Sets)
        {
            var i = 0;
            foreach (var var in joinVars)
            {
                INode value = x[var];
                if (value != null)
                {
                    if (values[i].TryGetValue(value, out List<int> ids))
                    {
                        ids.Add(x.ID);
                    }
                    else
                    {
                        values[i].Add(value, new List<int> { x.ID });
                    }
                }
                else
                {
                    nulls[i].Add(x.ID);
                }
                i++;
            }
        }

        // Then do a pass over the RHS and work out the intersections
        var exists = new HashSet<int>();
        foreach (ISet y in other.Sets)
        {
            IEnumerable<int> possMatches = null;
            var i = 0;
            foreach (var var in joinVars)
            {
                INode value = y[var];
                if (value != null)
                {
                    if (values[i].ContainsKey(value))
                    {
                        possMatches = (possMatches == null ? values[i][value].Concat(nulls[i]) : possMatches.Intersect(values[i][value].Concat(nulls[i])));
                    }
                    else
                    {
                        possMatches = Enumerable.Empty<int>();
                        break;
                    }
                }
                else
                {
                    // Don't forget that a null will be potentially compatible with everything
                    possMatches = (possMatches == null ? SetIDs : possMatches.Intersect(SetIDs));
                }
                i++;
            }
            if (possMatches == null) continue;

            // Look at possible matches, if is a valid match then mark the set as having an existing match
            // Don't reconsider sets which have already been marked as having an existing match
            foreach (var poss in possMatches)
            {
                if (exists.Contains(poss)) continue;
                if (this[poss].IsCompatibleWith(y, joinVars))
                {
                    exists.Add(poss);
                }
            }
        }

        // Apply the actual exists
        if (exists.Count == Count)
        {
            // If number of sets that have a match is equal to number of sets then we're either returning everything or nothing
            if (mustExist)
            {
                return this;
            }
            else
            {
                return new NullMultiset();
            }
        }
        else
        {
            // Otherwise iterate
            foreach (ISet x in Sets)
            {
                if (mustExist)
                {
                    if (exists.Contains(x.ID))
                    {
                        joinedSet.Add(x.Copy());
                    }
                }
                else
                {
                    if (!exists.Contains(x.ID))
                    {
                        joinedSet.Add(x.Copy());
                    }
                }
            }
        }

        return joinedSet;
    }

    /// <summary>
    /// Does a Minus Join of this Multiset to another Multiset where any joinable results are subtracted from this Multiset to give the resulting Multiset.
    /// </summary>
    /// <param name="other">Other Multiset.</param>
    /// <returns></returns>
    public virtual BaseMultiset MinusJoin(BaseMultiset other)
    {
        // If the other Multiset is the Identity/Null Multiset then minus-ing it doesn't alter this set
        if (other is IdentityMultiset) return this;
        if (other is NullMultiset) return this;
        // If the other Multiset is disjoint then minus-ing it also doesn't alter this set
        if (IsDisjointWith(other)) return this;

        // Find the Variables that are to be used for Joining
        var joinVars = Variables.Where(v => other.Variables.Contains(v)).ToList();
        if (joinVars.Count == 0) return Product(other);

        // Start building the Joined Set
        var joinedSet = new Multiset();

        // This is the new algorithm which is also correct but is O(3n) so much faster and scalable
        // Downside is that it does require more memory than the old algorithm
        var values = new List<MultiDictionary<INode, List<int>>>();
        var nulls = new List<List<int>>();
        foreach (var var in joinVars)
        {
            values.Add(new MultiDictionary<INode, List<int>>(new FastVirtualNodeComparer()));
            nulls.Add(new List<int>());
        }

        // First do a pass over the LHS Result to find all possible values for joined variables
        foreach (ISet x in Sets)
        {
            var i = 0;
            foreach (var var in joinVars)
            {
                INode value = x[var];
                if (value != null)
                {
                    if (values[i].TryGetValue(value, out List<int> ids))
                    {
                        ids.Add(x.ID);
                    }
                    else
                    {
                        values[i].Add(value, new List<int> { x.ID });
                    }
                }
                else
                {
                    nulls[i].Add(x.ID);
                }
                i++;
            }
        }

        // Then do a pass over the RHS and work out the intersections
        var toMinus = new HashSet<int>();
        foreach (ISet y in other.Sets)
        {
            IEnumerable<int> possMatches = null;
            var i = 0;
            foreach (var var in joinVars)
            {
                INode value = y[var];
                if (value != null)
                {
                    if (values[i].ContainsKey(value))
                    {
                        possMatches = (possMatches == null ? values[i][value].Concat(nulls[i]) : possMatches.Intersect(values[i][value].Concat(nulls[i])));
                    }
                    else
                    {
                        possMatches = Enumerable.Empty<int>();
                        break;
                    }
                }
                else
                {
                    // Don't forget that a null will be potentially compatible with everything
                    possMatches = (possMatches == null ? SetIDs : possMatches.Intersect(SetIDs));
                }
                i++;
            }
            if (possMatches == null) continue;

            // Look at possible matches, if is a valid match then mark the matched set for minus'ing
            // Don't reconsider sets which have already been marked for minusing
            foreach (var poss in possMatches)
            {
                if (toMinus.Contains(poss)) continue;
                if (this[poss].IsMinusCompatibleWith(y, joinVars))
                {
                    toMinus.Add(poss);
                }
            }
        }

        // Apply the actual minus
        if (toMinus.Count == Count)
        {
            // If number of sets to minus is equal to number of sets then we're minusing everything
            return new NullMultiset();
        }
        else
        {
            // Otherwise iterate
            foreach (ISet x in Sets)
            {
                if (!toMinus.Contains(x.ID))
                {
                    joinedSet.Add(x.Copy());
                }
            }
        }

        return joinedSet;
    }

    /// <summary>
    /// Does a Product of this Multiset and another Multiset.
    /// </summary>
    /// <param name="other">Other Multiset.</param>
    /// <returns></returns>
    public virtual BaseMultiset Product(BaseMultiset other)
    {
        if (other is IdentityMultiset) return this;
        if (other is NullMultiset) return other;
        if (other.IsEmpty) return new NullMultiset();

        if (UsePLinqEvaluation)
        {
            // Determine partition sizes so we can do a parallel product
            // Want to parallelize over whichever side is larger
            PartitionedMultiset partitionedSet;
            if (Count >= other.Count)
            {
                partitionedSet = new PartitionedMultiset(Count, other.Count);
                Sets.AsParallel().ForAll(x => EvalProduct(x, other, partitionedSet));
            }
            else
            {
                partitionedSet = new PartitionedMultiset(other.Count, Count);
                other.Sets.AsParallel().ForAll(y => EvalProduct(y, this, partitionedSet));
            }
            return partitionedSet;
        }
        else
        {
            // Use serial calculation which is likely to really suck for big products
            var productSet = new Multiset();
            foreach (ISet x in Sets)
            {
                foreach (ISet y in other.Sets)
                {
                    productSet.Add(x.Join(y));
                }
            }
            return productSet;
        }
    }

    private void EvalProduct(ISet x, BaseMultiset other, PartitionedMultiset productSet)
    {
        var id = productSet.GetNextBaseID();
        foreach (ISet y in other.Sets)
        {
            id++;
            ISet z = x.Join(y);
            z.ID = id;
            productSet.Add(z);
        }
    }

    /// <summary>
    /// Does a Union of this Multiset and another Multiset.
    /// </summary>
    /// <param name="other">Other Multiset.</param>
    /// <returns></returns>
    public virtual BaseMultiset Union(BaseMultiset other)
    {
        if (other is IdentityMultiset) return this;
        if (other is NullMultiset) return this;
        if (other.IsEmpty) return this;

        foreach (ISet s in other.Sets)
        {
            Add(s.Copy());
        }
        return this;
    }

    /// <summary>
    /// Does a merge of this multiset and another multiset, avoiding adding duplicate sets to this multiset.
    /// </summary>
    /// <param name="other">The multiset to be merged into this multiset.</param>
    /// <returns>This multiset.</returns>
    /// <remarks>The merge operation adds only sets from <paramref name="other"/> that do not exist in this multiset.</remarks>
    public virtual BaseMultiset Merge(BaseMultiset other)
    {
        if (other is IdentityMultiset || other is NullMultiset || other.IsEmpty) return this;
        var sets = new HashSet<ISet>(Sets);
        foreach (var s in other.Sets)
        {
            // A simple hash of s does not work here as if s is a superset of a set in other
            // the hash codes are different even though the sets compare equal.
            // Instead we test using a trimmed version of s that contains only the variables in this multiset
            // to ensure that hash code and equality tests match
            // Although this does add some processing overhead, it is still much faster for moderate to large sets.
            var trimmedSet = new Set();
            foreach (var v in this.Variables)
            {
                trimmedSet.Add(v, s[v]);
            }
            if (!sets.Contains(trimmedSet))
            {
                sets.Add(trimmedSet);
                Add(s.Copy());
            }
        }
        return this;
    }

    /// <summary>
    /// Determines whether the Multiset contains the given Value for the given Variable.
    /// </summary>
    /// <param name="var">Variable.</param>
    /// <param name="n">Value.</param>
    /// <returns></returns>
    public abstract bool ContainsValue(string var, INode n);

    /// <summary>
    /// Determines whether the Multiset contains the given Variable.
    /// </summary>
    /// <param name="var">Variable.</param>
    /// <returns></returns>
    public abstract bool ContainsVariable(string var);

    /// <summary>
    /// Determines whether the Multiset contains all of the given variables.
    /// </summary>
    /// <param name="vars">Variables.</param>
    /// <returns>True if all of the variables in <paramref name="vars"/> are contained in the multiset, or if <paramref name="vars"/> is the empty set.</returns>
    public abstract bool ContainsVariables(IEnumerable<string> vars);

    /// <summary>
    /// Determines whether the Multiset is disjoint with the given Multiset.
    /// </summary>
    /// <param name="other">Multiset.</param>
    /// <returns></returns>
    public abstract bool IsDisjointWith(BaseMultiset other);

    /// <summary>
    /// Adds a Set to the Multiset.
    /// </summary>
    /// <param name="s">Set to add.</param>
    public abstract void Add(ISet s);

    /// <summary>
    /// Adds a Variable to the Multiset.
    /// </summary>
    /// <param name="variable">Variable.</param>
    public abstract void AddVariable(string variable);

    /// <summary>
    /// Sets the variable ordering for the multiset.
    /// </summary>
    /// <param name="variables">Variable Ordering.</param>
    public abstract void SetVariableOrder(IEnumerable<string> variables);

    /// <summary>
    /// Removes a Set (by ID) from the Multiset.
    /// </summary>
    /// <param name="id">ID.</param>
    public abstract void Remove(int id);

    /// <summary>
    /// Sorts a Set based on the given Comparer.
    /// </summary>
    /// <param name="comparer">Comparer on Sets.</param>
    public virtual void Sort(IComparer<ISet> comparer)
    {
        if (comparer != null)
        {
            _orderedIDs = (from s in Sets.OrderBy(x => x, comparer)
                                select s.ID).ToList();
        }
        else
        {
            _orderedIDs = null;
        }
    }

    /// <summary>
    /// Returns whether the Multiset is Empty.
    /// </summary>
    public abstract bool IsEmpty
    {
        get;
    }

    /// <summary>
    /// Gets the Count of Sets in the Multiset.
    /// </summary>
    public virtual int Count
    {
        get
        {
            return 0;
        }
    }

    /// <summary>
    /// Trims the Multiset of Temporary Variables.
    /// </summary>
    public virtual void Trim()
    {
        // Does nothing by default
    }

    /// <summary>
    /// Trims the Multiset by removing all Values for the given Variable.
    /// </summary>
    /// <param name="variable">Variable.</param>
    public virtual void Trim(string variable)
    {

    }

    /// <summary>
    /// Gets the Variables in the Multiset.
    /// </summary>
    public abstract IEnumerable<string> Variables
    {
        get;
    }

    /// <summary>
    /// Gets the Sets in the Multiset.
    /// </summary>
    public abstract IEnumerable<ISet> Sets
    {
        get;
    }

    /// <summary>
    /// Gets the IDs of Sets in the Multiset.
    /// </summary>
    public abstract IEnumerable<int> SetIDs
    {
        get;
    }

    /// <summary>
    /// Retrieves the Set with the given ID.
    /// </summary>
    /// <param name="id">ID.</param>
    /// <returns></returns>
    public abstract ISet this[int id]
    {
        get;
    }

    /// <summary>
    /// Gets the string representation of the multiset (intended for debugging only).
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return string.Join(", ", Variables.ToArray()) + " (" + Count + " Results)";
    }
}
