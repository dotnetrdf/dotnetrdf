namespace Grom
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Parsing;

    internal static class Helper
    {
        internal static IUriNode ConvertNode(object predicate, IGraph graph, Uri baseUri)
        {
            // TODO: Consider graph.BaseUri?

            if (!(predicate is IUriNode predicateNode))
            {
                if (!(predicate is Uri predicateUri))
                {
                    if (!(predicate is string predicateString))
                    {
                        throw new ArgumentException("Only IUriNode, Uri or string predicates", "predicate");
                    }

                    if (!Helper.TryResolveQName(predicateString, graph, out predicateUri))
                    {
                        if (!Uri.TryCreate(predicateString, UriKind.RelativeOrAbsolute, out predicateUri))
                        {
                            throw new FormatException("Illegal Uri.");
                        }
                    }
                }

                if (!predicateUri.IsAbsoluteUri)
                {
                    if (baseUri == null)
                    {
                        throw new InvalidOperationException("Can't use relative uri index without baseUri.");
                    }

                    if (baseUri.AbsoluteUri.EndsWith("#"))
                    {
                        var builder = new UriBuilder(baseUri) { Fragment = predicateUri.ToString() };

                        predicateUri = builder.Uri;
                    }
                    else
                    {
                        predicateUri = new Uri(baseUri, predicateUri);
                    }
                }

                predicateNode = graph.CreateUriNode(predicateUri);
            }

            return predicateNode;
        }

        internal static IEnumerable<string> GetDynamicMemberNames(IEnumerable<IUriNode> nodes, Uri baseUri)
        {
            return nodes.Select(x => GetPropertyName(x, baseUri));
        }

        private static string GetPropertyName(IUriNode node, Uri baseUri)
        {
            // TODO: Consider graph.BaseUri?
            // TODO: Consider qnames?

            var nodeUri = node.Uri;

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

        private static bool TryResolveQName(string predicate, IGraph graph, out Uri predicateUri)
        {
            predicateUri = null;

            if (!RdfXmlSpecsHelper.IsValidQName(predicate))
            {
                return false;
            }

            try
            {
                predicateUri = graph.ResolveQName(predicate);
            }
            catch (RdfException)
            {
                return false;
            }

            return true;
        }
    }
}
