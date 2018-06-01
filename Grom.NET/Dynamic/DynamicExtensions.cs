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
            if (!(node is IUriNode || node is IBlankNode))
            {
                throw new Exception("Only URI and blank nodes.");
            }

            return new DynamicNode(node, baseUri, collapseSingularArrays);
        }
    }
}
