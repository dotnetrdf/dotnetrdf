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
using System.Linq;

namespace VDS.RDF.Query.Datasets;

/// <summary>
/// A Triple Collection which is a thin wrapper around a <see cref="BaseQuadDataset">BaseQuadDataset</see> to reduce much of the complexity for <see cref="ISparqlDataset">ISparqlDataset</see> implementors around returning of Graphs.
/// </summary>
public class QuadDatasetTripleCollection
    : BaseTripleCollection
{
    private readonly BaseQuadDataset _dataset;
    private readonly IRefNode _graphName;

    /// <summary>
    /// Create a new triple collection that exposes a single named graph in a quad dataset.
    /// </summary>
    /// <param name="dataset">The dataset to wrap.</param>
    /// <param name="graphName">The URI of the graph to expose through this interface.</param>
    [Obsolete("Replaced by QuadDatasetTripleCollection(BaseQuadDataset, IRefNode)")]
    public QuadDatasetTripleCollection(BaseQuadDataset dataset, Uri graphName) : this(dataset, new UriNode(graphName)) { }

    /// <summary>
    /// Create a new triple collection that exposes a single named graph in a quad dataset.
    /// </summary>
    /// <param name="dataset">The dataset to wrap.</param>
    /// <param name="graphName">The URI of the graph to expose through this interface.</param>
    public QuadDatasetTripleCollection(BaseQuadDataset dataset, IRefNode graphName)
    {
        _dataset = dataset;
        _graphName = graphName;
    }


    /// <inheritdoc />
    protected internal override bool Add(Triple t)
    {
        return _dataset.AddQuad(_graphName, t);
    }

    /// <inheritdoc />
    public override bool Contains(Triple t)
    {
        return _dataset.ContainsQuad(_graphName, t);
    }

    /// <inheritdoc/>
    public override bool ContainsQuoted(Triple t)
    {
        return _dataset.ContainsQuoted(_graphName, t);
    }

    /// <inheritdoc />
    public override int Count
    {
        get 
        {
            return _dataset.GetQuads(_graphName).Count();
        }
    }

    /// <inheritdoc />
    public override int QuotedCount => _dataset.GetQuoted(_graphName).Count();

    /// <inheritdoc />
    protected internal override bool Delete(Triple t)
    {
        return _dataset.RemoveQuad(_graphName, t);
    }

    /// <inheritdoc />
    public override Triple this[Triple t]
    {
        get 
        {
            if (_dataset.ContainsQuad(_graphName, t))
            {
                return t;
            }
            else
            {
                throw new KeyNotFoundException("Given Triple does not exist in the Graph");
            }
        }
    }

    /// <inheritdoc />
    public override IEnumerable<INode> ObjectNodes
    {
        get 
        {
            return _dataset.GetQuads(_graphName).Select(t => t.Object).Distinct();
        }
    }

    /// <inheritdoc />
    public override IEnumerable<INode> PredicateNodes
    {
        get 
        {
            return _dataset.GetQuads(_graphName).Select(t => t.Predicate).Distinct();
        }
    }

    /// <inheritdoc />
    public override IEnumerable<INode> SubjectNodes
    {
        get 
        {
            return _dataset.GetQuads(_graphName).Select(t => t.Subject).Distinct(); 
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<INode> QuotedObjectNodes => 
        _dataset.GetQuoted(_graphName).Select(t=>t.Object).Distinct();

    /// <inheritdoc/>
    public override IEnumerable<INode> QuotedPredicateNodes =>
        _dataset.GetQuoted(_graphName).Select(t => t.Predicate).Distinct();

    /// <inheritdoc/>
    public override IEnumerable<INode> QuotedSubjectNodes =>
        _dataset.GetQuoted(_graphName).Select(t => t.Subject).Distinct();

    /// <inheritdoc />
    public override IEnumerator<Triple> GetEnumerator()
    {
        return _dataset.GetQuads(_graphName).GetEnumerator();
    }

    /// <inheritdoc />
    public override IEnumerable<Triple> Asserted =>  _dataset.GetQuads(_graphName);

    /// <inheritdoc />
    public override IEnumerable<Triple> Quoted => _dataset.GetQuoted(_graphName);


    /// <inheritdoc />
    public override IEnumerable<Triple> WithObject(INode obj)
    {
        return _dataset.GetQuadsWithObject(_graphName, obj);
    }

    /// <inheritdoc />
    public override IEnumerable<Triple> WithPredicate(INode pred)
    {
        return _dataset.GetQuadsWithPredicate(_graphName, pred);
    }

    /// <inheritdoc />
    public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
    {
        return _dataset.GetQuadsWithPredicateObject(_graphName, pred, obj);
    }

    /// <inheritdoc />
    public override IEnumerable<Triple> WithSubject(INode subj)
    {
        return _dataset.GetQuadsWithSubject(_graphName, subj);
    }

    /// <inheritdoc />
    public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
    {
        return _dataset.GetQuadsWithSubjectObject(_graphName, subj, obj);
    }

    /// <inheritdoc />
    public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
    {
        return _dataset.GetQuadsWithSubjectPredicate(_graphName, subj, pred);
    }

    /// <inheritdoc />
    public override IEnumerable<Triple>QuotedWithObject(INode obj)
    {
        return _dataset.GetQuotedWithObject(_graphName, obj);
    }

    /// <inheritdoc />
    public override IEnumerable<Triple>QuotedWithPredicate(INode pred)
    {
        return _dataset.GetQuotedWithPredicate(_graphName, pred);
    }

    /// <inheritdoc />
    public override IEnumerable<Triple>QuotedWithPredicateObject(INode pred, INode obj)
    {
        return _dataset.GetQuotedWithPredicateObject(_graphName, pred, obj);
    }

    /// <inheritdoc />
    public override IEnumerable<Triple>QuotedWithSubject(INode subj)
    {
        return _dataset.GetQuotedWithSubject(_graphName, subj);
    }

    /// <inheritdoc />
    public override IEnumerable<Triple>QuotedWithSubjectObject(INode subj, INode obj)
    {
        return _dataset.GetQuotedWithSubjectObject(_graphName, subj, obj);
    }

    /// <inheritdoc />
    public override IEnumerable<Triple>QuotedWithSubjectPredicate(INode subj, INode pred)
    {
        return _dataset.GetQuotedWithSubjectPredicate(_graphName, subj, pred);
    }
}
