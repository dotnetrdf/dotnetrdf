namespace Grom.Tests
{
    using VDS.RDF;

    internal static class Helper
    {
        internal static IGraph Load(string rdf)
        {
            var graph = new Graph();

            graph.LoadFromString(rdf);

            return graph;        }
    }
}
