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

        public override void AddVariable(string variable)
        {
            if (this._variables.Count == 0) this._variables.Add(new HashSet<string>());
            this._variables[0].Add(variable);
        }

        public override void Remove(int id)
        {
            int p = id / this._partitionSize;
            this._partitions[p].Remove(id);
            this._cacheInvalid = true;
        }

        public override bool IsEmpty
        {
            get 
            {
                return this._partitions.All(p => p.Count == 0);
            }
        }

        public override IEnumerable<string> Variables
        {
            get
            {
                return (from vs in this._variables
                        from v in vs
                        select v).Distinct();
            }
        }

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