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
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Management;

namespace VDS.RDF.Storage;

/// <summary>
/// Provides a Read-Only wrapper that can be placed around another <see cref="IStorageProvider">IStorageProvider</see> instance.
/// </summary>
/// <remarks>
/// <para>
/// This is useful if you want to allow some code read-only access to a mutable store and ensure that it cannot modify the store via the manager instance.
/// </para>
/// </remarks>
public class ReadOnlyConnector 
    : IStorageProvider, IConfigurationSerializable
{
    private readonly IStorageProvider _manager;

    /// <summary>
    /// Creates a new Read-Only connection which is a read-only wrapper around another store.
    /// </summary>
    /// <param name="manager">Manager for the Store you want to wrap as read-only.</param>
    public ReadOnlyConnector(IStorageProvider manager)
    {
        _manager = manager;
    }

    #region IStorageProvider Members

    /// <summary>
    /// Gets the parent server (if any).
    /// </summary>
    public virtual IStorageServer ParentServer
    {
        get
        {
            return _manager.ParentServer;
        }
    }

    /// <summary>
    /// Loads a Graph from the underlying Store.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    public void LoadGraph(IGraph g, Uri graphUri)
    {
        _manager.LoadGraph(g, graphUri);
    }

    /// <summary>
    /// Loads a Graph from the underlying Store.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    public void LoadGraph(IGraph g, string graphUri)
    {
        _manager.LoadGraph(g, graphUri);
    }

    /// <summary>
    /// Loads a Graph from the underlying Store.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    public void LoadGraph(IRdfHandler handler, Uri graphUri)
    {
        _manager.LoadGraph(handler, graphUri);
    }

    /// <summary>
    /// Loads a Graph from the underlying Store.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    public void LoadGraph(IRdfHandler handler, string graphUri)
    {
        _manager.LoadGraph(handler, graphUri);
    }

    /// <summary>
    /// Throws an exception since you cannot save a Graph using a read-only connection.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <exception cref="RdfStorageException">Thrown since you cannot save a Graph using a read-only connection.</exception>
    public void SaveGraph(IGraph g)
    {
        throw new RdfStorageException("The Read-Only Connector is a read-only connection");
    }

    /// <summary>
    /// Gets the IO Behaviour of the read-only connection taking into account the IO Behaviour of the underlying store.
    /// </summary>
    public IOBehaviour IOBehaviour
    {
        get
        {
            return (_manager.IOBehaviour | IOBehaviour.IsReadOnly) & (IOBehaviour.HasDefaultGraph | IOBehaviour.HasDefaultGraph | IOBehaviour.IsQuadStore | IOBehaviour.IsTripleStore | IOBehaviour.IsReadOnly);
        }
    }

    /// <summary>
    /// Throws an exception since you cannot update a Graph using a read-only connection.
    /// </summary>
    /// <param name="graphUri">URI of the Graph.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    /// <exception cref="RdfStorageException">Thrown since you cannot update a Graph using a read-only connection.</exception>
    public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        throw new RdfStorageException("The Read-Only Connector is a read-only connection");
    }

    /// <summary>
    /// Throws an exception since you cannot update a Graph using a read-only connection.
    /// </summary>
    /// <param name="graphUri">URI of the Graph.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    /// <exception cref="RdfStorageException">Thrown since you cannot update a Graph using a read-only connection.</exception>
    public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        throw new RdfStorageException("The Read-Only Connector is a read-only connection");
    }

    /// <summary>
    /// Throws an exception since you cannot update a Graph using a read-only connection.
    /// </summary>
    /// <param name="graphName">Name of the Graph.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    /// <exception cref="RdfStorageException">Thrown since you cannot update a Graph using a read-only connection.</exception>
    public void UpdateGraph(IRefNode graphName, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        throw new RdfStorageException("The Read-Only Connector is a read-only connection");
    }

    /// <summary>
    /// Returns that Update is not supported.
    /// </summary>
    public bool UpdateSupported
    {
        get 
        {
            return false; 
        }
    }

    /// <summary>
    /// Throws an exception as you cannot delete a Graph using a read-only connection.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to delete.</param>
    /// <exception cref="RdfStorageException">Thrown since you cannot delete a Graph using a read-only connection.</exception>
    public void DeleteGraph(Uri graphUri)
    {
        throw new RdfStorageException("The Read-Only Connector is a read-only connection");
    }

    /// <summary>
    /// Throws an exception as you cannot delete a Graph using a read-only connection.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to delete.</param>
    /// <exception cref="RdfStorageException">Thrown since you cannot delete a Graph using a read-only connection.</exception>
    public void DeleteGraph(string graphUri)
    {
        throw new RdfStorageException("The Read-Only Connector is a read-only connection");
    }

    
    /// <summary>
    /// Returns that deleting graphs is not supported.
    /// </summary>
    public bool DeleteSupported
    {
        get 
        {
            return false; 
        }
    }

    /// <summary>
    /// Gets the list of graphs in the underlying store.
    /// </summary>
    /// <returns></returns>
    [Obsolete("Replaced by ListGraphNames")]
    public virtual IEnumerable<Uri> ListGraphs()
    {
        return _manager.ListGraphs();
    }

    /// <summary>
    /// Gets an enumeration of the names of the graphs in the store.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Implementations should implement this method only if they need to provide a custom way of listing Graphs.  If the Store for which you are providing a manager can efficiently return the Graphs using a SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } } query then there should be no need to implement this function.
    /// </para>
    /// </remarks>
    public virtual IEnumerable<string> ListGraphNames()
    {
        return _manager.ListGraphNames();
    }

    /// <summary>
    /// Returns whether listing graphs is supported by the underlying store.
    /// </summary>
    public virtual bool ListGraphsSupported
    {
        get
        {
            return _manager.ListGraphsSupported;
        }
    }

    /// <summary>
    /// Returns whether the Store is ready.
    /// </summary>
    public bool IsReady
    {
        get 
        {
            return _manager.IsReady; 
        }

    }

    /// <summary>
    /// Returns that the Store is read-only.
    /// </summary>
    public bool IsReadOnly
    {
        get 
        {
            return true; 
        }
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Disposes of the Store.
    /// </summary>
    public void Dispose()
    {
        _manager.Dispose();
    }

    #endregion

    /// <summary>
    /// Gets the String representation of the Manager.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "[Read Only]" + _manager;
    }

    /// <summary>
    /// Serializes the Configuration of the Manager.
    /// </summary>
    /// <param name="context">Configuration Serialization Context.</param>
    public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        INode manager = context.NextSubject;
        INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
        INode rdfsLabel = context.Graph.CreateUriNode(context.UriFactory.Create(NamespaceMapper.RDFS + "label"));
        INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
        INode storageProvider = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyStorageProvider));

        context.Graph.Assert(manager, rdfType, context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassStorageProvider)));
        context.Graph.Assert(manager, dnrType, context.Graph.CreateLiteralNode(GetType().ToString()));
        context.Graph.Assert(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString()));

        if (_manager is IConfigurationSerializable serializable)
        {
            INode managerObj = context.Graph.CreateBlankNode();
            context.NextSubject = managerObj;
            serializable.SerializeConfiguration(context);
            context.Graph.Assert(manager, storageProvider, managerObj);
        }
        else
        {
            throw new DotNetRdfConfigurationException("Unable to serialize configuration as the underlying IStorageProvider is not serializable");
        }
    }
}