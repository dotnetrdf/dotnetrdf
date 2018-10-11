namespace VDS.RDF.Dynamic
{
    using System;
    using System.Text.RegularExpressions;
    using VDS.RDF.Nodes;

    internal static class DynamicHelper
    {
        internal static Uri Convert(string key, IGraph graph)
        {
            if (!DynamicHelper.TryResolveQName(key, graph, out var uri))
            {
                if (!Uri.TryCreate(key, UriKind.RelativeOrAbsolute, out uri))
                {
                    throw new FormatException("Illegal Uri.");
                }
            }

            return uri;
        }

        internal static INode Convert(Uri key, IGraph graph, Uri baseUri)
        {
            if (graph is null)
            {
                throw new InvalidOperationException("missing graph");
            }

            if (!key.IsAbsoluteUri)
            {
                if (baseUri is null)
                {
                    throw new InvalidOperationException("Can't use relative uri without baseUri.");
                }

                if (baseUri.AbsoluteUri.EndsWith("#"))
                {
                    var builder = new UriBuilder(baseUri) { Fragment = key.ToString() };

                    key = builder.Uri;
                }
                else
                {
                    key = new Uri(baseUri, key);
                }
            }

            return graph.CreateUriNode(key);
        }

        internal static object ConvertToObject(INode objectNode, Uri baseUri)
        {
            switch (objectNode.AsValuedNode())
            {
                case IUriNode uriNode:
                case IBlankNode blankNode:
                    return objectNode.AsDynamic(baseUri);

                case DoubleNode doubleNode:
                    return doubleNode.AsDouble();

                case FloatNode floatNode:
                    return floatNode.AsFloat();

                case DecimalNode decimalNode:
                    return decimalNode.AsDecimal();

                case BooleanNode booleanNode:
                    return booleanNode.AsBoolean();

                case DateTimeNode dateTimeNode:
                    return dateTimeNode.AsDateTimeOffset();

                case TimeSpanNode timeSpanNode:
                    return timeSpanNode.AsTimeSpan();

                case NumericNode numericNode:
                    return numericNode.AsInteger();

                default:
                    return objectNode.ToString();
            }
        }

        internal static string ConvertToName(IUriNode node, Uri baseUri)
        {
            var nodeUri = node.Uri;

            if (node.Graph.NamespaceMap.ReduceToQName(nodeUri.AbsoluteUri, out var qname))
            {
                return qname;
            }

            if (baseUri is null)
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
            // TODO: This is naive
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
