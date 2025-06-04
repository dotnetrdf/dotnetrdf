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

using System;
using System.Collections.Generic;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Utils.Describe;

/// <summary>
/// Abstract Base Class for SPARQL Describe Algorithms which provides BNode rewriting functionality.
/// </summary>
public abstract class BaseDescribeAlgorithm
    : IDescribeAlgorithm
{
    /// <inheritdoc />
    public IGraph Describe(ITripleIndex dataset, IEnumerable<INode> nodes)
    {
        var g = new Graph();
        Describe(new GraphHandler(g), dataset, nodes);
        return g;
    }

    /// <inheritdoc />
    public void Describe(IRdfHandler handler, ITripleIndex dataset, IEnumerable<INode> nodes, Uri baseUri = null,
        INamespaceMapper namespaceMap = null)
    {
        try
        {
            handler.StartRdf();

            // Apply Base URI and Namespaces to the Handler
            if (baseUri != null)
            {
                if (!handler.HandleBaseUri(baseUri))
                {
                    ParserHelper.Stop();
                }
            }

            if (namespaceMap != null)
            {
                foreach (var prefix in namespaceMap.Prefixes)
                {
                    if (!handler.HandleNamespace(prefix, namespaceMap.GetNamespaceUri(prefix)))
                    {
                        ParserHelper.Stop();
                    }
                }
            }

            DescribeInternal(handler, dataset, nodes);

            handler.EndRdf(true);
        }
        catch (RdfParsingTerminatedException)
        {
            handler.EndRdf(true);
        }
        catch
        {
            handler.EndRdf(false);
            throw;
        }
    }

    /// <summary>
    /// Generates the Description for each of the Nodes to be described.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="dataset">Dataset to extract descriptions from.</param>
    /// <param name="nodes">Nodes to be described.</param>
    protected abstract void DescribeInternal(IRdfHandler handler, ITripleIndex dataset, IEnumerable<INode> nodes);

    
    /// <summary>
    /// Helper method which rewrites Blank Node IDs for Describe Queries.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <param name="mapping">Mapping of IDs to new Blank Nodes.</param>
    /// <param name="factory">Factory to create Nodes in.</param>
    /// <returns></returns>
    protected Triple RewriteDescribeBNodes(Triple t, Dictionary<string, INode> mapping, INodeFactory factory)
    {
        INode s, p, o;
        string id;

        if (t.Subject.NodeType == NodeType.Blank)
        {
            id = t.Subject.GetHashCode().ToString();
            if (mapping.ContainsKey(id))
            {
                s = mapping[id];
            }
            else
            {
                s = factory.CreateBlankNode(id);
                mapping.Add(id, s);
            }
        }
        else
        {
            s = t.Subject;
        }

        if (t.Predicate.NodeType == NodeType.Blank)
        {
            id = t.Predicate.GetHashCode().ToString();
            if (mapping.ContainsKey(id))
            {
                p = mapping[id];
            }
            else
            {
                p = factory.CreateBlankNode(id);
                mapping.Add(id, p);
            }
        }
        else
        {
            p = t.Predicate;
        }

        if (t.Object.NodeType == NodeType.Blank)
        {
            id = t.Object.GetHashCode().ToString();
            if (mapping.ContainsKey(id))
            {
                o = mapping[id];
            }
            else
            {
                o = factory.CreateBlankNode(id);
                mapping.Add(id, o);
            }
        }
        else
        {
            o = t.Object;
        }

        return new Triple(s, p, o);
    }
}
