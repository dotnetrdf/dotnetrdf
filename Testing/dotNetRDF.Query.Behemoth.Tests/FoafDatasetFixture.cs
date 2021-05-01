using System.Reflection;
using VDS.RDF;
using VDS.RDF.Query.Datasets;
using Graph = VDS.RDF.Graph;

namespace dotNetRDF.Query.Behemoth.Tests
{
    public class FoafDatasetFixture
    {
        public BehemothEvaluationContext EvaluationContext { get; }

        public FoafDatasetFixture()
        {
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var g = new Graph();
            g.LoadFromFile("resources\\foaf.ttl");
            var dataset = new InMemoryDataset(g);
            EvaluationContext = new BehemothEvaluationContext(dataset);
        }
    }
}
