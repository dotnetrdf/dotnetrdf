namespace Dynamic
{
    using System;
    using VDS.RDF;

    public static class DynamicExtensions
    {
        public static dynamic AsDynamic(this IGraph graph, Uri subjectBaseUri = null, Uri predicateBaseUri = null, bool collapseSingularArrays = false)
        {
            return new DynamicGraph(graph, subjectBaseUri, predicateBaseUri, collapseSingularArrays);
        }

        public static dynamic AsDynamic(this INode node, Uri baseUri = null, bool collapseSingularArrays = false)
        {
            switch (node)
            {
                case IUriNode uriNode:
                    return new DynamicUriNode(uriNode, baseUri, collapseSingularArrays);

                case IBlankNode blankNode:
                    return new DynamicBlankNode(blankNode, baseUri, collapseSingularArrays);

                default:
                    throw new Exception("Only URI and blank nodes.");
            }
        }
    }
}
