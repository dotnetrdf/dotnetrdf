/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

namespace VDS.RDF.Dynamic
{
    using System;
    using System.Text.RegularExpressions;
    using VDS.RDF.Nodes;
    using VDS.RDF.Query;

    /// <summary>
    /// Contains helper extension methods for dynamic graphs and nodes.
    /// </summary>
    public static class DynamicExtensions
    {
        /// <summary>
        /// Dynamically wraps a graph.
        /// </summary>
        /// <param name="graph">The graph to wrap dynamically.</param>
        /// <param name="subjectBaseUri">The Uri to use for resolving relative subject references.</param>
        /// <param name="predicateBaseUri">The Uri used to resolve relative predicate references.</param>
        /// <returns>A dynamic graph that wrappes <paramref name="graph"/>.</returns>
        public static dynamic AsDynamic(this IGraph graph, Uri subjectBaseUri = null, Uri predicateBaseUri = null)
        {
            return new DynamicGraph(graph, subjectBaseUri, predicateBaseUri);
        }

        /// <summary>
        /// Dynamically wraps a node.
        /// </summary>
        /// <param name="node">The node to wrap dynamically.</param>
        /// <param name="baseUri">The Uri to use for resolving relative predicate references.</param>
        /// <returns>A dynamic node that wraps <paramref name="node"/>.</returns>
        public static dynamic AsDynamic(this INode node, Uri baseUri = null)
        {
            return new DynamicNode(node, baseUri);
        }

        /// <summary>
        /// Dynamically wraps a SPARQL result set.
        /// </summary>
        /// <param name="set">The SPARQL result set to wrap dynamically.</param>
        /// <returns>A dynamic result set that wraps <paramref name="set"/>.</returns>
        public static dynamic AsDynamic(this SparqlResultSet set)
        {
            return new DynamicSparqlResultSet(set);
        }

        /// <summary>
        /// Dynamically wraps a SPARQL result.
        /// </summary>
        /// <param name="result">The SPARQL result to wrap dynamically.</param>
        /// <returns>A dynamic result that wraps <paramref name="result"/>.</returns>
        public static dynamic AsDynamic(this SparqlResult result)
        {
            return new DynamicSparqlResult(result);
        }

        internal static object AsObject(this INode node, Uri baseUri)
        {
            switch (node.AsValuedNode())
            {
                case IUriNode uriNode:
                case IBlankNode blankNode:
                    return node.AsDynamic(baseUri);

                default:
                    return node.AsObject();
            }
        }

        internal static object AsObject(this INode node)
        {
            switch (node.AsValuedNode())
            {
                case null:
                    return null;

                case IUriNode uriNode:
                    return uriNode.Uri;

                case IBlankNode blankNode:
                    return node;

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

        internal static IUriNode AsUriNode(this string key, IGraph graph, Uri baseUri)
        {
            if (!TryResolveQName(key, graph, out var uri))
            {
                if (!Uri.TryCreate(key, UriKind.RelativeOrAbsolute, out uri))
                {
                    throw new FormatException("Illegal Uri.");
                }
            }

            return uri.AsUriNode(graph, baseUri);
        }

        internal static IUriNode AsUriNode(this Uri key, IGraph graph, Uri baseUri)
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

        internal static INode AsNode(this object value, IGraph graph)
        {
            switch (value)
            {
                case INode nodeValue:
                    return nodeValue.CopyNode(graph);

                case Uri uriValue:
                    return ((INodeFactory)graph ?? new NodeFactory()).CreateUriNode(uriValue);

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

        internal static INode AsNode(this object value)
        {
            switch (value)
            {
                case null:
                    return null;

                case INode nodeValue:
                    return nodeValue;

                default:
                    return value.AsNode(null);
            }
        }

        internal static string AsName(this IUriNode node, Uri baseUri)
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
