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

using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;

namespace VDS.RDF.Utils.Describe;

/// <summary>
/// Computes a Concise Bounded Description for all the Values resulting from the Query.
/// </summary>
/// <remarks>
/// <para>
/// The Description returned is all the Triples for which a Value is a Subject and with any Blank Nodes expanded to include Triples with the Blank Node as the Subject.
/// </para>
/// </remarks>
public class ConciseBoundedDescription 
    : BaseDescribeAlgorithm
{
    /// <summary>
    /// Generates the Description for each of the Nodes to be described.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="dataset">Dataset to extract descriptions from.</param>
    /// <param name="nodes">Nodes to be described.</param>
    protected override void DescribeInternal(IRdfHandler handler, ITripleIndex dataset, IEnumerable<INode> nodes)
    {
        // Rewrite Blank Node IDs for DESCRIBE Results
        var bnodeMapping = new Dictionary<string, INode>();

        // Get Triples for this Subject
        var bnodes = new Queue<INode>();
        var expandedBNodes = new HashSet<INode>();
        foreach (INode n in nodes)
        {
            // Get Triples where the Node is the Subject
            foreach (Triple t in dataset.GetTriplesWithSubject(n).ToList())
            {
                if (t.Object.NodeType == NodeType.Blank)
                {
                    if (!expandedBNodes.Contains(t.Object)) bnodes.Enqueue(t.Object);
                }
                if (!handler.HandleTriple(RewriteDescribeBNodes(t, bnodeMapping, handler))) ParserHelper.Stop();
            }

            // Compute the Blank Node Closure for this Subject
            while (bnodes.Count > 0)
            {
                INode bsubj = bnodes.Dequeue();
                if (expandedBNodes.Contains(bsubj)) continue;
                expandedBNodes.Add(bsubj);

                foreach (Triple t2 in dataset.GetTriplesWithSubject(bsubj).ToList())
                {
                    if (t2.Object.NodeType == NodeType.Blank)
                    {
                        if (!expandedBNodes.Contains(t2.Object)) bnodes.Enqueue(t2.Object);
                    }
                    if (!handler.HandleTriple(RewriteDescribeBNodes(t2, bnodeMapping, handler))) ParserHelper.Stop();
                }
            }
        }
    }
}
