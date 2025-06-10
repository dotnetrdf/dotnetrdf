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
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Utils.Describe;

namespace VDS.RDF.Query.Describe;

/// <summary>
/// Wrapper class that implements the <see cref="ISparqlDescribe"/> interface for describing SPARQL query results by
/// delegating to a <see cref="IDescribeAlgorithm"/> implementation to do the extraction from the query dataset.
/// </summary>
public class SparqlDescriber
    : ISparqlDescribe
{
    private readonly IDescribeAlgorithm _describeAlgorithm;

    /// <summary>
    /// Create a new describer instance that uses the specified algorithm.
    /// </summary>
    /// <param name="describeAlgorithm">Describe algorithm.</param>
    public SparqlDescriber(IDescribeAlgorithm describeAlgorithm)
    {
        _describeAlgorithm = describeAlgorithm;
    }

    /// <summary>
    /// Gets the Description Graph based on the Query Results from the given Evaluation Context.
    /// </summary>
    /// <param name="context">SPARQL Evaluation Context.</param>
    /// <returns></returns>
    public IGraph Describe(SparqlEvaluationContext context)
    {
        var g = new Graph();
        Describe(new GraphHandler(g), (ISparqlDescribeContext)context);
        return g;
    }

    /// <summary>
    /// Gets the Description Graph based on the Query Results from the given Evaluation Context passing the resulting Triples to the given RDF Handler.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public void Describe(IRdfHandler handler, SparqlEvaluationContext context)
    {
        Describe(handler, (ISparqlDescribeContext)context);
    }

    /// <inheritdoc />
    public IGraph Describe(ISparqlDescribeContext context)
    {
        var g = new Graph();
        Describe(new GraphHandler(g), context);
        return g;
    }

    /// <inheritdoc />
    public void Describe(IRdfHandler handler, ISparqlDescribeContext context)
    {
        IEnumerable<INode> nodes = context.GetNodes(handler);
        _describeAlgorithm.Describe(handler, context.TripleIndex, nodes, context.Query?.BaseUri,
            context.Query?.NamespaceMap);
    }

}
