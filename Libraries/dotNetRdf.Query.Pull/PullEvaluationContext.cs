/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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

using System.Globalization;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Pull;

internal class PullEvaluationContext : IPatternEvaluationContext
{
    internal readonly BaseTripleCollection _defaultGraph;
    private readonly IDictionary<IRefNode, BaseTripleCollection> _namedGraphs;
    
    public bool RigorousEvaluation { get; }

    public ISparqlNodeComparer NodeComparer { get; private set; }

    public SparqlOrderingComparer OrderingComparer { get; private set; }
    internal IEnumerable<IRefNode> NamedGraphNames => _namedGraphs.Keys;
    public VariableFactory AutoVarFactory { get; private set; }
    internal ISparqlExpressionProcessor<IValuedNode, PullEvaluationContext, ExpressionContext> ExpressionProcessor { get; }

    /// <summary>
    /// Get or set the total length of the slice of results that the processor will return (offset + limit).
    /// </summary>
    internal int? SliceLength { get; set; } 
    public ITripleStore Data { get; private set; }
    public bool UnionDefaultGraph { get; private set; }
    public INodeFactory NodeFactory { get; private set; }
    public Uri? BaseUri { get; private set; }
    public IUriFactory UriFactory { get; private set; }

    private PullEvaluationContext(
        PullEvaluationContext parent,
        bool unionDefaultGraph,
        IEnumerable<IRefNode> defaultGraphNames,
        IEnumerable<IRefNode> namedGraphs):
    this(
        parent.Data,
        unionDefaultGraph,
        defaultGraphNames,
        namedGraphs,
        parent.AutoVarFactory.Prefix,
        parent.NodeFactory,
        parent.UriFactory,
        parent.NodeComparer,
        parent.BaseUri)
    {
        
    }
    public PullEvaluationContext(
        ITripleStore data,
        bool unionDefaultGraph = true,
        IEnumerable<IRefNode?>? defaultGraphNames = null,
        IEnumerable<IRefNode>? namedGraphs = null,
        string? autoVarPrefix = null,
        INodeFactory? nodeFactory = null,
        IUriFactory? uriFactory = null,
        ISparqlNodeComparer? nodeComparer = null,
        Uri? baseUri = null)
    {
        NodeFactory = nodeFactory ?? new NodeFactory();
        Data = data;
        UnionDefaultGraph = unionDefaultGraph;
        NodeComparer = nodeComparer ?? new SparqlNodeComparer(CultureInfo.InvariantCulture, CompareOptions.Ordinal);
        OrderingComparer = new SparqlOrderingComparer(NodeComparer);
        AutoVarFactory = new VariableFactory(autoVarPrefix ?? "_auto");
        BaseUri = baseUri;
        var customDefaultGraph = false;
        if (unionDefaultGraph)
        {
            _defaultGraph = new UnionTripleCollection(data.Graphs.First().Triples, data.Graphs.Skip(1).Select(g=>g.Triples));
        }
        else
        {
            if (defaultGraphNames != null)
            {
                var graphNames = defaultGraphNames.Where(data.HasGraph).ToList();
                _defaultGraph = graphNames.Count switch
                {
                    0 => data.HasGraph((IRefNode?)null) ? data[(IRefNode?)null].Triples : new TripleCollection(),
                    1 => data[graphNames[0]].Triples,
                    _ => new UnionTripleCollection(data[graphNames[0]].Triples,
                        graphNames.Skip(1).Select(g => data[g].Triples))
                };
                if (graphNames.Any(g=>g != null)) customDefaultGraph = true;
            }
            else
            {
                _defaultGraph = data.HasGraph((IRefNode?)null) ? data[(IRefNode?)null].Triples : new TripleCollection();
            }
        }

        _namedGraphs = namedGraphs != null ? namedGraphs.ToDictionary(g => g, g => data.HasGraph(g) ? data[g].Triples : new TripleCollection()) : [];
        if (!unionDefaultGraph && !customDefaultGraph && !_namedGraphs.Any())
        {
            _namedGraphs = data.Graphs.Where(g => g.Name != null).ToDictionary(g => g.Name, g => g.Triples);
        }
        RigorousEvaluation = true;
        UriFactory = uriFactory ?? VDS.RDF.UriFactory.Root;
        ExpressionProcessor = new PullExpressionProcessor(
            new SparqlNodeComparer(CultureInfo.InvariantCulture, CompareOptions.Ordinal),
            UriFactory,
            RigorousEvaluation);

    }

    internal PullEvaluationContext MakeSubContext(IEnumerable<IRefNode> defaultGraphNames, IEnumerable<IRefNode> namedGraphs)
    {
        return new PullEvaluationContext(this, UnionDefaultGraph, defaultGraphNames, namedGraphs);
    }

    private static INode? GetNode(PatternItem patternItem, ISet? inputBindings)
    {
        if (patternItem is NodeMatchPattern nodeMatchPattern) { return nodeMatchPattern.Node; }

        if (inputBindings != null && patternItem is VariablePattern variablePattern &&
            inputBindings.ContainsVariable(variablePattern.VariableName))
        {
            return inputBindings[variablePattern.VariableName];
        }

        return null;
    }

    private bool ContainsTriple(Triple t, IRefNode? activeGraph)
    {
        if (activeGraph != null)
        {
            return _namedGraphs.ContainsKey(activeGraph) && _namedGraphs[activeGraph].Contains(t);
        }

        return _defaultGraph.Contains(t);
    }

    internal bool HasAnyTriples(IRefNode? activeGraph)
    {
        BaseTripleCollection tripleCollection = activeGraph != null ? _namedGraphs[activeGraph] : _defaultGraph;
        return tripleCollection.Any();
    }
    internal IEnumerable<Triple> GetTriples(IMatchTriplePattern triplePattern, ISet? inputBindings, IRefNode? activeGraph)
    {
        BaseTripleCollection tripleCollection = activeGraph != null ? _namedGraphs[activeGraph] : _defaultGraph;
        // Expand quoted triple patterns in subject or object position of the triple pattern
        if (triplePattern.Subject is QuotedTriplePattern subjectTriplePattern)
        {
            return GetQuotedTriples(subjectTriplePattern, activeGraph).SelectMany(tn =>
                GetTriples(new TriplePattern(new NodeMatchPattern(tn), triplePattern.Predicate,
                    triplePattern.Object), inputBindings, activeGraph));
        }

        if (triplePattern.Object is QuotedTriplePattern objectTriplePattern)
        {
            return GetQuotedTriples(objectTriplePattern, activeGraph).SelectMany(tn =>
                GetTriples(new TriplePattern(triplePattern.Subject, triplePattern.Predicate,
                    new NodeMatchPattern(tn)), inputBindings, activeGraph));
        }

        INode? subj = GetNode(triplePattern.Subject, inputBindings);
        INode? pred = GetNode(triplePattern.Predicate, inputBindings);
        INode? obj = GetNode(triplePattern.Object, inputBindings);
        if (subj != null)
        {
            if (pred != null)
            {
                if (obj != null)
                {
                    // Return if the triple exists
                    var t = new Triple(subj, pred, obj);
                    return ContainsTriple(t, activeGraph) ? [t] : [];
                }
            }
        }
        return tripleCollection[(subj, pred, obj)];
    }

    internal IEnumerable<INode> GetNodes(PatternItem nodePattern, IRefNode? activeGraph)
    {
        BaseTripleCollection tripleCollection = activeGraph != null ? _namedGraphs[activeGraph] : _defaultGraph;
        return tripleCollection.SubjectNodes.Where(n => nodePattern.Accepts(this, n, new Set()))
            .Union(tripleCollection.ObjectNodes.Where(n => nodePattern.Accepts(this, n, new Set())));
    }

    private IEnumerable<ITripleNode> GetQuotedTriples(QuotedTriplePattern qtp, IRefNode? activeGraph)
    {
        TriplePattern triplePattern = qtp.QuotedTriple;
        INode s, p, o;
        BaseTripleCollection tripleCollection = activeGraph != null ? _namedGraphs[activeGraph] : _defaultGraph;
        switch (triplePattern.IndexType)
        {
            case TripleIndexType.Subject:
                s = ((NodeMatchPattern)triplePattern.Subject).Node;
                return tripleCollection.QuotedWithSubject(s).Select(t => new TripleNode(t));

            case TripleIndexType.Predicate:
                p = ((NodeMatchPattern)triplePattern.Predicate).Node;
                return tripleCollection.QuotedWithPredicate(p).Select(t => new TripleNode(t));

            case TripleIndexType.Object:
                o = ((NodeMatchPattern)triplePattern.Object).Node;
                return tripleCollection.QuotedWithObject(o).Select(t => new TripleNode(t));

            case TripleIndexType.SubjectPredicate:
                s = ((NodeMatchPattern)triplePattern.Subject).Node;
                p = ((NodeMatchPattern)triplePattern.Predicate).Node;
                return tripleCollection.QuotedWithSubjectPredicate(s, p).Select(t => new TripleNode(t));

            case TripleIndexType.SubjectObject:
                s = ((NodeMatchPattern)triplePattern.Subject).Node;
                o = ((NodeMatchPattern)triplePattern.Object).Node;
                return tripleCollection.QuotedWithSubjectObject(s, o).Select(t => new TripleNode(t));

            case TripleIndexType.PredicateObject:
                p = ((NodeMatchPattern)triplePattern.Predicate).Node;
                o = ((NodeMatchPattern)triplePattern.Object).Node;
                return tripleCollection.QuotedWithPredicateObject(p, o).Select(t => new TripleNode(t));

            case TripleIndexType.NoVariables:
                s = ((NodeMatchPattern)triplePattern.Subject).Node;
                p = ((NodeMatchPattern)triplePattern.Predicate).Node;
                o = ((NodeMatchPattern)triplePattern.Object).Node;
                var t = new Triple(s, p, o);
                if (tripleCollection.ContainsQuoted(t))
                {
                    return [new TripleNode(t)];
                }
                return [];
            case TripleIndexType.None:
                return tripleCollection.Quoted.Select(t => new TripleNode(t));
        }

        return [];
    }

    public bool ContainsVariable(string varName)
    {
        return true;
    }

    public bool ContainsValue(string varName, INode value)
    {
        return true;
    }
}