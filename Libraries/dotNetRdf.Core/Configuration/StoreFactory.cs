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
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace VDS.RDF.Configuration;

/// <summary>
/// Factory class for producing Triple Stores from Configuration Graphs.
/// </summary>
public class StoreFactory
    : IObjectFactory
{
    private const string TripleStore = "VDS.RDF.TripleStore",
                         WebDemandTripleStore = "VDS.RDF.WebDemandTripleStore",
                         PersistentTripleStore = "VDS.RDF.PersistentTripleStore",
                         ThreadSafeTripleStore = "VDS.RDF.ThreadSafeTripleStore";


    /// <summary>
    /// Tries to load a Triple Store based on information from the Configuration Graph.
    /// </summary>
    /// <param name="g">Configuration Graph.</param>
    /// <param name="objNode">Object Node.</param>
    /// <param name="targetType">Target Type.</param>
    /// <param name="obj">Output Object.</param>
    /// <returns></returns>
    public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
    {
        obj = null;

        ITripleStore store = null;
        object temp;

        // Get Property Nodes we need
        INode propStorageProvider = g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyStorageProvider));

        // Check whether to use a specific Graph Collection
        INode collectionNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingGraphCollection)));

        INode uriFactoryNode = ConfigurationLoader.GetConfigurationNode(g, objNode,
            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingUriFactory)));

        // Instantiate the Store Class
        switch (targetType.FullName)
        {
            case TripleStore:
                IUriFactory uriFactory = UriFactory.Root;
                if (uriFactoryNode != null)
                {
                    uriFactory = ConfigurationLoader.LoadObject(g, uriFactoryNode) as IUriFactory;
                }

                if (collectionNode == null)
                {
                    store = new TripleStore(new GraphCollection(), uriFactory);
                }
                else
                {
                    var graphCollection = ConfigurationLoader.LoadObject(g, collectionNode) as BaseGraphCollection;
                    if (graphCollection == null) throw new DotNetRdfConfigurationException("Unable to load the Triple Store identified by the Node '" + objNode.ToString() + "' as the dnr:usingGraphCollection points to an object which cannot be loaded as an instance of the required type BaseGraphCollection");
                    store = new TripleStore(graphCollection, uriFactory);
                }
                break;

            case WebDemandTripleStore:
                store = new WebDemandTripleStore();
                break;

            case PersistentTripleStore:
                INode subObj = ConfigurationLoader.GetConfigurationNode(g, objNode, propStorageProvider);
                if (subObj == null) return false;

                temp = ConfigurationLoader.LoadObject(g, subObj);
                if (temp is IStorageProvider storageProvider)
                {
                    store = new PersistentTripleStore(storageProvider);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load a Persistent Triple Store identified by the Node '" + objNode.ToString() + "' as the value given the for dnr:genericManager property points to an Object which could not be loaded as an object which implements the IStorageProvider interface");
                }
                break;
            
            case ThreadSafeTripleStore:
                if (collectionNode == null)
                {
                    store = new ThreadSafeTripleStore();
                }
                else
                {
                    var graphCollection = ConfigurationLoader.LoadObject(g, collectionNode) as BaseGraphCollection;
                    if (graphCollection == null) throw new DotNetRdfConfigurationException("Unable to load the Triple Store identified by the Node '" + objNode.ToString() + "' as the dnr:usingGraphCollection points to an object which cannot be loaded as an instance of the required type BaseGraphCollection");
                    store = new ThreadSafeTripleStore(graphCollection);
                }
                break;
        }

        // Read in additional data to be added to the Store
        if (store != null)
        {
            IEnumerable<INode> sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingGraph)));

            // Read from Graphs
            foreach (INode source in sources)
            {
                temp = ConfigurationLoader.LoadObject(g, source);
                if (temp is IGraph graph)
                {
                    store.Add(graph);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from a Graph for the Triple Store identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:usingGraph property points to an Object that cannot be loaded as an object which implements the IGraph interface");
                }
            }

            // Load from Embedded Resources
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyFromEmbedded)));
            foreach (INode source in sources)
            {
                if (source.NodeType == NodeType.Literal)
                {
                    EmbeddedResourceLoader.Load(store, ((ILiteralNode)source).Value);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from an Embedded Resource for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:fromEmbedded property is not a Literal Node as required");
                }
            }

            // Read from Files - we assume these files are Dataset Files
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyFromFile)));
            foreach (INode source in sources)
            {
                if (source.NodeType == NodeType.Literal)
                {
                    FileLoader.Load(store, ConfigurationLoader.ResolvePath(((ILiteralNode)source).Value));
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from a file for the Triple Store identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:fromFile property is not a Literal Node as required");
                }
            }

            // And as an absolute final step, if the store is transactional we'll flush any changes we've made
            if (store is ITransactionalStore transactionalStore)
            {
                transactionalStore.Flush();
            }
        }

        obj = store;
        return (store != null);
    }

    /// <summary>
    /// Gets whether this Factory can load objects of the given Type.
    /// </summary>
    /// <param name="t">Type.</param>
    /// <returns></returns>
    public bool CanLoadObject(Type t)
    {
        switch (t.FullName)
        {
            case TripleStore:
            case WebDemandTripleStore:
            case PersistentTripleStore:
            case ThreadSafeTripleStore:
                return true;
            default:
                return false;
        }
    }
}
