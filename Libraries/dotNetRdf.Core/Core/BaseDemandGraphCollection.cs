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
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF;

/// <summary>
/// A decorator for graph collections that allows for graphs to be loaded on demand if they don't exist in the underlying graph collection.
/// </summary>
public abstract class BaseDemandGraphCollection
    : WrapperGraphCollection
{
    /// <summary>
    /// Creates a new decorator.
    /// </summary>
    protected BaseDemandGraphCollection()
        : base() { }

    /// <summary>
    /// Creates a new decorator over the given graph collection.
    /// </summary>
    /// <param name="collection">Graph Collection.</param>
    protected BaseDemandGraphCollection(BaseGraphCollection collection)
        : base(collection) { }

    /// <summary>
    /// Checks whether the collection contains a Graph invoking an on-demand load if not present in the underlying collection.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete]
    public override bool Contains(Uri graphUri)
    {
        try
        {
            if (base.Contains(graphUri))
            {
                return true;
            }
            else
            {
                // Try to do on-demand loading
                IGraph g = LoadOnDemand(graphUri);

                // Remember to set the Graph URI to the URI being asked for prior to adding it to the underlying collection
                // in case the loading process sets it otherwise
                g.BaseUri = graphUri;
                Add(g, false);
                return true;
            }
        }
        catch
        {
            // Any errors in checking if the Graph already exists or loading it on-demand leads to a return of false
            return false;
        }
    }

    /// <summary>
    /// Checks whether the graph with the given name exists in this graph collection.
    /// </summary>
    /// <param name="graphName">Graph name to test for.</param>
    /// <returns>True if a graph with the specified name is in the collection, false otherwise.</returns>
    /// <remarks>The null value is used to reference the default (unnamed) graph.</remarks>
    public override bool Contains(IRefNode graphName)
    {
        try
        {
            if (base.Contains(graphName))
            {
                return true;
            }

            if (graphName is IUriNode uriNode)
            {
                // Try to do on-demand loading
                IGraph g = LoadOnDemand(uriNode.Uri);

                // Remember to set the Graph URI to the URI being asked for prior to adding it to the underlying collection
                // in case the loading process sets it otherwise
                g.BaseUri = uriNode.Uri;
                Add(g, false);
                return true;
            }

            return false;
        }
        catch
        {
            // Any errors in checking if the Graph already exists or loading it on-demand leads to a return of false
            return false;
        }
    }

    /// <summary>
    /// Loads a Graph on demand.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <returns>A Graph if it could be loaded and throws an error otherwise.</returns>
    protected abstract IGraph LoadOnDemand(Uri graphUri);
}

/// <summary>
/// A decorator for graph collections where graphs not in the underlying graph collection can be loaded on-demand from the Web as needed.
/// </summary>
public class WebDemandGraphCollection
    : BaseDemandGraphCollection
{
    /// <summary>
    /// Creates a new Web Demand Graph Collection which loads Graphs from the Web on demand.
    /// </summary>
    public WebDemandGraphCollection() { }

    /// <summary>
    /// Creates a new Web Demand Graph Collection which loads Graphs from the Web on demand.
    /// </summary>
    /// <param name="collection">Collection to decorate.</param>
    public WebDemandGraphCollection(BaseGraphCollection collection)
        : base(collection) { }

    /// <summary>
    /// Tries to load a Graph on demand from a URI.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    protected override IGraph LoadOnDemand(Uri graphUri)
    {
        if (graphUri != null)
        {
            try
            {
                var g = new Graph(new UriNode(graphUri));
                ConfigurationLoader.Loader.LoadGraph(g, graphUri);
                return g;
            }
            catch
            {
                throw new RdfException("The Graph with the URI " + graphUri.AbsoluteUri + " does not exist in this collection");
            }
        }
        else
        {
            throw new RdfException("The Graph with the URI does not exist in this collection");
        }
    }
}

/// <summary>
/// A decorator for graph collection where graphs not in the underlying graph collection can be loaded on-demand from the Files on Disk as needed.
/// </summary>
public class DiskDemandGraphCollection
    : BaseDemandGraphCollection
{
    private readonly Loader _loader;

    /// <summary>
    /// Creates a new Disk Demand Graph Collection which loads Graphs from the Web on demand.
    /// </summary>
    public DiskDemandGraphCollection()
        : base()
    {
        _loader = new Loader();
    }

    /// <summary>
    /// Creates a new Disk Demand Graph Collection.
    /// </summary>
    /// <param name="collection">Collection to decorate.</param>
    public DiskDemandGraphCollection(BaseGraphCollection collection)
        : base(collection)
    {
        _loader = new Loader();
    }

    /// <summary>
    /// Tries to load a Graph on demand.
    /// </summary>
    /// <param name="graphUri"></param>
    /// <returns></returns>
    protected override IGraph LoadOnDemand(Uri graphUri)
    {
        if (graphUri == null) throw new RdfException("The Graph with the given URI does not exist in this collection");
        if (graphUri.IsFile)
        {
            try
            {
                var g = new Graph(new UriNode(graphUri));
                _loader.LoadGraph(g, graphUri);
                return g;
            }
            catch
            {
                throw new RdfException("The Graph with the URI " + graphUri.AbsoluteUri + " does not exist in this collection");
            }
        }
        else
        {
            throw new RdfException("The Graph with the URI " + graphUri.AbsoluteUri + " does not exist in this collection");
        }
    }
}
