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
using System.Diagnostics;
using System.Linq;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Shacl.Paths;

namespace VDS.RDF.Shacl;

/// <summary>
/// Represents a SHACL property path.
/// </summary>
public abstract class Path : GraphWrapperNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Path"/> class.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="shapesGraph">The graph containing the SHACL path.</param>
    [DebuggerStepThrough]
    protected Path(INode node, IGraph shapesGraph)
        : base(node, shapesGraph)
    {
    }

    internal abstract Query.Paths.ISparqlPath SparqlPath { get; }

    internal abstract IEnumerable<Triple> AsTriples { get; }

    internal static Path Parse(INode value, IGraph graph)
    {
        if (value.NodeType == NodeType.Uri)
        {
            return new Predicate(value, graph);
        }

        if (value.IsListRoot(graph))
        {
            return new Sequence(value, graph);
        }

        INode predicate = graph.GetTriplesWithSubject(value).Single().Predicate;

        switch (predicate)
        {
            case INode t when t.Equals(Vocabulary.ZeroOrMorePath): return new ZeroOrMore(value, graph);
            case INode t when t.Equals(Vocabulary.OneOrMorePath): return new OneOrMore(value, graph);
            case INode t when t.Equals(Vocabulary.AlternativePath): return new Alternative(value, graph);
            case INode t when t.Equals(Vocabulary.InversePath): return new Inverse(value, graph);
            case INode t when t.Equals(Vocabulary.ZeroOrOnePath): return new ZeroOrOne(value, graph);

            default: throw new Exception();
        }
    }

    internal IEnumerable<INode> SelectValueNodes(IGraph dataGraph, INode focusNode)
    {
        const string value = "value";
        SparqlQuery query = QueryBuilder.Select(value).Distinct().Where(new PropertyPathPattern(new NodeMatchPattern(focusNode), SparqlPath, new VariablePattern(value))).BuildQuery();
        var results = dataGraph.ExecuteQuery(query) as SparqlResultSet;
        return results?.Select(result => result[value]);
    }
}