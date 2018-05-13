namespace Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using VDS.RDF;

    internal static class DynamicHelper
    {
        internal static IUriNode ConvertIndex(object index, IGraph graph, Uri baseUri)
        {
            if (index is DynamicNode indexWrapper)
            {
                index = indexWrapper.graphNode;
            }

            if (!(index is IUriNode indexNode))
            {
                if (!(index is Uri indexUri))
                {
                    if (!(index is string indexString))
                    {
                        throw new ArgumentException("Only IUriNode, Uri or string indices", "index");
                    }

                    if (!DynamicHelper.TryResolveQName(indexString, graph, out indexUri))
                    {
                        if (!Uri.TryCreate(indexString, UriKind.RelativeOrAbsolute, out indexUri))
                        {
                            throw new FormatException("Illegal Uri.");
                        }
                    }
                }

                if (!indexUri.IsAbsoluteUri)
                {
                    if (baseUri == null)
                    {
                        throw new InvalidOperationException("Can't use relative uri index without baseUri.");
                    }

                    if (baseUri.AbsoluteUri.EndsWith("#"))
                    {
                        var builder = new UriBuilder(baseUri) { Fragment = indexUri.ToString() };

                        indexUri = builder.Uri;
                    }
                    else
                    {
                        indexUri = new Uri(baseUri, indexUri);
                    }
                }

                indexNode = graph.CreateUriNode(indexUri);
            }

            return indexNode;
        }

        internal static IEnumerable<string> GetDynamicMemberNames(IEnumerable<IUriNode> nodes, Uri baseUri)
        {
            return nodes.Select(node => GetPropertyName(node, baseUri));
        }

        private static string GetPropertyName(IUriNode node, Uri baseUri)
        {
            // TODO: Consider qnames?

            var nodeUri = node.Uri;

            if (node.Graph.NamespaceMap.ReduceToQName(nodeUri.AbsoluteUri, out string qname))
            {
                return qname;
            }

            if (baseUri == null)
            {
                return nodeUri.AbsoluteUri;
            }

            if (baseUri.AbsoluteUri.EndsWith("#"))
            {
                return nodeUri.Fragment.TrimStart('#');
            }

            return baseUri.MakeRelativeUri(nodeUri).ToString();
        }

        private static bool TryResolveQName(string index, IGraph graph, out Uri indexUri)
        {

            if (!Regex.IsMatch(index, @"^\w*:\w+$"))
            {
                indexUri = null;
                return false;
            }

            indexUri = graph.ResolveQName(index);
            return true;
        }
    }
}
