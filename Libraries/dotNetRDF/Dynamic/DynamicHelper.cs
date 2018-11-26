namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using VDS.RDF.Nodes;

    internal static class DynamicHelper
    {
        internal static object ConvertNode(INode node, Uri baseUri)
        {
            switch (node.AsValuedNode())
            {
                case IUriNode uriNode:
                    return new DynamicNode(node, baseUri);

                case IBlankNode blankNode:
                    if (node.IsListRoot(node.Graph))
                    {
                        return new DynamicCollectionList(node, baseUri);
                    }

                    return new DynamicNode(node, baseUri);

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

                case StringNode stringNode when stringNode.DataType is null && string.IsNullOrEmpty(stringNode.Language):
                    return stringNode.AsString();

                default:
                    return node;
            }
        }

        // TODO: Rename, not just predicates
        internal static Uri ConvertPredicate(string key, IGraph graph)
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

        // TODO: Rename, not just predicates
        internal static INode ConvertPredicate(Uri key, IGraph graph, Uri baseUri)
        {
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

        internal static INode ConvertObject(object value, IGraph graph)
        {
            if (value is IRdfCollection values)
            {
                var list = new List<object>();
                foreach (var item in values)
                {
                    list.Add(item);
                }

                // TODO: It's wrong (side-effect unexpected by caller) to assert at this point
                return graph.AssertList(list, o => ConvertObject(o, graph));
            }

            switch (value)
            {
                case INode nodeValue:
                    return nodeValue.CopyNode(graph);

                case Uri uriValue:
                    return graph.CreateUriNode(uriValue);

                case bool boolValue:
                    return new BooleanNode(graph, boolValue);

                case byte byteValue:
                    return new ByteNode(graph, byteValue);

                case DateTime dateTimeValue:
                    return new DateTimeNode(graph, dateTimeValue);

                case DateTimeOffset dateTimeOffsetValue:
                    return new DateTimeNode(graph, dateTimeOffsetValue);

                case decimal decimalValue:
                    return new DecimalNode(graph, decimalValue);

                case double doubleValue:
                    return new DoubleNode(graph, doubleValue);

                case float floatValue:
                    return new FloatNode(graph, floatValue);

                case long longValue:
                    return new LongNode(graph, longValue);

                case int intValue:
                    return new LongNode(graph, intValue);

                case string stringValue:
                    return new StringNode(graph, stringValue);

                case char charValue:
                    return new StringNode(graph, charValue.ToString());

                case TimeSpan timeSpanValue:
                    return new TimeSpanNode(graph, timeSpanValue);

                default:
                    throw new InvalidOperationException($"Can't convert type {value.GetType()}");
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
            if (index.StartsWith("urn:") || !Regex.IsMatch(index, @"^\w*:\w+$"))
            {
                indexUri = null;
                return false;
            }

            indexUri = graph.ResolveQName(index);
            return true;
        }
    }
}
