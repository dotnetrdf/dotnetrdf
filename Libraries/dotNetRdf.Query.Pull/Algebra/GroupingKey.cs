/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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

using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class GroupingKey : IEquatable<GroupingKey>
{
    private readonly IList<INode?> _bindingsList;
    private readonly int _hashCode;
    public GroupingKey(IEnumerable<INode?> bindings)
    {
        _bindingsList = bindings.ToList();
        _hashCode = CombineHashCodes(_bindingsList);
    }

    private int CombineHashCodes(IEnumerable<INode?> bindings)
    {
        return bindings.Aggregate(17, (current, o) => (31 * current) + o?.GetHashCode() ?? 0);
    }

    public ISet ToSet(IList<string?> varNames)
    {
        ISet ret = new Set();
        for (var i = 0; i < varNames.Count; i++)
        {
            if (varNames[i] != null)
            {
                ret.Add(varNames[i], _bindingsList[i]);
            }
        }

        return ret;
    }

    public override int GetHashCode()
    {
        return _hashCode;
    }

    public override bool Equals(object? obj)
    {
        return obj is GroupingKey other && Equals(other);
    }

    public bool Equals(GroupingKey other)
    {
        if (other._bindingsList.Count != _bindingsList.Count) return false;
        return _bindingsList.SequenceEqual(other._bindingsList, new FastNodeComparer());
    }

    public override string ToString()
    {
        return string.Join(", ", _bindingsList);
    }
}