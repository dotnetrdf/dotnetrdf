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
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF;

/// <summary>
/// Represents an in-memory view of a triple store provided by an <see cref="IStorageProvider">IStorageProvider</see> instance where changes to the in-memory view get reflected in the persisted view.
/// </summary>
/// <remarks>
/// <h3>Persistence Behaviour</h3>
/// <para>
/// <strong>Note:</strong> This is a transactional implementation - this means that changes made are not persisted until you either call <see cref="PersistentTripleStore.Flush()">Flush()</see> or you dispose of the instance.  Alternatively you may invoke the <see cref="PersistentTripleStore.Discard()">Discard()</see> method to throw away changes made to the in-memory state.
/// </para>
/// <para>
/// The actual level of persistence provided will vary according to the <see cref="IStorageProvider">IStorageProvider</see> instance you use.  For example if the <see cref="IStorageProvider.DeleteGraph(Uri)">DeleteGraph()</see> method is not supported then Graph removals won't persist in the underlying store.  Similarily an instance which is read-only will allow you to pull out existing graphs from the store but won't persist any changes.
/// </para>
/// <para>
/// The Contains() method of the underlying <see cref="BaseGraphCollection">BaseGraphCollection</see> has been overridden so that invoking Contains causes the Graph from the underlying store to be loaded if it exists, this means that operations like <see cref="BaseTripleStore.HasGraph(Uri)">HasGraph()</see> may be slower than expected or cause applications to stop while they wait to load data from the store.
/// </para>
/// <h3>SPARQL Query Behaviour</h3>
/// <para>
/// The exact SPARQL Query behaviour will depend on the capabilities of the underlying <see cref="IStorageProvider">IStorageProvider</see> instance.  If it also implements the <see cref="IQueryableStorage">IQueryableStorage</see> interface then its own SPARQL implementation will be used, note that if you try and make a SPARQL query but the in-memory view has not been synced (via a <see cref="PersistentTripleStore.Flush()">Flush()</see> or <see cref="PersistentTripleStore.Discard()">Discard()</see> call) prior to the query then an <see cref="RdfQueryException">RdfQueryException</see> will be thrown.  If you want to make the query regardless you can do so by invoking the query method on the underlying store directly by accessing it via the <see cref="PersistentTripleStore.UnderlyingStore">UnderlyingStore</see> property.
/// </para>
/// <para>
/// If the underlying store does not support SPARQL itself then SPARQL queries cannot be applied and a <see cref="NotSupportedException">NotSupportedException</see> will be thrown.
/// </para>
/// <h3>SPARQL Update Behaviour</h3>
/// <para>
/// Similarly to SPARQL Query support the SPARQL Update behaviour depends on whether the underlying <see cref="IStorageProvider">IStorageProvider</see> instance also implements the <see cref="IUpdateableStorage">IUpdateableStorage</see> interface.  If it does then its own SPARQL implementation is used, otherwise a <see cref="GenericUpdateProcessor">GenericUpdateProcessor</see> will be used to approximate the SPARQL Update.
/// </para>
/// <para>
/// Please be aware that as with SPARQL Query if the in-memory view is not synced with the underlying store a <see cref="SparqlUpdateException">SparqlUpdateException</see> will be thrown.
/// </para>
/// <h3>Other Notes</h3>
/// <para>
/// It is possible for the in-memory view of the triple store to get out of sync with the underlying store if that store is being modified by other processes or other code not utilising the <see cref="PersistentTripleStore">PersistentTripleStore</see> instance that you have created.  Currently there is no means to resync the in-memory view with the underlying view so you should be careful of using this class in scenarios where your underlying store may be modified.
/// </para>
/// </remarks>
public sealed class PersistentTripleStore
    : BaseTripleStore, INativelyQueryableStore, IUpdateableTripleStore, ITransactionalStore
{
    private IStorageProvider _manager;
    private SparqlUpdateParser _updateParser;
    private GenericUpdateProcessor _updateProcessor;

    /// <summary>
    /// Creates a new in-memory view of some underlying store represented by the <see cref="IStorageProvider">IStorageProvider</see> instance.
    /// </summary>
    /// <param name="manager">IO Manager.</param>
    /// <remarks>
    /// Please see the remarks for this class for notes on exact behaviour of this class.
    /// </remarks>
    public PersistentTripleStore(IStorageProvider manager)
        : this(manager, RDF.UriFactory.Root)
    {
    }

    /// <summary>
    /// Creates a new in-memory view of some underlying store represented by the <see cref="IStorageProvider">IStorageProvider</see> instance.
    /// </summary>
    /// <param name="manager">IO Manager.</param>
    /// <param name="uriFactory">The preferred URI factory to use when creating URIs for this store.</param>
    /// <remarks>
    /// Please see the remarks for this class for notes on exact behaviour of this class.
    /// </remarks>
    public PersistentTripleStore(IStorageProvider manager, IUriFactory uriFactory) : base(
        new PersistentGraphCollection(manager))
    {
        _manager = manager;
        UriFactory = uriFactory;
    }

    /// <summary>
    /// Finalizer which ensures that the instance is properly disposed of thereby persisting any outstanding changes to the underlying store
    /// </summary>
    /// <remarks>
    /// If you do not wish to persist your changes you must call <see cref="PersistentTripleStore.Discard()">Discard()</see> prior to disposing of this instance or allowing it to go out of scope such that the finalizer gets called
    /// </remarks>
    ~PersistentTripleStore()
    {
        Dispose(false);
    }

    /// <summary>
    /// Get the preferred URI factory to use when creating URIs in this store.
    /// </summary>
    public override  IUriFactory UriFactory { get; }

    /// <summary>
    /// Gets the underlying store.
    /// </summary>
    public IStorageProvider UnderlyingStore
    {
        get
        {
            return _manager;
        }
    }

    /// <summary>
    /// Disposes of the Triple Store flushing any outstanding changes to the underlying store.
    /// </summary>
    /// <remarks>
    /// If you do not want to persist changes you have please ensure you call <see cref="PersistentTripleStore.Discard()">Discard()</see> prior to disposing of the instance.
    /// </remarks>
    public override void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (disposing) GC.SuppressFinalize(this);
        Flush();
    }

    /// <summary>
    /// Flushes any outstanding changes to the underlying store.
    /// </summary>
    public void  Flush()
    {
        if (_graphs != null)
        {
            ((PersistentGraphCollection)_graphs).Flush();
        }
    }

    /// <summary>
    /// Discards any outstanding changes returning the in-memory view of the store to the state it was in after the last Flush/Discard operation.
    /// </summary>
    public void  Discard()
    {
        if (_graphs != null)
        {
            ((PersistentGraphCollection)_graphs).Discard();
        }
    }

    #region INativelyQueryableStore Members

    /// <summary>
    /// Executes a SPARQL Query on the Triple Store.
    /// </summary>
    /// <param name="query">Sparql Query as unparsed String.</param>
    /// <returns></returns>
    public object ExecuteQuery(string query)
    {
        var g = new Graph();
        var results = new SparqlResultSet();
        ExecuteQuery(new GraphHandler(g), new ResultSetHandler(results), query);
        if (results.ResultsType != SparqlResultsType.Unknown)
        {
            return results;
        }
        else
        {
            return g;
        }
    }

    /// <summary>
    /// Executes a SPARQL Query on the Triple Store processing the results using an appropriate handler from those provided.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="query">SPARQL Query as unparsed String.</param>
    public void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string query)
    {
        if (_manager is IQueryableStorage)
        {
            if (!((PersistentGraphCollection)_graphs).IsSynced)
            {
                throw new RdfQueryException("Unable to execute a SPARQL Query as the in-memory view of the store is not synced with the underlying store, please invoked Flush() or Discard() and try again.  Alternatively if you do not want to see in-memory changes reflected in query results you can invoke the Query() method directly on the underlying store by accessing it through the UnderlyingStore property.");
            }

            ((IQueryableStorage)_manager).Query(rdfHandler, resultsHandler, query);
        }
        else
        {
            throw new NotSupportedException("SPARQL Query is not supported as the underlying store does not support it");
        }
    }

    #endregion

    #region IUpdateableTripleStore Members

    /// <summary>
    /// Executes an Update against the Triple Store.
    /// </summary>
    /// <param name="update">SPARQL Update Command(s).</param>
    /// <remarks>
    /// As per the SPARQL 1.1 Update specification the command string may be a sequence of commands.
    /// </remarks>
    public void ExecuteUpdate(string update)
    {
        if (_manager is IUpdateableStorage)
        {
            if (!((PersistentGraphCollection)_graphs).IsSynced)
            {
                throw new SparqlUpdateException("Unable to execute a SPARQL Update as the in-memory view of the store is not synced with the underlying store, please invoked Flush() or Discard() and try again.  Alternatively if you do not want to see in-memory changes reflected in update results you can invoke the Update() method directly on the underlying store by accessing it through the UnderlyingStore property.");
            }

            ((IUpdateableStorage)_manager).Update(update);
        }
        else
        {
            if (_updateProcessor == null) _updateProcessor = new GenericUpdateProcessor(_manager);
            if (_updateParser == null) _updateParser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = _updateParser.ParseFromString(update);
            _updateProcessor.ProcessCommandSet(cmds);
        }
    }

    /// <summary>
    /// Executes a single Update Command against the Triple Store.
    /// </summary>
    /// <param name="update">SPARQL Update Command.</param>
    public void ExecuteUpdate(SparqlUpdateCommand update)
    {
        ExecuteUpdate(update.ToString());
    }

    /// <summary>
    /// Executes a set of Update Commands against the Triple Store.
    /// </summary>
    /// <param name="updates">SPARQL Update Command Set.</param>
    public void ExecuteUpdate(SparqlUpdateCommandSet updates)
    {
        ExecuteUpdate(updates.ToString());
    }

    #endregion
}