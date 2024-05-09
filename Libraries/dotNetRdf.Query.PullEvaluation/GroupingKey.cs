using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

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