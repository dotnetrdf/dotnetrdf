using NUnit.Framework;
using VDS.RDF.Graphs;

namespace VDS.RDF.Query.Processors
{
    [TestFixture]
    public class GraphQueryProcessorTests
        : AbstractQueryProcessorTests
    {
        protected override IQueryProcessor CreateProcessor(IGraph g)
        {
            return new GraphQueryProcesor(g);
        }
    }
}
