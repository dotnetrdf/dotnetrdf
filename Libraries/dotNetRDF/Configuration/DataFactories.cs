/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Reflection;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Inference;
using VDS.RDF.Storage;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Factory class for producing Graphs from Configuration Graphs
    /// </summary>
    public class GraphFactory
        : IObjectFactory
    {
        /// <summary>
        /// Tries to load a Graph based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            IGraph output;

            // Check whether to use a specific Triple Collection
            INode collectionNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingTripleCollection)));

            try
            {
                if (collectionNode == null)
                {
                    // Simple Graph creation
                    output = (IGraph)Activator.CreateInstance(targetType);
                }
                else
                {
                    // Graph with custom triple collection
                    BaseTripleCollection tripleCollection = ConfigurationLoader.LoadObject(g, collectionNode) as BaseTripleCollection;
                    if (tripleCollection == null) throw new DotNetRdfConfigurationException("Unable to load the Graph identified by the Node '" + objNode.ToString() + "' as the dnr:usingTripleCollection points to an object which cannot be loaded as an instance of the required type BaseTripleCollection");
                    output = (IGraph)Activator.CreateInstance(targetType, new Object[] { tripleCollection });
                }
            }
            catch
            {
                // Any error means this loader can't load this type
                return false;
            }

            // Now we want to find out where the data for the Graph is coming from
            // Data Source loading order is Graphs, Files, Strings, Databases, Stores, URIs
            IEnumerable<INode> sources;

            // Load from Graphs
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyFromGraph)));
            foreach (INode source in sources)
            {
                ConfigurationLoader.CheckCircularReference(objNode, source, "dnr:fromGraph");

                Object graph = ConfigurationLoader.LoadObject(g, source);
                if (graph is IGraph)
                {
                    output.Merge((IGraph)graph);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from another Graph for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:fromGraph property points to an Object that cannot be loaded as an object which implements the IGraph interface");
                }
            }

            // Load from Embedded Resources
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyFromEmbedded)));
            foreach (INode source in sources)
            {
                if (source.NodeType == NodeType.Literal)
                {
                    EmbeddedResourceLoader.Load(output, ((ILiteralNode)source).Value);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from an Embedded Resource for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:fromEmbedded property is not a Literal Node as required");
                }
            }
            
            // Load from Files
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyFromFile)));
            foreach (INode source in sources)
            {
                if (source.NodeType == NodeType.Literal)
                {
                    FileLoader.Load(output, ConfigurationLoader.ResolvePath(((ILiteralNode)source).Value));
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from a file for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:fromFile property is not a Literal Node as required");
                }
            }

            // Load from Strings
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyFromString)));
            foreach (INode source in sources)
            {
                if (source.NodeType == NodeType.Literal)
                {
                    StringParser.Parse(output, ((ILiteralNode)source).Value);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from a string for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:fromString property is not a Literal Node as required");
                }
            }

            IEnumerable<Object> connections;

            // Load from Stores
            IEnumerable<INode> stores = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyFromStore)));
            stores.All(s => !ConfigurationLoader.CheckCircularReference(objNode, s, "dnr:fromStore"));
            connections = stores.Select(s => ConfigurationLoader.LoadObject(g, s));
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyWithUri)));
            foreach (Object store in connections)
            {
                if (store is IStorageProvider)
                {
                    foreach (INode source in sources)
                    {
                        if (source.NodeType == NodeType.Uri || source.NodeType == NodeType.Literal)
                        {
                            ((IStorageProvider)store).LoadGraph(output, source.ToString());
                        } 
                        else 
                        {
                            throw new DotNetRdfConfigurationException("Unable to load data from a Generic Store for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:withUri property is not a URI/Literal Node as required");
                        }
                    }
                }
                else if (store is ITripleStore)
                {
                    foreach (INode source in sources)
                    {
                        if (source.NodeType == NodeType.Uri)
                        {
                            output.Merge(((ITripleStore)store)[((IUriNode)source).Uri]);
                        }
                        else if (source.NodeType == NodeType.Literal)
                        {
                            output.Merge(((ITripleStore)store)[UriFactory.Create(((ILiteralNode)source).Value)]);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load data from a Store for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:withUri property is not a URI/Literal Node as required");
                        }
                    }
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from a Store for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values of the dnr:fromStore property points to an Object which cannot be loaded as an object which implements either the IStorageProvider/ITripleStore interface");
                }
            }

            // Load from Datasets
            IEnumerable<INode> ds = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyFromDataset)));
            ds.All(d => !ConfigurationLoader.CheckCircularReference(objNode, d, ConfigurationLoader.PropertyFromDataset));
            IEnumerable<Object> datasets = ds.Select(d => ConfigurationLoader.LoadObject(g, d));
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyWithUri)));
            foreach (Object dataset in datasets)
            {
                if (dataset is ISparqlDataset)
                {
                    foreach (INode source in sources)
                    {
                        if (source.NodeType == NodeType.Uri)
                        {
                            output.Merge(((ISparqlDataset)dataset)[((IUriNode)sources).Uri]);
                        }
                        else if (source.NodeType == NodeType.Literal)
                        {
                            output.Merge(((ISparqlDataset)dataset)[UriFactory.Create(((ILiteralNode)source).Value)]);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load data from a Dataset for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:withUri property is not a URI/Literal Node as required");
                        }
                    }
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from a Dataset for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values of the dnr:fromDataset property points to an Object which cannot be loaded as an object which implements the required ISparqlDataset interface");
                }
            }


            // Finally load from Remote URIs
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyFromUri)));
            foreach (INode source in sources)
            {
                if (source.NodeType == NodeType.Uri)
                {
                    UriLoader.Load(output, ((IUriNode)source).Uri);
                }
                else if (source.NodeType == NodeType.Literal)
                {
                    UriLoader.Load(output, UriFactory.Create(((ILiteralNode)source).Value));
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from a URI for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:fromUri property is not a URI/Literal Node as required");
                }
            }
            
            // Then are we assigning a Base URI to this Graph which overrides any existing Base URI?
            INode baseUri = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyAssignUri)));
            if (baseUri != null)
            {
                if (baseUri.NodeType == NodeType.Uri)
                {
                    output.BaseUri = ((IUriNode)baseUri).Uri;
                }
                else if (baseUri.NodeType == NodeType.Literal)
                {
                    output.BaseUri = UriFactory.Create(((ILiteralNode)baseUri).Value);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to assign a new Base URI for the Graph identified by the Node '" + objNode.ToString() + "' as the value for the dnr:assignUri property is not a URI/Literal Node as required");
                }
            }

            // Finally we'll apply any reasoners
            IEnumerable<INode> reasoners = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyReasoner)));
            foreach (INode reasoner in reasoners)
            {
                Object temp = ConfigurationLoader.LoadObject(g, reasoner);
                if (temp is IInferenceEngine)
                {
                    ((IInferenceEngine)temp).Apply(output);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to apply a reasoner for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:reasoner property points to an Object which cannot be loaded as an object which implements the IInferenceEngine interface");
                }
            }

            obj = output;
            return true;
        }

        /// <summary>
        /// Gets whether this Factory can load objects of the given Type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            Type igraph = typeof(IGraph);
            
            // We can load any object which implements IGraph and has a public unparameterized constructor
            if (t.GetInterfaces().Any(i => i.Equals(igraph)))
            {
                ConstructorInfo c = t.GetConstructor(new Type[0]);
                if (c != null)
                {
                    return c.IsPublic;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Factory class for producing Triple Stores from Configuration Graphs
    /// </summary>
    public class StoreFactory 
        : IObjectFactory
    {
        private const String TripleStore = "VDS.RDF.TripleStore",
                             WebDemandTripleStore = "VDS.RDF.WebDemandTripleStore",
                             PersistentTripleStore = "VDS.RDF.PersistentTripleStore";


        /// <summary>
        /// Tries to load a Triple Store based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;

            ITripleStore store = null;
            INode subObj;
            Object temp;

            // Get Property Nodes we need
            INode propStorageProvider = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyStorageProvider)),
                  propAsync = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyAsync));

            // Check whether to use a specific Graph Collection
            INode collectionNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingGraphCollection)));

            // Instantiate the Store Class
            switch (targetType.FullName)
            {
                case TripleStore:
                    if (collectionNode == null)
                    {
                        store = new TripleStore();
                    }
                    else
                    {
                        BaseGraphCollection graphCollection = ConfigurationLoader.LoadObject(g, collectionNode) as BaseGraphCollection;
                        if (graphCollection == null) throw new DotNetRdfConfigurationException("Unable to load the Triple Store identified by the Node '" + objNode.ToString() + "' as the dnr:usingGraphCollection points to an object which cannot be loaded as an instance of the required type BaseGraphCollection");
                        store = new TripleStore(graphCollection);
                    }
                    break;

                case WebDemandTripleStore:
                    store = new WebDemandTripleStore();
                    break;

                case PersistentTripleStore:
                    subObj = ConfigurationLoader.GetConfigurationNode(g, objNode, propStorageProvider);
                    if (subObj == null) return false;

                    temp = ConfigurationLoader.LoadObject(g, subObj);
                    if (temp is IStorageProvider)
                    {
                        store = new PersistentTripleStore((IStorageProvider)temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load a Persistent Triple Store identified by the Node '" + objNode.ToString() + "' as the value given the for dnr:genericManager property points to an Object which could not be loaded as an object which implements the IStorageProvider interface");
                    }
                    break;
            }
            
            // Read in additional data to be added to the Store
            if (store != null)
            {
                IEnumerable<INode> sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingGraph)));

                // Read from Graphs
                foreach (INode source in sources)
                {
                    temp = ConfigurationLoader.LoadObject(g, source);
                    if (temp is IGraph)
                    {
                        store.Add((IGraph)temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load data from a Graph for the Triple Store identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:usingGraph property points to an Object that cannot be loaded as an object which implements the IGraph interface");
                    }
                }

                // Load from Embedded Resources
                sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyFromEmbedded)));
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
                sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyFromFile)));
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

                // Finally we'll apply any reasoners
                if (store is IInferencingTripleStore)
                {
                    IEnumerable<INode> reasoners = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyReasoner)));
                    foreach (INode reasoner in reasoners)
                    {
                        temp = ConfigurationLoader.LoadObject(g, reasoner);
                        if (temp is IInferenceEngine)
                        {
                            ((IInferencingTripleStore)store).AddInferenceEngine((IInferenceEngine)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to apply a reasoner for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:reasoner property points to an Object which cannot be loaded as an object which implements the IInferenceEngine interface");
                        }
                    }
                }

                // And as an absolute final step if the store is transactional we'll flush any changes we've made
                if (store is ITransactionalStore)
                {
                    ((ITransactionalStore)store).Flush();
                }
            }

            obj = store;
            return (store != null);
        }

        /// <summary>
        /// Gets whether this Factory can load objects of the given Type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            switch (t.FullName)
            {
                case TripleStore:
                case WebDemandTripleStore:
                case PersistentTripleStore:
                    return true;
                default:
                    return false;
            }
        }
    }
}
