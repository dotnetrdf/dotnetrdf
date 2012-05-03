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
            int id;
#if NET40 && !SILVERLIGHT
            if (Options.UsePLinqEvaluation)
            {
                lock (this._sets)
                {
                    this._counter++;
                    id = this._counter;
                    this._sets.Add(id, s);
                    s.ID = id;
                }
            }
            else
            {
#endif
                id = this._counter++;
                this._sets.Add(id, s);
                s.ID = id;
#if NET40 && !SILVERLIGHT
            }
#endif
            
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
#if NET40 && !SILVERLIGHT
            lock (this._sets)
            {
#endif
                if (this._sets.ContainsKey(id))
                {
                    this._sets.Remove(id);
                    if (this._orderedIDs != null)
                    {
                        this._orderedIDs.Remove(id);
                    }
                    this._cacheInvalid = true;
                }
#if NET40 && !SILVERLIGHT
            }
#endif
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
            if (variable == null) return;
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
                ISet s;
                if (this._sets.TryGetValue(id, out s))
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
            this.Add(new Set());
        }

        public SingletonMultiset(IEnumerable<String> vars)
            : base(vars)
        {
            this.Add(new Set());
        }
    }
}
