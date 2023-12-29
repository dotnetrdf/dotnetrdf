using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation.Tests;

public class EnumeratorTestBase
{
    protected readonly INodeFactory _nodeFactory = new NodeFactory();

    protected async Task<List<ISet>> ReadAllAsync(IAsyncEnumerable<ISet> enumerator)
    {
        var ret = new List<ISet>();
        await foreach(ISet s in enumerator) { ret.Add(s);}
        return ret;
    }
}