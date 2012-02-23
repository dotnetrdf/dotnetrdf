/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a Multiset of possible solutions
    /// </summary>
    public class Multiset 
        : BaseMultiset
    {
        /// <summary>
        /// Variables contained in the Multiset
        /// </summary>
        protected List<String> _variables = new List<string>();
        /// <summary>
        /// Dictionary of Sets in the Multiset
        /// </summary>
        protected Dictionary<int, ISet> _sets = new Dictionary<int,ISet>();
        /// <summary>
        /// Counter used to assign Set IDs
        /// </summary>
        protected int _counter = 0;
        /// <summary>
        /// List of IDs that is used to return the Sets in order if the Multiset has been sorted
        /// </summary>
        protected List<int> _orderedIDs = null;
        private Dictionary<String, HashSet<INode>> _containsCache;
        private bool _cacheInvalid = true;

        /// <summary>
        /// Creates a new Empty Multiset
        /// </summary>
        public Multiset() { }

        /// <summary>
        /// Creates a new Empty Mutliset that has the list of given Variables
        /// </summary>
        /// <param name="variables"></param>
        public Multiset(IEnumerable<String> variables)
        {
            foreach (String var in variables)
            {
                this._variables.Add(var);
            }
        }

        /// <summary>
        /// Creates a new Multiset from a SPARQL Result Set
        /// </summary>
        /// <param name="results">Result Set</param>
        internal Multiset(SparqlResultSet results)
        {
            foreach (String var in results.Variables)
            {
                this.AddVariable(var);
            }
            foreach (SparqlResult r in results.Results)
            {
                this.Add(new Set(r));
            }
        }

        /// <summary>
        /// Creates a new Multiset by flattening a Group Multiset
        /// </summary>
        /// <param name="multiset">Group Multiset</param>
        internal Multiset(GroupMultiset multiset)
        {
            foreach (String var in multiset.Variables)
            {
                this.AddVariable(var);
            }
            foreach (ISet s in multiset.Sets)
            {
                this.Add(s.Copy());
            }
        }

        /// <summary>
        /// Joins this Multiset to another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns></returns>
        public override BaseMultiset Join(BaseMultiset other)
        {
            //If the Other is the Identity Multiset the result is this Multiset
            if (other is IdentityMultiset) return this;
            //If the Other is the Null Multiset the result is the Null Multiset
            if (other is NullMultiset) return other;
            //If the Other is Empty then the result is the Null Multiset
            if (other.IsEmpty) return new NullMultiset();

            //Find the First Variable from this Multiset which is in both Multisets
            //If there is no Variable from this Multiset in the other Multiset then this
            //should be a Join operation instead of a LeftJoin
            List<String> joinVars = this._variables.Where(v => other.Variables.Contains(v)).ToList();
            if (joinVars.Count == 0) return this.Product(other);

            //Start building the Joined Set
            Multiset joinedSet = new Multiset();

            //This is the old Join algorithm which while correct is O(n^2) so scales terribly
            //foreach (ISet x in this.Sets)
            //{
            //    //For sets to be compatible for every joinable variable they must either have a null for the
            //    //variable in one of the sets or if they have values the values must be equal

            //    ////This first check is to try speed things up, it looks whether there are solutions that may match
            //    ////without needing to do a full table scan of the RHS results as the subsequent LINQ call will do
            //    ////if (!joinVars.All(v => x[v] == null || other.ContainsValue(v, x[v]) || other.ContainsValue(v, null))) continue;

            //    IEnumerable<ISet> ys = other.Sets.Where(s => joinVars.All(v => x[v] == null || s[v] == null || x[v].Equals(s[v])));
            //    //IEnumerable<ISet> ys = other.Sets.Where(s => s.IsCompatibleWith(x, joinVars));

            //    foreach (ISet y in ys)
            //    {
            //        joinedSet.Add(x.Join(y));
            //    }
            //}

            //This is the new Join algorithm which is also correct but is O(2n) so much faster and scalable
            //Downside is that it does require more memory than the old algorithm
            List<HashTable<INode, int>> values = new List<HashTable<INode, int>>();
            List<List<int>> nulls = new List<List<int>>();
            foreach (String var in joinVars)
            {
                values.Add(new HashTable<INode, int>(HashTableBias.Enumeration));
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
                        values[i].Add(value, x.ID);
                    }
                    else
                    {
                        nulls[i].Add(x.ID);
                    }
                    i++;
                }
            }

            //Then do a pass over the RHS and work out the intersections
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
                            possMatches = (possMatches == null ? values[i].GetValues(value).Concat(nulls[i]) : possMatches.Intersect(values[i].GetValues(value).Concat(nulls[i])));
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

                //Now do the actual joins for the current set
                //Note - We access the dictionary directly here because going through the this[int id] method
                //incurs a Contains() call each time and we know the IDs must exist because they came from
                //our dictionary originally!
                foreach (int poss in possMatches)
                {
                    if (this._sets[poss].IsCompatibleWith(y, joinVars))
                    {
                        joinedSet.Add(this._sets[poss].Join(y));
                    }
                }
            }

            return joinedSet;
        }

        /// <summary>
        /// Does a Left Join of this Multiset to another Multiset where the Join is predicated on the given Expression
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <param name="expr">Expression</param>
        /// <returns></returns>
        public override BaseMultiset LeftJoin(BaseMultiset other, ISparqlExpression expr)
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
            List<String> joinVars = this._variables.Where(v => other.Variables.Contains(v)).ToList();
            if (joinVars.Count == 0)
            {
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
            }
            else
            {
                //This is the old algorithm which is correct but has complexity O(n^2) so it scales terribly
                //foreach (ISet x in this.Sets)
                //{
                //    IEnumerable<ISet> ys = other.Sets.Where(s => joinVars.All(v => x[v] == null || s[v] == null || x[v].Equals(s[v])));
                //    //IEnumerable<ISet> ys = other.Sets.Where(s => s.IsCompatibleWith(x, joinVars));
                //    bool standalone = false;
                //    int i = 0;
                //    foreach (ISet y in ys)
                //    {
                //        i++;
                //        ISet z = x.Join(y);
                //        try
                //        {
                //            joinedSet.Add(z);
                //            if (!expr.Evaluate(subcontext, z.ID).AsSafeBoolean())
                //            {
                //                joinedSet.Remove(z.ID);
                //                standalone = true;
                //            }
                //        }
                //        catch
                //        {
                //            joinedSet.Remove(z.ID);
                //            standalone = true;
                //        }
                //    }
                //    if (standalone || i == 0) joinedSet.Add(x);
                //}

                //This is the new Join algorithm which is also correct but is O(2n) so much faster and scalable
                //Downside is that it does require more memory than the old algorithm
                List<HashTable<INode, int>> values = new List<HashTable<INode, int>>();
                List<List<int>> nulls = new List<List<int>>();
                foreach (String var in joinVars)
                {
                    values.Add(new HashTable<INode, int>(HashTableBias.Enumeration));
                    nulls.Add(new List<int>());
                }

                //First do a pass over the LHS Result to find all possible values for joined variables
                Dictionary<int, bool> matched = new Dictionary<int, bool>();
                HashSet<int> standalone = new HashSet<int>();
                foreach (ISet x in this.Sets)
                {
                    int i = 0;
                    foreach (String var in joinVars)
                    {
                        INode value = x[var];
                        if (value != null)
                        {
                            values[i].Add(value, x.ID);
                        }
                        else
                        {
                            nulls[i].Add(x.ID);
                        }
                        i++;
                    }
                    matched.Add(x.ID, false);
                }

                //Then do a pass over the RHS and work out the intersections
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
                                possMatches = (possMatches == null ? values[i].GetValues(value).Concat(nulls[i]) : possMatches.Intersect(values[i].GetValues(value).Concat(nulls[i])));
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

                    //Now do the actual joins for the current set
                    //Note - We access the dictionary directly here because going through the this[int id] method
                    //incurs a Contains() call each time and we know the IDs must exist because they came from
                    //our dictionary originally!
                    foreach (int poss in possMatches)
                    {
                        if (this._sets[poss].IsCompatibleWith(y, joinVars))
                        {
                            ISet z = this._sets[poss].Join(y);
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
                                    matched[poss] = true;
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

                //Finally add in unmatched sets from LHS
                foreach (int id in this.SetIDs)
                {
                    if (!matched[id] || standalone.Contains(id)) joinedSet.Add(this._sets[id].Copy());
                }
            }
            return joinedSet;
        }

        /// <summary>
        /// Does an Exists Join of this Multiset to another Multiset where the Join is predicated on the existence/non-existence of a joinable solution on the RHS
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <param name="mustExist">Whether a solution must exist in the Other Multiset for the join to be made</param>
        /// <returns></returns>
        public override BaseMultiset ExistsJoin(BaseMultiset other, bool mustExist)
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
            List<String> joinVars = this._variables.Where(v => other.Variables.Contains(v)).ToList();
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

            //This is the old algorithm which is correct but naive with worse case O(n^2)
            //foreach (ISet x in this.Sets)
            //{
            //    //New ExistsJoin() logic based on the improved Join() logic
            //    bool exists = other.Sets.Any(s => joinVars.All(v => x[v] == null || s[v] == null || x[v].Equals(s[v])));
            //    //bool exists = other.Sets.Any(s => s.IsCompatibleWith(x, joinVars));

            //    if (exists)
            //    {
            //        //If there are compatible sets and this is an EXIST then preserve the solution
            //        if (mustExist) joinedSet.Add(x);
            //    }
            //    else
            //    {
            //        //If there are no compatible sets and this is a NOT EXISTS then preserve the solution
            //        if (!mustExist) joinedSet.Add(x);
            //    }
            //}

            //This is the new algorithm which is also correct but is O(3n) so much faster and scalable
            //Downside is that it does require more memory than the old algorithm
            List<HashTable<INode, int>> values = new List<HashTable<INode, int>>();
            List<List<int>> nulls = new List<List<int>>();
            foreach (String var in joinVars)
            {
                values.Add(new HashTable<INode, int>(HashTableBias.Enumeration));
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
                        values[i].Add(value, x.ID);
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
                            possMatches = (possMatches == null ? values[i].GetValues(value).Concat(nulls[i]) : possMatches.Intersect(values[i].GetValues(value).Concat(nulls[i])));
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
                    if (this._sets[poss].IsCompatibleWith(y, joinVars))
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
        public override BaseMultiset MinusJoin(BaseMultiset other)
        {
            //If the other Multiset is the Identity/Null Multiset then minus-ing it doesn't alter this set
            if (other is IdentityMultiset) return this;
            if (other is NullMultiset) return this;
            //If the other Multiset is disjoint then minus-ing it also doesn't alter this set
            if (this.IsDisjointWith(other)) return this;

            //Find the Variables that are to be used for Joining
            List<String> joinVars = this._variables.Where(v => other.Variables.Contains(v)).ToList();
            if (joinVars.Count == 0) return this.Product(other);

            //Start building the Joined Set
            Multiset joinedSet = new Multiset();

            //This is the old algorithm which is correct but naive and has O(n^2) complexity
            //foreach (ISet x in this.Sets)
            //{
            //    //New Minus logic based on the improved Join() logic
            //    bool minus = other.Sets.Any(s => joinVars.All(v => x[v] == null || s[v] == null || x[v].Equals(s[v])));

            //    //If no compatible sets then this set is preserved
            //    if (!minus)
            //    {
            //        joinedSet.Add(x);
            //    }
            //}

            //This is the new algorithm which is also correct but is O(3n) so much faster and scalable
            //Downside is that it does require more memory than the old algorithm
            List<HashTable<INode, int>> values = new List<HashTable<INode, int>>();
            List<List<int>> nulls = new List<List<int>>();
            foreach (String var in joinVars)
            {
                values.Add(new HashTable<INode, int>(HashTableBias.Enumeration));
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
                        values[i].Add(value, x.ID);
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
                            possMatches = (possMatches == null ? values[i].GetValues(value).Concat(nulls[i]) : possMatches.Intersect(values[i].GetValues(value).Concat(nulls[i])));
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
                    if (this._sets[poss].IsCompatibleWith(y, joinVars))
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
        public override BaseMultiset Product(BaseMultiset other)
        {
            if (other is IdentityMultiset) return this;
            if (other is NullMultiset) return other;
            if (other.IsEmpty) return new NullMultiset();

            Multiset productSet = new Multiset();
            foreach (ISet x in this.Sets)
            {
                foreach (ISet y in other.Sets)
                {
                    productSet.Add(x.Join(y));
                }
            }
            return productSet;
        }

        /// <summary>
        /// Does a Union of this Multiset and another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns></returns>
        public override BaseMultiset Union(BaseMultiset other)
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
        /// Determines whether a given Value is present for a given Variable in any Set in this Multiset
        /// </summary>
        /// <param name="var">Variable</param>
        /// <param name="n">Value</param>
        /// <returns></returns>
        public override bool ContainsValue(String var, INode n)
        {
            if (this._variables.Contains(var))
            {
                //Create the Cache if necessary and reset it when necessary
                if (this._containsCache == null || this._cacheInvalid)
                {
                    this._containsCache = new Dictionary<string, HashSet<INode>>();
                    this._cacheInvalid = false;
                }
                if (!this._containsCache.ContainsKey(var))
                {
                    this._containsCache.Add(var, new HashSet<INode>(this._sets.Values.Select(s => s[var])));
                }
                //return this._sets.Values.Any(s => n.Equals(s[var]));
                return this._containsCache[var].Contains(n);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns whether a given Variable is present in any Set in this Multiset
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns></returns>
        public override bool ContainsVariable(string var)
        {
            return this._variables.Contains(var);
        }

        /// <summary>
        /// Determines whether this Multiset is disjoint with another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns></returns>
        public override bool IsDisjointWith(BaseMultiset other)
        {
            if (other is IdentityMultiset || other is NullMultiset) return false;
 
            return this._variables.All(v => !other.ContainsVariable(v));
        }

        /// <summary>
        /// Adds a Set to the Multiset
        /// </summary>
        /// <param name="s">Set</param>
        public override void Add(ISet s)
        {
            this._counter++;
            this._sets.Add(this._counter, s);
            s.ID = this._counter;
            foreach (String var in s.Variables)
            {
                if (!this._variables.Contains(var)) this._variables.Add(var);
            }
            this._cacheInvalid = true;
        }

        /// <summary>
        /// Adds a Variable to the list of Variables present in this Multiset
        /// </summary>
        /// <param name="variable">Variable</param>
        public override void AddVariable(string variable)
        {
            if (!this._variables.Contains(variable)) this._variables.Add(variable);
        }

        /// <summary>
        /// Removes a Set from the Multiset
        /// </summary>
        /// <param name="id">Set ID</param>
        public override void Remove(int id)
        {
            if (this._sets.ContainsKey(id))
            {
                this._sets.Remove(id);
                if (this._orderedIDs != null)
                {
                    this._orderedIDs.Remove(id);
                }
                this._cacheInvalid = true;
            }
        }

        /// <summary>
        /// Sorts a Set based on the given Comparer
        /// </summary>
        /// <param name="comparer">Comparer on Sets</param>
        public override void Sort(IComparer<ISet> comparer)
        {
            if (comparer != null)
            {
                this._orderedIDs = (from s in this._sets.Values.OrderBy(x => x, comparer)
                                    select s.ID).ToList();
            }
            else
            {
                this._orderedIDs = null;
            }
        }

        /// <summary>
        /// Trims the Multiset to remove Temporary Variables
        /// </summary>
        public override void Trim()
        {
            foreach (String var in this._variables)
            {
                if (var.StartsWith("_:"))
                {
                    foreach (ISet s in this._sets.Values)
                    {
                        s.Remove(var);
                    }
                }
            }
            this._variables.RemoveAll(v => v.StartsWith("_:"));
        }

        /// <summary>
        /// Trims the Multiset to remove the given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        public override void Trim(String variable)
        {
            if (this._variables.Remove(variable))
            {
                foreach (ISet s in this._sets.Values)
                {
                    s.Remove(variable);
                }
            }
        }

        /// <summary>
        /// Gets whether the Multiset is empty
        /// </summary>
        public override bool IsEmpty
        {
            get 
            {
                return (this._sets.Count == 0);
            }
        }

        /// <summary>
        /// Gets the number of Sets in the Multiset
        /// </summary>
        public override int Count
        {
            get
            {
                return this._sets.Count;
            }
        }

        /// <summary>
        /// Gets the Variables in the Multiset
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get 
            {
                return (from var in this._variables
                        select var);
            }
        }

        /// <summary>
        /// Gets the Sets in the Multiset
        /// </summary>
        public override IEnumerable<ISet> Sets
        {
            get 
            {
                if (this._orderedIDs == null)
                {
                    return (from s in this._sets.Values
                            select s);
                }
                else
                {
                    return (from id in this._orderedIDs
                            select this._sets[id]);
                }
            }
        }

        /// <summary>
        /// Gets the IDs of Sets in the Multiset
        /// </summary>
        public override IEnumerable<int> SetIDs
        {
            get 
            {
                if (this._orderedIDs == null)
                {
                    return (from id in this._sets.Keys
                            select id);
                }
                else
                {
                    return this._orderedIDs;
                }
            }
        }

        /// <summary>
        /// Gets a Set from the Multiset
        /// </summary>
        /// <param name="id">Set ID</param>
        /// <returns></returns>
        public override ISet this[int id]
        {
            get 
            {
                if (this._sets.ContainsKey(id))
                {
                    return this._sets[id];
                }
                else
                {
                    throw new RdfQueryException("A Set with ID " + id + " does not exist in this Multiset");
                }
            }
        }
    }

    internal class SingletonMultiset : Multiset
    {
        public SingletonMultiset()
            : base()
        {
            this.Add(new Set());
        }

        public SingletonMultiset(IEnumerable<String> vars)
            : base(vars)
        {
            this.Add(new Set());
        }
    }
}
