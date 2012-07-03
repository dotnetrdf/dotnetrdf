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

#if NET40 && !SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Implementation of a multiset which is suitable for multiple threads to write to in parallel, useful for parallelizing certain operations
    /// </summary>
    public class PartitionedMultiset
        : BaseMultiset
    {
        private int _partitionSize, _numPartitions, _counter = -1;
        private List<Dictionary<int, ISet>> _partitions;
        private List<HashSet<String>> _variables;

        private Dictionary<String, HashSet<INode>> _containsCache;
        private bool _cacheInvalid = true;

        /// <summary>
        /// Creates a new Partionted Multiset
        /// </summary>
        /// <param name="numPartitions">Number of partitions</param>
        /// <param name="partitionSize">Partition Size</param>
        public PartitionedMultiset(int numPartitions, int partitionSize)
        {
            this._numPartitions = numPartitions;
            this._partitionSize = partitionSize;
            this._partitions = new List<Dictionary<int, ISet>>(this._numPartitions);
            this._variables = new List<HashSet<string>>(this._numPartitions);
            this._counter -= this._partitionSize;
        }

        /// <summary>
        /// Gets the next Base ID to be used
        /// </summary>
        /// <returns></returns>
        public int GetNextBaseID()
        {
            int baseID;
            lock (this._partitions)
            {
                this._partitions.Add(new Dictionary<int, ISet>());
                //We don't always need to create a new variable partition as in some cases there may be one already (if AddVariable() got called first)
                if (this._variables.Count < this._partitions.Count) this._variables.Add(new HashSet<string>());
                this._counter += this._partitionSize;
                baseID = this._counter;
            }
            return baseID;
        }

        /// <summary>
        /// Determines whether a given Value is present for a given Variable in any Set in this Multiset
        /// </summary>
        /// <param name="var">Variable</param>
        /// <param name="n">Value</param>
        /// <returns></returns>
        public override bool ContainsValue(String var, INode n)
        {
            if (this.ContainsVariable(var))
            {
                //Create the Cache if necessary and reset it when necessary
                if (this._containsCache == null || this._cacheInvalid)
                {
                    this._containsCache = new Dictionary<string, HashSet<INode>>();
                    this._cacheInvalid = false;
                }
                if (!this._containsCache.ContainsKey(var))
                {
                    this._containsCache.Add(var, new HashSet<INode>(this.Sets.Select(s => s[var])));
                }
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
            return this._variables.Any(v => v.Contains(var));
        }

        /// <summary>
        /// Determines whether this Multiset is disjoint with another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns></returns>
        public override bool IsDisjointWith(BaseMultiset other)
        {
            if (other is IdentityMultiset || other is NullMultiset) return false;

            return this.Variables.All(v => !other.ContainsVariable(v));
        }

        /// <summary>
        /// Adds a Set to the multiset
        /// </summary>
        /// <param name="s">Set</param>
        /// <remarks>
        /// Assumes the caller has set the ID of the set appropriately and will use this to determine which partition to add to
        /// </remarks>
        public override void Add(ISet s)
        {
            //Compute which partition based on the ID
            int p = s.ID / this._partitionSize;
            this._partitions[p].Add(s.ID, s);
            this._cacheInvalid = true;

            foreach (String var in s.Variables)
            {
                this._variables[p].Add(var);
            }
        }

        /// <summary>
        /// Adds a Variable to the multiset
        /// </summary>
        /// <param name="variable">Variable</param>
        public override void AddVariable(string variable)
        {
            if (this._variables.Count == 0) this._variables.Add(new HashSet<string>());
            this._variables[0].Add(variable);
        }

        /// <summary>
        /// Removes a Set from the multiset
        /// </summary>
        /// <param name="id">Set ID</param>
        public override void Remove(int id)
        {
            int p = id / this._partitionSize;
            this._partitions[p].Remove(id);
            this._cacheInvalid = true;
        }

        /// <summary>
        /// Gets whether the multiset is empty
        /// </summary>
        public override bool IsEmpty
        {
            get 
            {
                return this._partitions.All(p => p.Count == 0);
            }
        }

        /// <summary>
        /// Gets the number of sets in the multiset
        /// </summary>
        public override int Count
        {
            get
            {
                return this._partitions.Sum(p => p.Count);
            }
        }

        /// <summary>
        /// Gets the variables in the multiset
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get
            {
                return (from vs in this._variables
                        from v in vs
                        where v != null
                        select v).Distinct();
            }
        }

        /// <summary>
        /// Gets the sets in the multiset
        /// </summary>
        public override IEnumerable<ISet> Sets
        {
            get 
            {
                if (this._orderedIDs == null)
                {
                    return (from p in this._partitions
                            from s in p.Values
                            select s);
                }
                else
                {
                    return (from id in this._orderedIDs
                            select this[id]);
                }
            }
        }

        /// <summary>
        /// Gets the Set IDs in the mutliset
        /// </summary>
        public override IEnumerable<int> SetIDs
        {
            get 
            {
                if (this._orderedIDs == null)
                {
                    return (from p in this._partitions
                            from id in p.Keys
                            select id);
                }
                else
                {
                    return this._orderedIDs;
                }
            }
        }

        /// <summary>
        /// Gets a Set from the multiset
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ISet this[int id]
        {
            get
            {
                int p = id / this._partitionSize;
                if (p < this._partitions.Count)
                {
                    ISet s;
                    if (this._partitions[p].TryGetValue(id, out s))
                    {
                        return s;
                    }
                    else
                    {
                        throw new RdfQueryException("A Set with the given ID " + id + " does not exist in this Multiset");
                    }
                }
                else
                {
                    throw new RdfQueryException("A Set with the given ID " + id + " does not exist in this Multiset");
                }
            }
        }

        /// <summary>
        /// Removes temporary variables from all sets the multiset
        /// </summary>
        public override void Trim()
        {
            List<String> trimVars = this.Variables.Where(v => v.StartsWith("_:")).ToList();
            foreach (String var in trimVars)
            {
                foreach (HashSet<String> vs in this._variables)
                {
                    vs.Remove(var);
                }
                if (this._containsCache != null)
                {
                    this._containsCache.Remove(var);
                }
            }

            if (Options.UsePLinqEvaluation)
            {
                this.Sets.AsParallel().ForAll(s =>
                    {
                        foreach (String var in trimVars)
                        {
                            s.Remove(var);
                        }
                    });
            }
            else
            {
                foreach (ISet s in this.Sets)
                {
                    foreach (String var in trimVars)
                    {
                        s.Remove(var);
                    }
                }
            }
        }

        /// <summary>
        /// Removes a specific variable from all sets in the multiset
        /// </summary>
        /// <param name="variable">Variable</param>
        public override void Trim(string variable)
        {
            if (variable == null) return;
            foreach (HashSet<String> vs in this._variables)
            {
                vs.Remove(variable);
            }
            if (Options.UsePLinqEvaluation)
            {
                this.Sets.AsParallel().ForAll(s => s.Remove(variable));
            }
            else
            {
                foreach (ISet s in this.Sets)
                {
                    s.Remove(variable);
                }
            }
            if (this._containsCache != null)
            {
                this._containsCache.Remove(variable);
            }
        }
    }
}

#endif