using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Pull;

namespace dotNetRdf.Query.Pull.Tests;

public class EnumeratorTestBase
{
    protected readonly INodeFactory NodeFactory = new NodeFactory();
    protected readonly PullEvaluationContext Context = new(new TripleStore());

    protected async Task<List<ISet>> GetBatchResultsAsync(IAsyncEnumerable<IEnumerable<ISet>> enumeration)
    {
        var results = new List<ISet>();
        await foreach (IEnumerable<ISet> batch in enumeration)
        {
            results.AddRange(batch);
        }

        return results;
    }
}