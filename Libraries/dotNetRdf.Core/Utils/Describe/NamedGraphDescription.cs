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
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Utils.Describe;

/// <summary>
/// Computes a Description for all the results such that the description is the merge of all the Graphs named with a resulting URI.
/// </summary>
public class NamedGraphDescription 
    : BaseDescribeAlgorithm
{
    /// <summary>
    /// Generates the Description for each of the Nodes to be described.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="dataset">The dataset to extract descriptions from.</param>
    /// <param name="nodes">Nodes to be described.</param>
    protected override void DescribeInternal(IRdfHandler handler, ITripleIndex dataset, IEnumerable<INode> nodes)
    {
        // Rewrite Blank Node IDs for DESCRIBE Results
        var bnodeMapping = new Dictionary<string, INode>();

        if (dataset is ISparqlDataset sparqlDataset)
        {
            foreach (INode n in nodes)
            {
                if (n.NodeType is NodeType.Uri or NodeType.Blank)
                {
                    IGraph g = sparqlDataset[(IRefNode)n];
                    foreach (Triple t in g.Triples.ToList())
                    {
                        if (!handler.HandleTriple(RewriteDescribeBNodes(t, bnodeMapping, handler)))
                            ParserHelper.Stop();
                    }
                }
            }
        }
    }
}
