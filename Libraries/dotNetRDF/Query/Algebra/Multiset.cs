/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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

using System;
using System.Collections.Generic;
using System.Linq;

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
                _variables.Add(var);
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
                AddVariable(var);
            }
            foreach (SparqlResult r in results.Results)
            {
                Add(new Set(r));
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
                AddVariable(var);
            }
            foreach (ISet s in multiset.Sets)
            {
                Add(s.Copy());
            }
        }

        /// <summary>
        /// Determines whether a given Value is present for a given Variable in any Set in this Multiset
        /// </summary>
        /// <param name="var">Variable</param>
        /// <param name="n">Value</param>
        /// <returns></returns>
        public override bool ContainsValue(String var, INode n)
        {
            if (_variables.Contains(var))
            {
                // Create the Cache if necessary and reset it when necessary
                if (_containsCache == null || _cacheInvalid)
                {
                    _containsCache = new Dictionary<string, HashSet<INode>>();
                    _cacheInvalid = false;
                }
                if (!_containsCache.ContainsKey(var))
                {
                    _containsCache.Add(var, new HashSet<INode>(_sets.Values.Select(s => s[var])));
                }
                // return this._sets.Values.Any(s => n.Equals(s[var]));
                return _containsCache[var].Contains(n);
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
            return _variables.Contains(var);
        }

        /// <summary>
        /// Determines whether this Multiset is disjoint with another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns></returns>
        public override bool IsDisjointWith(BaseMultiset other)
        {
            if (other is IdentityMultiset || other is NullMultiset) return false;
 
            return Variables.All(v => !other.ContainsVariable(v));
        }

        /// <summary>
        /// Adds a Set to the Multiset
        /// </summary>
        /// <param name="s">Set</param>
        public override void Add(ISet s)
        {
            int id;
#if NET40
            if (Options.UsePLinqEvaluation)
            {
                lock (this._sets)
                {
                    this._counter++;
                    id = this._counter;
                    this._sets.Add(id, s);
                    s.ID = id;
                }
                lock (this._variables)
                {
                    foreach (String var in s.Variables)
                    {
                        if (!this._variables.Contains(var)) this._variables.Add(var);
                    }
                }
            }
            else
            {
                this._counter++;
                id = this._counter;
                this._sets.Add(id, s);
                s.ID = id;

                foreach (String var in s.Variables)
                {
                    if (!this._variables.Contains(var)) this._variables.Add(var);
                }
            }
#else
            _counter++;
            id = _counter;
            _sets.Add(id, s);
            s.ID = id;

            foreach (string var in s.Variables)
            {
                if (!_variables.Contains(var)) _variables.Add(var);
            }
#endif
            _cacheInvalid = true;
        }

        /// <summary>
        /// Adds a Variable to the list of Variables present in this Multiset
        /// </summary>
        /// <param name="variable">Variable</param>
        public override void AddVariable(string variable)
        {
            if (!_variables.Contains(variable)) _variables.Add(variable);
        }

        /// <summary>
        /// Sets the variable ordering for the multiset
        /// </summary>
        /// <param name="variables">Variable Ordering</param>
        public override void SetVariableOrder(IEnumerable<string> variables)
        {
            // Validate that the ordering is applicable
            if (variables.Count() < _variables.Count) throw new RdfQueryException("Cannot set a variable ordering that contains less variables then are currently specified");
            foreach (String var in _variables)
            {
                if (!variables.Contains(var)) throw new RdfQueryException("Cannot set a variable ordering that omits the variable ?" + var + " currently present in the multiset, use Trim(\"" + var + "\") first to remove this variable");
            }
            // Apply ordering
            _variables = new List<string>(variables);
        }

        /// <summary>
        /// Removes a Set from the Multiset
        /// </summary>
        /// <param name="id">Set ID</param>
        public override void Remove(int id)
        {
#if NET40
            lock (this._sets)
            {
#endif
                if (_sets.ContainsKey(id))
                {
                    _sets.Remove(id);
                    if (_orderedIDs != null)
                    {
                        _orderedIDs.Remove(id);
                    }
                    _cacheInvalid = true;
                }
#if NET40
            }
#endif
        }

        /// <summary>
        /// Trims the Multiset to remove Temporary Variables
        /// </summary>
        public override void Trim()
        {
            foreach (String var in _variables)
            {
                if (var.StartsWith("_:"))
                {
                    foreach (ISet s in _sets.Values)
                    {
                        s.Remove(var);
                    }
                }
            }
            _variables.RemoveAll(v => v.StartsWith("_:"));
        }

        /// <summary>
        /// Trims the Multiset to remove the given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        public override void Trim(String variable)
        {
            if (variable == null) return;
            if (_variables.Remove(variable))
            {
                foreach (ISet s in _sets.Values)
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
                return (_sets.Count == 0);
            }
        }

        /// <summary>
        /// Gets the number of Sets in the Multiset
        /// </summary>
        public override int Count
        {
            get
            {
                return _sets.Count;
            }
        }

        /// <summary>
        /// Gets the Variables in the Multiset
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get 
            {
                return (from var in _variables
                        where var != null
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
                if (_orderedIDs == null)
                {
                    return (from s in _sets.Values
                            select s);
                }
                else
                {
                    return (from id in _orderedIDs
                            select _sets[id]);
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
                if (_orderedIDs == null)
                {
                    return (from id in _sets.Keys
                            select id);
                }
                else
                {
                    return _orderedIDs;
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
                ISet s;
                if (_sets.TryGetValue(id, out s))
                {
                    return s;
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
            Add(new Set());
        }

        public SingletonMultiset(IEnumerable<String> vars)
            : base(vars)
        {
            Add(new Set());
        }
    }
}
