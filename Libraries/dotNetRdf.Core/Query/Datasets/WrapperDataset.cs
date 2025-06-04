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
using System.Reflection;
using System.Threading;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Datasets;

/// <summary>
/// An abstract dataset wrapper that can be used to wrap another dataset and just modify some functionality i.e. provides a decorator over an existing dataset.
/// </summary>
public abstract class WrapperDataset
    : IConfigurationSerializable, IThreadSafeDataset
{
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    /// <summary>
    /// Underlying Dataset.
    /// </summary>
    protected ISparqlDataset _dataset;

    /// <summary>
    /// Creates a new wrapped dataset.
    /// </summary>
    /// <param name="dataset">Dataset.</param>
    protected WrapperDataset(ISparqlDataset dataset)
    {
        _dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
    }

    /// <summary>
    /// Gets the Lock used to ensure MRSW concurrency on the dataset when available.
    /// </summary>
    public ReaderWriterLockSlim Lock
    {
        get
        {
            if (_dataset is IThreadSafeDataset threadSafeDataset)
            {
                return threadSafeDataset.Lock;
            }

            return _lock;
        }
    }

    /// <summary>
    /// Gets the underlying dataset.
    /// </summary>
    public ISparqlDataset UnderlyingDataset
    {
        get
        {
            return _dataset;
        }
    }

    #region ISparqlDataset Members

    /// <summary>
    /// Sets the Active Graph for the dataset.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    [Obsolete("Replaced by SetActiveGraph(IList<IRefNode>)")]
    public virtual void SetActiveGraph(IEnumerable<Uri> graphUris)
    {
        _dataset.SetActiveGraph(graphUris);
    }

    /// <summary>
    /// Sets the Active Graph for the dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    [Obsolete("Replaced by SetActiveGraph(IRefNode)")]
    public virtual void SetActiveGraph(Uri graphUri)
    {
        _dataset.SetActiveGraph(graphUri);
    }

    /// <summary>
    /// Sets the active graph to be the graph with the given name.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    public virtual void SetActiveGraph(IRefNode graphName)
    {
        _dataset.SetActiveGraph(graphName);
    }

    /// <summary>
    /// Sets the active graph to be the union of the graphs with the given names.
    /// </summary>
    /// <param name="graphNames"></param>
    public virtual void SetActiveGraph(IList<IRefNode> graphNames)
    {
        _dataset.SetActiveGraph(graphNames);
    }

    /// <summary>
    /// Sets the Default Graph for the dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    [Obsolete("Replaced by SetDefaultGraph(IRefNode)")]
    public virtual void SetDefaultGraph(Uri graphUri)
    {
        _dataset.SetDefaultGraph(graphUri);
    }

    /// <summary>
    /// Sets the Default Graph for the dataset.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    [Obsolete("Replaced by SetDefaultGraph(IList<IRefNode>)")]
    public virtual void SetDefaultGraph(IEnumerable<Uri> graphUris)
    {
        _dataset.SetDefaultGraph(graphUris);
    }

    /// <summary>
    /// Sets the default graph to be the graph with the given name.
    /// </summary>
    /// <param name="graphName"></param>
    public virtual void SetDefaultGraph(IRefNode graphName)
    {
        _dataset.SetDefaultGraph(graphName);
    }
    /// <summary>
    /// Sets the default graph to be the union of the graphs with the given names.
    /// </summary>
    /// <param name="graphNames">Graph names.</param>
    public virtual void SetDefaultGraph(IList<IRefNode> graphNames)
    {
        _dataset.SetDefaultGraph(graphNames);
    }

    /// <summary>
    /// Resets the Active Graph.
    /// </summary>
    public virtual void ResetActiveGraph()
    {
        _dataset.ResetActiveGraph();
    }

    /// <summary>
    /// Resets the Default Graph.
    /// </summary>
    public virtual void ResetDefaultGraph()
    {
        _dataset.ResetDefaultGraph();
    }

    /// <summary>
    /// Gets the Default Graph URIs.
    /// </summary>
    [Obsolete("Replaced by DefaultGraphNames")]
    public virtual IEnumerable<Uri> DefaultGraphUris
    {
        get
        {
            return _dataset.DefaultGraphUris;
        }
    }

    /// <summary>
    /// Gets the Active Graph URIs.
    /// </summary>
    [Obsolete("Replaced by ActiveGraphNames")]
    public virtual IEnumerable<Uri> ActiveGraphUris
    {
        get
        {
            return _dataset.ActiveGraphUris;
        }
    }

    /// <summary>
    /// Gets the enumeration of the names of the graphs that currently make up the default graph.
    /// </summary>
    public virtual IEnumerable<IRefNode> DefaultGraphNames => _dataset.DefaultGraphNames;

    /// <summary>
    /// Gets the enumeration of the names of the graphs that currently make up the active graph.
    /// </summary>
    public virtual IEnumerable<IRefNode> ActiveGraphNames => _dataset.ActiveGraphNames;
    /// <summary>
    /// Gets whether the default graph is the union of all graphs.
    /// </summary>
    public virtual bool UsesUnionDefaultGraph
    {
        get
        {
            return _dataset.UsesUnionDefaultGraph;
        }
    }

    /// <summary>
    /// Adds a Graph to the dataset.
    /// </summary>
    /// <param name="g">Graph.</param>
    public virtual bool AddGraph(IGraph g)
    {
        return _dataset.AddGraph(g);
    }

    /// <summary>
    /// Removes a Graph from the dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    [Obsolete("Replaced by RemoveGraph(IRefNode)")]
    public virtual bool RemoveGraph(Uri graphUri)
    {
        return _dataset.RemoveGraph(graphUri);
    }

    /// <summary>
    /// Removes a Graph from the Dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <exception cref="NotSupportedException">May be thrown if the Dataset is immutable i.e. Updates not supported.</exception>
    public virtual bool RemoveGraph(IRefNode graphName)
    {
        return _dataset.RemoveGraph(graphName);
    }

    /// <summary>
    /// Gets whether the dataset contains a given Graph.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by HasGraph(IRefNode)")]
    public virtual bool HasGraph(Uri graphUri)
    {
        return _dataset.HasGraph(graphUri);
    }

    /// <summary>
    /// Gets whether a Graph with the given name is the Dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    public virtual bool HasGraph(IRefNode graphName)
    {
        return _dataset.HasGraph(graphName);
    }

    /// <summary>
    /// Gets the Graphs in the dataset.
    /// </summary>
    public virtual IEnumerable<IGraph> Graphs
    {
        get 
        {
            return _dataset.Graphs;
        }
    }

    /// <summary>
    /// Gets the URIs of Graphs in the dataset.
    /// </summary>
    [Obsolete("Replaced by GraphNames")]
    public virtual IEnumerable<Uri> GraphUris
    {
        get 
        {
            return _dataset.GraphUris;
        }
    }

    /// <summary>
    /// Gets an enumeration of the names of all graphs in the dataset.
    /// </summary>
    public virtual IEnumerable<IRefNode> GraphNames => _dataset.GraphNames;

    /// <summary>
    /// Gets a Graph from the dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by this[IRefNode]")]
    public virtual IGraph this[Uri graphUri]
    {
        get
        {
            return _dataset[graphUri];
        }
    }

    /// <summary>
    /// Gets the graph with the given name from the dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// This property need only return a read-only view of the Graph, code which wishes to modify Graphs should use the <see cref="ISparqlDataset.GetModifiableGraph(IRefNode)">GetModifiableGraph()</see> method to guarantee a Graph they can modify and will be persisted to the underlying storage.
    /// </para>
    /// </remarks>
    public virtual IGraph this[IRefNode graphName] => _dataset[graphName];

    /// <summary>
    /// Gets a modifiable graph from the dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by GetModifiableGraph(IRefNode)")]
    public virtual IGraph GetModifiableGraph(Uri graphUri)
    {
        return _dataset.GetModifiableGraph(graphUri);
    }

    /// <summary>
    /// Gets the Graph with the given name from the Dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">May be thrown if the Dataset is immutable i.e. Updates not supported.</exception>
    /// <remarks>
    /// <para>
    /// Graphs returned from this method must be modifiable and the Dataset must guarantee that when it is Flushed or Disposed of that any changes to the Graph are persisted.
    /// </para>
    /// </remarks>
    public virtual IGraph GetModifiableGraph(IRefNode graphName)
    {
        return _dataset.GetModifiableGraph(graphName);
    }

    /// <summary>
    /// Gets whether the dataset has any triples.
    /// </summary>
    public virtual bool HasTriples
    {
        get 
        {
            return _dataset.HasTriples; 
        }
    }

    /// <summary>
    /// Gets whether the dataset contains a given triple.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns></returns>
    public virtual bool ContainsTriple(Triple t)
    {
        return _dataset.ContainsTriple(t);
    }

    /// <inheritdoc />
    public IEnumerable<Triple> QuotedTriples => _dataset.QuotedTriples;

    /// <summary>
    /// Gets whether the dataset contains a specific quoted triple.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns>True if the dataset contains <paramref name="t"/> as a quoted triple, false otherwise.</returns>
    public virtual bool ContainsQuotedTriple(Triple t)
    {
        return _dataset.ContainsQuotedTriple(t);
    }

    /// <summary>
    /// Gets all triples from the dataset.
    /// </summary>
    public virtual IEnumerable<Triple> Triples
    {
        get
        {
            return _dataset.Triples;
        }
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithPredicate(Uri u)
    {
        return _dataset.GetTriplesWithPredicate(u);
    }

    /// <summary>
    /// Gets triples with a given subject.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> GetTriplesWithSubject(INode subj)
    {
        return _dataset.GetTriplesWithSubject(subj);
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithSubject(Uri u)
    {
        return _dataset.GetTriplesWithSubject(u);
    }

    /// <summary>
    /// Gets triples with a given predicate.
    /// </summary>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
    {
        return _dataset.GetTriplesWithPredicate(pred);
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriples(Uri uri)
    {
        return _dataset.GetTriples(uri);
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriples(INode n)
    {
        return _dataset.GetTriples(n);
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithObject(Uri u)
    {
        return _dataset.GetTriplesWithObject(u);
    }

    /// <summary>
    /// Gets triples with a given object.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> GetTriplesWithObject(INode obj)
    {
        return _dataset.GetTriplesWithObject(obj);
    }

    /// <summary>
    /// Gets triples with a given subject and predicate.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
    {
        return _dataset.GetTriplesWithSubjectPredicate(subj, pred);
    }

    /// <summary>
    /// Gets triples with a given subject and object.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
    {
        return _dataset.GetTriplesWithSubjectObject(subj, obj);
    }

    /// <summary>
    /// Gets triples with a given predicate and object.
    /// </summary>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
    {
        return _dataset.GetTriplesWithPredicateObject(pred, obj);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuoted(Uri uri)
    {
        return _dataset.GetQuoted(uri);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuoted(INode n)
    {
        return _dataset.GetQuoted(n);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithObject(Uri u)
    {
        return _dataset.GetQuotedWithObject(u);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithObject(INode n)
    {
        return _dataset.GetQuotedWithObject(n);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithPredicate(INode n)
    {
        return _dataset.GetQuotedWithPredicate(n);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithPredicate(Uri u)
    {
        return _dataset.GetQuotedWithPredicate(u);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithSubject(INode n)
    {
        return _dataset.GetQuotedWithSubject(n);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithSubject(Uri u)
    {
        return _dataset.GetQuotedWithSubject(u);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithSubjectPredicate(INode subj, INode pred)
    {
        return _dataset.GetQuotedWithSubjectPredicate(subj, pred);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithSubjectObject(INode subj, INode obj)
    {
        return _dataset.GetQuotedWithSubjectObject(subj, obj);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithPredicateObject(INode pred, INode obj)
    {
        return _dataset.GetQuotedWithPredicateObject(pred, obj);
    }

    /// <summary>
    /// Flushes any changes to the dataset.
    /// </summary>
    public virtual void Flush()
    {
        _dataset.Flush();
    }

    /// <summary>
    /// Discards any changes to the dataset.
    /// </summary>
    public virtual void Discard()
    {
        _dataset.Discard();
    }

    #endregion

    /// <summary>
    /// Serializes the Configuration of the Dataset.
    /// </summary>
    /// <param name="context">Serialization Context.</param>
    public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        if (_dataset is IConfigurationSerializable)
        {
            INode dataset = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
            INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
            INode datasetClass = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassSparqlDataset));
            INode usingDataset = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyUsingDataset));
            INode innerDataset = context.Graph.CreateBlankNode();

#if NETCORE
            String assm = typeof(WrapperDataset).GetTypeInfo().Assembly.FullName;
#else
            var assm = Assembly.GetAssembly(GetType()).FullName;
#endif
            if (assm.Contains(",")) assm = assm.Substring(0, assm.IndexOf(','));
            var effectiveType = GetType().FullName + (assm.Equals("dotNetRDF") ? string.Empty : ", " + assm);

            context.Graph.Assert(dataset, rdfType, datasetClass);
            context.Graph.Assert(dataset, dnrType, context.Graph.CreateLiteralNode(effectiveType));
            context.Graph.Assert(dataset, usingDataset, innerDataset);
            context.NextSubject = innerDataset;

            ((IConfigurationSerializable)_dataset).SerializeConfiguration(context);
        }
        else
        {
            throw new DotNetRdfConfigurationException("Unable to serialize configuration as the inner dataset is now serializable");
        }
    }
}
