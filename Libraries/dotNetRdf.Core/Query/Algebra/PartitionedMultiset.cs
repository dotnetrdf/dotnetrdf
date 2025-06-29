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

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Implementation of a multiset which is suitable for multiple threads to write to in parallel, useful for parallelizing certain operations.
/// </summary>
public class PartitionedMultiset
    : BaseMultiset
{
    private int _partitionSize, _numPartitions, _counter = -1;
    private List<Dictionary<int, ISet>> _partitions;
    private List<HashSet<string>> _variables;
    private List<string> _orderedVariables;

    private Dictionary<string, HashSet<INode>> _containsCache;
    private bool _cacheInvalid = true;

    /// <summary>
    /// Creates a new Partionted Multiset.
    /// </summary>
    /// <param name="numPartitions">Number of partitions.</param>
    /// <param name="partitionSize">Partition Size.</param>
    public PartitionedMultiset(int numPartitions, int partitionSize)
    {
        _numPartitions = numPartitions;
        _partitionSize = partitionSize;
        _partitions = new List<Dictionary<int, ISet>>(_numPartitions);
        _variables = new List<HashSet<string>>(_numPartitions);
        _counter -= _partitionSize;
    }

    /// <summary>
    /// Gets the next Base ID to be used.
    /// </summary>
    /// <returns></returns>
    public int GetNextBaseID()
    {
        int baseID;
        lock (_partitions)
        {
            _partitions.Add(new Dictionary<int, ISet>());
            // We don't always need to create a new variable partition as in some cases there may be one already (if AddVariable() got called first)
            if (_variables.Count < _partitions.Count) _variables.Add(new HashSet<string>());
            _counter += _partitionSize;
            baseID = _counter;
        }
        return baseID;
    }

    /// <summary>
    /// Does a Union of this Multiset and another Multiset.
    /// </summary>
    /// <param name="other">Other Multiset.</param>
    /// <returns></returns>
    public override BaseMultiset Union(BaseMultiset other)
    {
        if (other is IdentityMultiset) return this;
        if (other is NullMultiset) return this;
        if (other.IsEmpty) return this;

        var m = new Multiset();
        foreach (ISet s in Sets)
        {
            m.Add(s.Copy());
        }
        foreach (ISet s in other.Sets)
        {
            m.Add(s.Copy());
        }
        return m;
    }

    /// <summary>
    /// Determines whether a given Value is present for a given Variable in any Set in this Multiset.
    /// </summary>
    /// <param name="var">Variable.</param>
    /// <param name="n">Value.</param>
    /// <returns></returns>
    public override bool ContainsValue(string var, INode n)
    {
        if (ContainsVariable(var))
        {
            // Create the Cache if necessary and reset it when necessary
            if (_containsCache == null || _cacheInvalid)
            {
                _containsCache = new Dictionary<string, HashSet<INode>>();
                _cacheInvalid = false;
            }
            if (!_containsCache.ContainsKey(var))
            {
                _containsCache.Add(var, new HashSet<INode>(Sets.Select(s => s[var])));
            }
            return _containsCache[var].Contains(n);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Returns whether a given Variable is present in any Set in this Multiset.
    /// </summary>
    /// <param name="var">Variable.</param>
    /// <returns></returns>
    public override bool ContainsVariable(string var)
    {
        return _variables.Any(v => v.Contains(var));
    }

    /// <summary>
    /// Returns whether the given Variables are present in any Set in this Multiset.
    /// </summary>
    /// <param name="vars">Variables.</param>
    /// <returns></returns>
    public override bool ContainsVariables(IEnumerable<string> vars)
    {
        return vars.All(var => _variables.Any(v => v.Contains(var)));
    }

    /// <summary>
    /// Determines whether this Multiset is disjoint with another Multiset.
    /// </summary>
    /// <param name="other">Other Multiset.</param>
    /// <returns></returns>
    public override bool IsDisjointWith(BaseMultiset other)
    {
        if (other is IdentityMultiset || other is NullMultiset) return false;

        return Variables.All(v => !other.ContainsVariable(v));
    }

    /// <summary>
    /// Adds a Set to the multiset.
    /// </summary>
    /// <param name="s">Set.</param>
    /// <remarks>
    /// Assumes the caller has set the ID of the set appropriately and will use this to determine which partition to add to.
    /// </remarks>
    public override void Add(ISet s)
    {
        // Compute which partition based on the ID
        var p = s.ID / _partitionSize;
        _partitions[p].Add(s.ID, s);
        _cacheInvalid = true;

        foreach (var var in s.Variables)
        {
            _variables[p].Add(var);
        }
    }

    /// <summary>
    /// Adds a Variable to the multiset.
    /// </summary>
    /// <param name="variable">Variable.</param>
    public override void AddVariable(string variable)
    {
        if (_variables.Count == 0) _variables.Add(new HashSet<string>());
        _variables[0].Add(variable);
        if (_orderedVariables == null) _orderedVariables = new List<string>(Variables);
        if (!_orderedVariables.Contains(variable)) _orderedVariables.Add(variable);
    }

    /// <summary>
    /// Sets the variable ordering for the multiset.
    /// </summary>
    /// <param name="variables">Variable Ordering.</param>
    public override void SetVariableOrder(IEnumerable<string> variables)
    {
        // Validate that the ordering is applicable
        var vars = new HashSet<string>();
        foreach (HashSet<string> varList in _variables)
        {
            foreach (var var in varList)
            {
                vars.Add(var);
            }
        }
        if (variables.Count() < vars.Count) throw new RdfQueryException("Cannot set a variable ordering that contains less variables then are currently specified");
        foreach (var var in vars)
        {
            if (!variables.Contains(var)) throw new RdfQueryException("Cannot set a variable ordering that omits the variable ?" + var + " currently present in the multiset, use Trim(\"" + var + "\") first to remove this variable");
        }

        // Apply ordering
        _orderedVariables = new List<string>(vars);
    }

    /// <summary>
    /// Removes a Set from the multiset.
    /// </summary>
    /// <param name="id">Set ID.</param>
    public override void Remove(int id)
    {
        var p = id / _partitionSize;
        _partitions[p].Remove(id);
        if (_orderedIDs != null)
        {
            _orderedIDs.Remove(id);
        }
        _cacheInvalid = true;
    }

    /// <summary>
    /// Gets whether the multiset is empty.
    /// </summary>
    public override bool IsEmpty
    {
        get 
        {
            return _partitions.All(p => p.Count == 0);
        }
    }

    /// <summary>
    /// Gets the number of sets in the multiset.
    /// </summary>
    public override int Count
    {
        get
        {
            return _partitions.Sum(p => p.Count);
        }
    }

    /// <summary>
    /// Gets the variables in the multiset.
    /// </summary>
    public override IEnumerable<string> Variables
    {
        get
        {
            if (_orderedVariables != null)
            {
                return _orderedVariables;
            }
            else
            {
                return (from vs in _variables
                        from v in vs
                        where v != null
                        select v).Distinct();
            }
        }
    }

    /// <summary>
    /// Gets the sets in the multiset.
    /// </summary>
    public override IEnumerable<ISet> Sets
    {
        get 
        {
            if (_orderedIDs == null)
            {
                return (from p in _partitions
                        from s in p.Values
                        select s);
            }
            else
            {
                return (from id in _orderedIDs
                        select this[id]);
            }
        }
    }

    /// <summary>
    /// Gets the Set IDs in the mutliset.
    /// </summary>
    public override IEnumerable<int> SetIDs
    {
        get 
        {
            if (_orderedIDs == null)
            {
                return (from p in _partitions
                        from id in p.Keys
                        select id);
            }
            else
            {
                return _orderedIDs;
            }
        }
    }

    /// <summary>
    /// Gets a Set from the multiset.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override ISet this[int id]
    {
        get
        {
            var p = id / _partitionSize;
            if (p < _partitions.Count)
            {
                ISet s;
                if (_partitions[p].TryGetValue(id, out s))
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
    /// Removes temporary variables from all sets the multiset.
    /// </summary>
    public override void Trim()
    {
        var trimVars = Variables.Where(v => v.StartsWith("_:")).ToList();
        foreach (var var in trimVars)
        {
            foreach (HashSet<string> vs in _variables)
            {
                vs.Remove(var);
            }
            if (_containsCache != null)
            {
                _containsCache.Remove(var);
            }
        }

        if (UsePLinqEvaluation)
        {
            Sets.AsParallel().ForAll(s =>
                {
                    foreach (var var in trimVars)
                    {
                        s.Remove(var);
                    }
                });
        }
        else
        {
            foreach (ISet s in Sets)
            {
                foreach (var var in trimVars)
                {
                    s.Remove(var);
                }
            }
        }
    }

    /// <summary>
    /// Removes a specific variable from all sets in the multiset.
    /// </summary>
    /// <param name="variable">Variable.</param>
    public override void Trim(string variable)
    {
        if (variable == null) return;
        foreach (HashSet<string> vs in _variables)
        {
            vs.Remove(variable);
        }
        if (UsePLinqEvaluation)
        {
            Sets.AsParallel().ForAll(s => s.Remove(variable));
        }
        else
        {
            foreach (ISet s in Sets)
            {
                s.Remove(variable);
            }
        }
        if (_containsCache != null)
        {
            _containsCache.Remove(variable);
        }
    }
}
