using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Primary;
using Xunit;

namespace VDS.RDF.Query.Algebra;

public class LeftJoinTests
{
    private NodeFactory _factory = new NodeFactory();

    [Fact]
    public void SparqlAlgebraLeftJoinMultiVariable5()
    {
        var x1 = new Set();
        x1.Add("a", _factory.CreateUriNode(UriFactory.Root.Create("http://x/x1")));
        x1.Add("b", _factory.CreateLiteralNode("2"));
        var x2 = new Set();
        x2.Add("a", _factory.CreateUriNode(UriFactory.Root.Create("http://x/x2")));
            
        var y1 = new Set();
        y1.Add("a", _factory.CreateUriNode(UriFactory.Root.Create("http://x/x2")));
        y1.Add("b", _factory.CreateLiteralNode("1"));
        var y2 = new Set();
        y2.Add("a", _factory.CreateUriNode(UriFactory.Root.Create("http://x/x3")));
        y2.Add("b", _factory.CreateLiteralNode("1"));
        var y3 = new Set();
        y3.Add("a", _factory.CreateUriNode(UriFactory.Root.Create("http://x/x4")));
        y3.Add("b", _factory.CreateLiteralNode("1"));

        var lhs = new Multiset();
        lhs.Add(x1);
        lhs.Add(x2);
        var rhs = new Multiset();
        rhs.Add(y2);
        rhs.Add(y3);
        // The matching set in the right hand side should have an index higher than the count of the left hand side to 
        // test the regression of the original error fixed by PR #677:
        rhs.Add(y1);
            
        var mockContext = new SparqlEvaluationContext(null, new LeviathanQueryOptions());
        var expressionProcessor = new LeviathanExpressionProcessor(mockContext.Options,
            mockContext.Processor as LeviathanQueryProcessor);
            
        BaseMultiset result = lhs.LeftJoin(rhs, new ConstantTerm(new BooleanNode(true)), mockContext, expressionProcessor);

        Assert.True(result.Sets.Where(s => s["a"].Equals(_factory.CreateUriNode(UriFactory.Root.Create("http://x/x2"))))
            .Select(s => s["b"])
            .All(n => n is not null && n.Equals(_factory.CreateLiteralNode("1"))));
    }
}