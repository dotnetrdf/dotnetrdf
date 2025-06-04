/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using VDS.RDF.Parsing;
using System;
using System.Text.RegularExpressions;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace VDS.RDF.Dynamic;

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
    /// <param name="graph">The graph context of th dynamic node.</param>
    /// <param name="baseUri">The Uri to use for resolving relative predicate references.</param>
    /// <returns>A dynamic node that wraps <paramref name="node"/>.</returns>
    public static dynamic AsDynamic(this INode node, IGraph graph, Uri baseUri = null)
    {
        return new DynamicNode(node, graph, baseUri);
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
    public static dynamic AsDynamic(this ISparqlResult result)
    {
        return new DynamicSparqlResult(result);
    }

    internal static object AsObject(this INode node, IGraph graph, Uri baseUri)
    {
        switch (node.AsValuedNode())
        {
            case IUriNode _:
            case IBlankNode _:
                return node.AsDynamic(graph, baseUri);

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

            case IBlankNode _:
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

            case StringNode stringNode when stringNode.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString):
                return stringNode.AsString();

            default:
                return node;
        }
    }

    internal static IUriNode AsUriNode(this string key, IGraph graph, Uri baseUri)
    {
        if (!TryResolveQName(key, graph, out Uri uri))
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
                return nodeValue;

            case Uri uriValue:
                return ((INodeFactory)graph ?? new NodeFactory(new NodeFactoryOptions())).CreateUriNode(uriValue);

            case bool boolValue:
                return new BooleanNode(boolValue);

            case byte byteValue:
                return new ByteNode(byteValue);

            case DateTime dateTimeValue:
                return new DateTimeNode(dateTimeValue);

            case DateTimeOffset dateTimeOffsetValue:
                return new DateTimeNode(dateTimeOffsetValue);

            case decimal decimalValue:
                return new DecimalNode(decimalValue);

            case double doubleValue:
                return new DoubleNode(doubleValue);

            case float floatValue:
                return new FloatNode(floatValue);

            case long longValue:
                return new LongNode(longValue);

            case int intValue:
                return new LongNode(intValue);

            case string stringValue:
                return new StringNode(stringValue);

            case char charValue:
                return new StringNode(charValue.ToString());

            case TimeSpan timeSpanValue:
                return new TimeSpanNode(timeSpanValue);

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

    internal static string AsName(this IUriNode node, Uri baseUri, INamespaceMapper nsMapper)
    {
        Uri nodeUri = node.Uri;

        if (nsMapper.ReduceToQName(nodeUri.AbsoluteUri, out var qname))
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
