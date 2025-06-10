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
using VDS.RDF.Query.Inference;

namespace VDS.RDF.Configuration;

/// <summary>
/// An configuration object factory for stores that support simple inferencing.
/// </summary>
public class InferencingStoreFactory : IObjectFactory
{

    private const string InferencingTripleStore = "VDS.RDF.InferencingTripleStore";

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

        // Get Property Nodes we need
        g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyAsync));

        // Check whether to use a specific Graph Collection
        INode collectionNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingGraphCollection)));

        // Instantiate the Store Class
        switch (targetType.FullName)
        {
            case InferencingTripleStore:
                if (collectionNode == null)
                {
                    store = new TripleStore();
                }
                else
                {
                    if (ConfigurationLoader.LoadObject(g,
                            collectionNode) is not BaseGraphCollection graphCollection)
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Triple Store identified by the Node '" + objNode.ToString() + "' as the dnr:usingGraphCollection points to an object which cannot be loaded as an instance of the required type BaseGraphCollection");
                    }
                    store = new TripleStore(graphCollection);
                }
                break;
        }

        // Read in additional data to be added to the Store
        if (store != null)
        {
            IEnumerable<INode> sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingGraph)));

            // Read from Graphs
            object temp;
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

            // Finally we'll apply any reasoners
            if (store is IInferencingTripleStore inferencingTripleStore)
            {
                IEnumerable<INode> reasoners = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyReasoner)));
                foreach (INode reasoner in reasoners)
                {
                    temp = ConfigurationLoader.LoadObject(g, reasoner);
                    if (temp is IInferenceEngine inferenceEngine)
                    {
                        inferencingTripleStore.AddInferenceEngine(inferenceEngine);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to apply a reasoner for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:reasoner property points to an Object which cannot be loaded as an object which implements the IInferenceEngine interface");
                    }
                }
            }

            // And as an absolute final step if the store is transactional we'll flush any changes we've made
            if (store is ITransactionalStore transactionalStore)
            {
                transactionalStore.Flush();
            }
        }

        obj = store;
        return store != null;
    }

    /// <summary>
    /// Gets whether this Factory can load objects of the given Type.
    /// </summary>
    /// <param name="t">Type.</param>
    /// <returns></returns>
    public bool CanLoadObject(Type t)
    {
        return t.FullName switch
        {
            InferencingTripleStore => true,
            _ => false
        };
    }
}
