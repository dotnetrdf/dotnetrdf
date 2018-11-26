namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;

    public static class DynamicExtensions
    {
        public static dynamic AsDynamic(this IGraph graph, Uri subjectBaseUri = null, Uri predicateBaseUri = null)
        {
            return new DynamicGraph(graph, subjectBaseUri, predicateBaseUri);
        }

        public static dynamic AsDynamic(this INode node, Uri baseUri = null)
        {
            return new DynamicNode(node, baseUri);
        }

        public static IRdfCollection AsRdfCollection(this IEnumerable original)
        {
            return new RdfCollection(original);
        }

    }
}
