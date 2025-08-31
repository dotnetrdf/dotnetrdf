using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Pull;

namespace dotNetRdf.Query.Pull.Tests;

public class EnumeratorTestBase
{
    protected readonly INodeFactory _nodeFactory = new NodeFactory();
    internal readonly PullEvaluationContext _context = new(new TripleStore());
}