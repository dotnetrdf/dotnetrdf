/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Common;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Abstract Base Class for representing Multisets
    /// </summary>
    public abstract class BaseMultiset
    {
        /// <summary>
        /// List of IDs that is used to return the Sets in order if the Multiset has been sorted
        /// </summary>
        protected List<int> _orderedIDs = null;

        /// <summary>
        /// Joins this Multiset to another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns></returns>
        public virtual BaseMultiset Join(BaseMultiset other)
        {
            //If the Other is the Identity Multiset the result is this Multiset
            if (other is IdentityMultiset) return this;
            //If the Other is the Null Multiset the result is the Null Multiset
            if (other is NullMultiset) return other;
            //If the Other is Empty then the result is the Null Multiset
            if (other.IsEmpty) return new NullMultiset();

            //Find the First Variable from this Multiset which is in both Multisets
            //If there is no Variable from this Multiset in the other Multiset then this
            //should be a Product operation instead of a Join
            List<String> joinVars = this.Variables.Where(v => other.Variables.Contains(v)).ToList();
            if (joinVars.Count == 0) return this.Product(other);

            //Start building the Joined Set
            Multiset joinedSet = new Multiset();

            //This is the new Join algorithm which is O(2n) so much faster and scalable
            //Downside is that it does require more memory than the old algorithm
            List<MultiDictionary<INode, List<int>>> values = new List<MultiDictionary<INode, List<int>>>();
            List<List<int>> nulls = new List<List<int>>();
            foreach (String var in joinVars)
            {
                values.Add(new MultiDictionary<INode, List<int>>(new FastNodeComparer()));
                nulls.Add(new List<int>());
            }

            //First do a pass over the LHS Result to find all possible values for joined variables
            foreach (ISet x in this.Sets)
            {
                int i = 0;
                foreach (String var in joinVars)
                {
                    INode value = x[var];
                    if (value != null)
                    {
                        List<int> ids;
                        if (values[i].TryGetValue(value, out ids))
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

#if NET40 && !SILVERLIGHT
            if (Options.UsePLinqEvaluation)
            {
                //Use a paralllel join
                other.Sets.AsParallel().ForAll(y => EvalJoin(y, joinVars, values, nulls, joinedSet));
            }
            else
            {
#endif
                //Use a serial join
                //Then do a pass over the RHS and work out the intersections
                foreach (ISet y in other.Sets)
                {
                    this.EvalJoin(y, joinVars, values, nulls, joinedSet);
                }
#if NET40 && !SILVERLIGHT
            }
#endif

            return joinedSet;
        }

        private void EvalJoin(ISet y, List<String> joinVars, List<MultiDictionary<INode, List<int>>> values, List<List<int>> nulls, BaseMultiset joinedSet)
        {
            IEnumerable<int> possMatches = null;
            int i = 0;
            foreach (String var in joinVars)
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
                    //Don't forget that a null will be potentially compatible with everything
                    possMatches = (possMatches == null ? this.SetIDs : possMatches.Intersect(this.SetIDs));
                }
                i++;
            }
            if (possMatches == null) return;

            //Now do the actual joins for the current set
            foreach (int poss in possMatches)
            {
                if (this[poss].IsCompatibleWith(y, joinVars))
                {
                    joinedSet.Add(this[poss].Join(y));
                }
            }
        }

        /// <summary>
        /// Does a Left Join of this Multiset to another Multiset where the Join is predicated on the given Expression
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <param name="expr">Expression</param>
        /// <returns></returns>
        public virtual BaseMultiset LeftJoin(BaseMultiset other, ISparqlExpression expr)
        {
            //If the Other is the Identity/Null Multiset the result is this Multiset
            if (other is IdentityMultiset) return this;
            if (other is NullMultiset) return this;
            if (other.IsEmpty) return this;

            Multiset joinedSet = new Multiset();
            LeviathanLeftJoinBinder binder = new LeviathanLeftJoinBinder(joinedSet);
            SparqlEvaluationContext subcontext = new SparqlEvaluationContext(binder);

            //Find the First Variable from this Multiset which is in both Multisets
            //If there is no Variable from this Multiset in the other Multiset then this
            //should be a Join operation instead of a LeftJoin
            List<String> joinVars = this.Variables.Where(v => other.Variables.Contains(v)).ToList();
            if (joinVars.Count == 0)
            {
#if NET40 && !SILVERLIGHT
                if (Options.UsePLinqEvaluation && expr.CanParallelise)
                {
                    PartitionedMultiset partitionedSet = new PartitionedMultiset(this.Count, other.Count + 1);
                    this.Sets.AsParallel().ForAll(x => EvalLeftJoinProduct(x, other, partitionedSet, expr));
                    return partitionedSet;
                }
                else
                {
#endif
                    //Calculate a Product filtering as we go
                    foreach (ISet x in this.Sets)
                    {
                        bool standalone = false;
                        foreach (ISet y in other.Sets)
                        {
                            ISet z = x.Join(y);
                            try
                            {
                                joinedSet.Add(z);
                                if (!expr.Evaluate(subcontext, z.ID).AsSafeBoolean())
                                {
                                    joinedSet.Remove(z.ID);
                                    standalone = true;
                                }
                            }
                            catch
                            {
                                joinedSet.Remove(z.ID);
                                standalone = true;
                            }
                        }
                        if (standalone) joinedSet.Add(x.Copy());
                    }
#if NET40 && !SILVERLIGHT
                }
#endif
            }
            else
            {
                //This is the new Join algorithm which is also correct but is O(2n) so much faster and scalable
                //Downside is that it does require more memory than the old algorithm
                List<MultiDictionary<INode, List<int>>> values = new List<MultiDictionary<INode, List<int>>>();
                List<List<int>> nulls = new List<List<int>>();
                foreach (String var in joinVars)
                {
                    values.Add(new MultiDictionary<INode, List<int>>(new FastNodeComparer()));
                    nulls.Add(new List<int>());
                }

                //First do a pass over the LHS Result to find all possible values for joined variables
                HashSet<int> matched = new HashSet<int>();
                HashSet<int> standalone = new HashSet<int>();
                foreach (ISet x in this.Sets)
                {
                    int i = 0;
                    foreach (String var in joinVars)
                    {
                        INode value = x[var];
                        if (value != null)
                        {
                            List<int> ids;
                            if (values[i].TryGetValue(value, out ids))
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

                //Then do a pass over the RHS and work out the intersections
#if NET40 && !SILVERLIGHT
                if (Options.UsePLinqEvaluation && expr.CanParallelise)
                {
                    other.Sets.AsParallel().ForAll(y => EvalLeftJoin(y, joinVars, values, nulls, joinedSet, subcontext, expr, standalone, matched));
                }
                else
                {
#endif
                    //Use a Serial Left Join
                    foreach (ISet y in other.Sets)
                    {
                        this.EvalLeftJoin(y, joinVars, values, nulls, joinedSet, subcontext, expr, standalone, matched);
                    }
#if NET40 && !SILVERLIGHT
                }
#endif

                //Finally add in unmatched sets from LHS
                foreach (int id in this.SetIDs)
                {
                    if (!matched.Contains(id) || standalone.Contains(id)) joinedSet.Add(this[id].Copy());
                }
            }
            return joinedSet;
        }

#if NET40 && !SILVERLIGHT
        
        private void EvalLeftJoinProduct(ISet x, BaseMultiset other, PartitionedMultiset partitionedSet, ISparqlExpression expr)
        {
            LeviathanLeftJoinBinder binder = new LeviathanLeftJoinBinder(partitionedSet);
            SparqlEvaluationContext subcontext = new SparqlEvaluationContext(binder);
            bool standalone = false;

            int id = partitionedSet.GetNextBaseID();
            foreach (ISet y in other.Sets)
            {
                id++;
                ISet z = x.Join(y);
                z.ID = id;
                try
                {
                    partitionedSet.Add(z);
                    if (!expr.Evaluate(subcontext, z.ID).AsSafeBoolean())
                    {
                        partitionedSet.Remove(z.ID);
                        standalone = true;
                    }
                }
                catch
                {
                    partitionedSet.Remove(z.ID);
                    standalone = true;
                }
            }
            if (standalone)
            {
                id++;
                ISet z = x.Copy();
                z.ID = id;
                partitionedSet.Add(z);
            }
        }

#endif

        private void EvalLeftJoin(ISet y, List<String> joinVars, List<MultiDictionary<INode, List<int>>> values, List<List<int>> nulls, BaseMultiset joinedSet, SparqlEvaluationContext subcontext, ISparqlExpression expr, HashSet<int> standalone, HashSet<int> matched)
        {
            IEnumerable<int> possMatches = null;
            int i = 0;
            foreach (String var in joinVars)
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
                    //Don't forget that a null will be potentially compatible with everything
                    possMatches = (possMatches == null ? this.SetIDs : possMatches.Intersect(this.SetIDs));
                }
                i++;
            }
            if (possMatches == null) return;

            //Now do the actual joins for the current set
            foreach (int poss in possMatches)
            {
                if (this[poss].IsCompatibleWith(y, joinVars))
                {
                    ISet z = this[poss].Join(y);
                    joinedSet.Add(z);
                    try
                    {
                        if (!expr.Evaluate(subcontext, z.ID).AsSafeBoolean())
                        {
                            joinedSet.Remove(z.ID);
                            standalone.Add(poss);
                        }
                        else
                        {
                            matched.Add(poss);
                        }
                    }
                    catch
                    {
                        joinedSet.Remove(z.ID);
                        standalone.Add(poss);
                    }
                }
            }
        }

        /// <summary>
        /// Does an Exists Join of this Multiset to another Multiset where the Join is predicated on the existence/non-existence of a joinable solution on the RHS
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <param name="mustExist">Whether a solution must exist in the Other Multiset for the join to be made</param>
        /// <returns></returns>
        public virtual BaseMultiset ExistsJoin(BaseMultiset other, bool mustExist)
        {
            //For EXISTS and NOT EXISTS if the other is the Identity then it has no effect
            if (other is IdentityMultiset) return this;
            if (mustExist)
            {
                //If an EXISTS then Null/Empty Other results in Null
                if (other is NullMultiset) return other;
                if (other.IsEmpty) return new NullMultiset();
            }
            else
            {
                //If a NOT EXISTS then Null/Empty results in this
                if (other is NullMultiset) return this;
                if (other.IsEmpty) return this;
            }

            //Find the Variables that are to be used for Joining
            List<String> joinVars = this.Variables.Where(v => other.Variables.Contains(v)).ToList();
            if (joinVars.Count == 0)
            {
                //All Disjoint Solutions are compatible
                if (mustExist)
                {
                    //If an EXISTS and disjoint then result is this
                    return this;
                }
                else
                {
                    //If a NOT EXISTS and disjoint then result is null
                    return new NullMultiset();
                }
            }

            //Start building the Joined Set
            Multiset joinedSet = new Multiset();

            //This is the new algorithm which is also correct but is O(3n) so much faster and scalable
            //Downside is that it does require more memory than the old algorithm
            List<MultiDictionary<INode, List<int>>> values = new List<MultiDictionary<INode, List<int>>>();
            List<List<int>> nulls = new List<List<int>>();
            foreach (String var in joinVars)
            {
                values.Add(new MultiDictionary<INode, List<int>>(new FastNodeComparer()));
                nulls.Add(new List<int>());
            }

            //First do a pass over the LHS Result to find all possible values for joined variables
            foreach (ISet x in this.Sets)
            {
                int i = 0;
                foreach (String var in joinVars)
                {
                    INode value = x[var];
                    if (value != null)
                    {
                        List<int> ids;
                        if (values[i].TryGetValue(value, out ids))
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

            //Then do a pass over the RHS and work out the intersections
            HashSet<int> exists = new HashSet<int>();
            foreach (ISet y in other.Sets)
            {
                IEnumerable<int> possMatches = null;
                int i = 0;
                foreach (String var in joinVars)
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
                        //Don't forget that a null will be potentially compatible with everything
                        possMatches = (possMatches == null ? this.SetIDs : possMatches.Intersect(this.SetIDs));
                    }
                    i++;
                }
                if (possMatches == null) continue;

                //Look at possible matches, if is a valid match then mark the set as having an existing match
                //Don't reconsider sets which have already been marked as having an existing match
                foreach (int poss in possMatches)
                {
                    if (exists.Contains(poss)) continue;
                    if (this[poss].IsCompatibleWith(y, joinVars))
                    {
                        exists.Add(poss);
                    }
                }
            }

            //Apply the actual exists
            if (exists.Count == this.Count)
            {
                //If number of sets that have a match is equal to number of sets then we're either returning everything or nothing
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
                //Otherwise iterate
                foreach (ISet x in this.Sets)
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
        /// Does a Minus Join of this Multiset to another Multiset where any joinable results are subtracted from this Multiset to give the resulting Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns></returns>
        public virtual BaseMultiset MinusJoin(BaseMultiset other)
        {
            //If the other Multiset is the Identity/Null Multiset then minus-ing it doesn't alter this set
            if (other is IdentityMultiset) return this;
            if (other is NullMultiset) return this;
            //If the other Multiset is disjoint then minus-ing it also doesn't alter this set
            if (this.IsDisjointWith(other)) return this;

            //Find the Variables that are to be used for Joining
            List<String> joinVars = this.Variables.Where(v => other.Variables.Contains(v)).ToList();
            if (joinVars.Count == 0) return this.Product(other);

            //Start building the Joined Set
            Multiset joinedSet = new Multiset();

            //This is the new algorithm which is also correct but is O(3n) so much faster and scalable
            //Downside is that it does require more memory than the old algorithm
            List<MultiDictionary<INode, List<int>>> values = new List<MultiDictionary<INode, List<int>>>();
            List<List<int>> nulls = new List<List<int>>();
            foreach (String var in joinVars)
            {
                values.Add(new MultiDictionary<INode, List<int>>(new FastNodeComparer()));
                nulls.Add(new List<int>());
            }

            //First do a pass over the LHS Result to find all possible values for joined variables
            foreach (ISet x in this.Sets)
            {
                int i = 0;
                foreach (String var in joinVars)
                {
                    INode value = x[var];
                    if (value != null)
                    {
                        List<int> ids;
                        if (values[i].TryGetValue(value, out ids))
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

            //Then do a pass over the RHS and work out the intersections
            HashSet<int> toMinus = new HashSet<int>();
            foreach (ISet y in other.Sets)
            {
                IEnumerable<int> possMatches = null;
                int i = 0;
                foreach (String var in joinVars)
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
                        //Don't forget that a null will be potentially compatible with everything
                        possMatches = (possMatches == null ? this.SetIDs : possMatches.Intersect(this.SetIDs));
                    }
                    i++;
                }
                if (possMatches == null) continue;

                //Look at possible matches, if is a valid match then mark the matched set for minus'ing
                //Don't reconsider sets which have already been marked for minusing
                foreach (int poss in possMatches)
                {
                    if (toMinus.Contains(poss)) continue;
                    if (this[poss].IsMinusCompatibleWith(y, joinVars))
                    {
                        toMinus.Add(poss);
                    }
                }
            }

            //Apply the actual minus
            if (toMinus.Count == this.Count)
            {
                //If number of sets to minus is equal to number of sets then we're minusing everything
                return new NullMultiset();
            }
            else
            {
                //Otherwise iterate
                foreach (ISet x in this.Sets)
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
        /// Does a Product of this Multiset and another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns></returns>
        public virtual BaseMultiset Product(BaseMultiset other)
        {
            if (other is IdentityMultiset) return this;
            if (other is NullMultiset) return other;
            if (other.IsEmpty) return new NullMultiset();

#if NET40 && !SILVERLIGHT
            if (Options.UsePLinqEvaluation)
            {
                //Determine partition sizes so we can do a parallel product
                //Want to parallelize over whichever side is larger
                PartitionedMultiset partitionedSet;
                if (this.Count >= other.Count)
                {
                    partitionedSet = new PartitionedMultiset(this.Count, other.Count);
                    this.Sets.AsParallel().ForAll(x => this.EvalProduct(x, other, partitionedSet));
                }
                else
                {
                    partitionedSet = new PartitionedMultiset(other.Count, this.Count);
                    other.Sets.AsParallel().ForAll(y => this.EvalProduct(y, this, partitionedSet));
                }
                return partitionedSet;
            }
            else
            {
#endif
                //Use serial calculation which is likely to really suck for big products
                Multiset productSet = new Multiset();
                foreach (ISet x in this.Sets)
                {
                    foreach (ISet y in other.Sets)
                    {
                        productSet.Add(x.Join(y));
                    }
                }
                return productSet;
#if NET40 && !SILVERLIGHT
            }
#endif
        }

#if NET40 && !SILVERLIGHT

        private void EvalProduct(ISet x, BaseMultiset other, PartitionedMultiset productSet)
        {
            int id = productSet.GetNextBaseID();
            foreach (ISet y in other.Sets)
            {
                id++;
                ISet z = x.Join(y);
                z.ID = id;
                productSet.Add(z);
            }
        }

#endif

        /// <summary>
        /// Does a Union of this Multiset and another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns></returns>
        public virtual BaseMultiset Union(BaseMultiset other)
        {
            if (other is IdentityMultiset) return this;
            if (other is NullMultiset) return this;
            if (other.IsEmpty) return this;

            foreach (ISet s in other.Sets)
            {
                this.Add(s.Copy());
            }
            return this;
        }

        /// <summary>
        /// Determines whether the Multiset contains the given Value for the given Variable
        /// </summary>
        /// <param name="var">Variable</param>
        /// <param name="n">Value</param>
        /// <returns></returns>
        public abstract bool ContainsValue(String var, INode n);

        /// <summary>
        /// Determines whether the Multiset contains the given Variable
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns></returns>
        public abstract bool ContainsVariable(String var);

        /// <summary>
        /// Determines whether the Mutliset is disjoint with the given Multiset
        /// </summary>
        /// <param name="other">Multiset</param>
        /// <returns></returns>
        public abstract bool IsDisjointWith(BaseMultiset other);

        /// <summary>
        /// Adds a Set to the Mutliset
        /// </summary>
        /// <param name="s">Set to add</param>
        public abstract void Add(ISet s);

        /// <summary>
        /// Adds a Variable to the Multiset
        /// </summary>
        /// <param name="variable">Variable</param>
        public abstract void AddVariable(String variable);

        /// <summary>
        /// Removes a Set (by ID) from the Multiset
        /// </summary>
        /// <param name="id">ID</param>
        public abstract void Remove(int id);

        /// <summary>
        /// Sorts a Set based on the given Comparer
        /// </summary>
        /// <param name="comparer">Comparer on Sets</param>
        public virtual void Sort(IComparer<ISet> comparer)
        {
            if (comparer != null)
            {
                this._orderedIDs = (from s in this.Sets.OrderBy(x => x, comparer)
                                    select s.ID).ToList();
            }
            else
            {
                this._orderedIDs = null;
            }
        }

        /// <summary>
        /// Returns whether the Multiset is Empty
        /// </summary>
        public abstract bool IsEmpty
        {
            get;
        }

        /// <summary>
        /// Gets the Count of Sets in the Multiset
        /// </summary>
        public virtual int Count
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Trims the Multiset of Temporary Variables
        /// </summary>
        public virtual void Trim()
        {
            //Does nothing by default
        }

        /// <summary>
        /// Trims the Multiset by removing all Values for the given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        public virtual void Trim(String variable)
        {

        }

        /// <summary>
        /// Gets the Variables in the Multiset
        /// </summary>
        public abstract IEnumerable<String> Variables
        {
            get;
        }

        /// <summary>
        /// Gets the Sets in the Multiset
        /// </summary>
        public abstract IEnumerable<ISet> Sets
        {
            get;
        }

        /// <summary>
        /// Gets the IDs of Sets in the Multiset
        /// </summary>
        public abstract IEnumerable<int> SetIDs
        {
            get;
        }

        /// <summary>
        /// Retrieves the Set with the given ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns></returns>
        public abstract ISet this[int id]
        {
            get;
        }
    }
}
