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
using System.Reflection;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;

namespace VDS.RDF.Configuration;

/// <summary>
/// Factory class for producing Graphs from Configuration Graphs.
/// </summary>
public class GraphFactory
    : IObjectFactory
{
    /// <summary>
    /// Tries to load a Graph based on information from the Configuration Graph.
    /// </summary>
    /// <param name="g">Configuration Graph.</param>
    /// <param name="objNode">Object Node.</param>
    /// <param name="targetType">Target Type.</param>
    /// <param name="obj">Output Object.</param>
    /// <returns></returns>
    public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
    {
        obj = null;
        IGraph output;

        // Check whether to create with a name
        INode nameNode = ConfigurationLoader.GetConfigurationNode(g, objNode,
            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyWithName)));
        
        // Check whether to use a specific Triple Collection
        INode collectionNode = ConfigurationLoader.GetConfigurationNode(g, objNode, 
            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingTripleCollection)));

        INode nodeFactoryNode = ConfigurationLoader.GetConfigurationNode(g, objNode,
            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingNodeFactory)));

        INode uriFactoryNode = ConfigurationLoader.GetConfigurationNode(g, objNode,
            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingUriFactory)));

        try
        {
            BaseTripleCollection tripleCollection = null;
            if (collectionNode != null)
            {
                // Graph with custom triple collection
                tripleCollection =
                    ConfigurationLoader.LoadObject(g, collectionNode) as BaseTripleCollection;
                if (tripleCollection == null)
                    throw new DotNetRdfConfigurationException(
                        "Unable to load the Graph identified by the Node '" + objNode.ToString() +
                        "' as the dnr:usingTripleCollection points to an object which cannot be loaded as an instance of the required type BaseTripleCollection");
            }

            INodeFactory nodeFactory = null;
            if (nodeFactoryNode != null)
            {
                nodeFactory = ConfigurationLoader.LoadObject(g, nodeFactoryNode) as INodeFactory;
                if (nodeFactory == null)
                {
                    throw new DotNetRdfConfigurationException(
                        $"Unable to load the Graph identified by the node '{objNode.ToString()}'" +
                        "as the dnr:usingNodeFactory property points to an object which cannot be loaded as an instance of the required type INodeFactory.");
                }
            }

            IRefNode graphName = null;
            if (nameNode != null)
            {
                if (nameNode is not IRefNode refNode)
                {
                    throw new DotNetRdfConfigurationException(
                        $"Unable to create named graph identified by the node '{objNode.ToString()}'. Graph name must be either a URI or a blank node.");
                }

                graphName = refNode;
            }

            IUriFactory uriFactory = null;
            if (uriFactoryNode != null)
            {
                uriFactory = ConfigurationLoader.LoadObject(g, uriFactoryNode) as IUriFactory;
                if (uriFactory == null)
                {
                    throw new DotNetRdfConfigurationException(
                        $"Unable to load the Graph identified by the node '{objNode.ToString()}'" +
                        "as the dnr:withUriFactory property points to an object which cannot be loaded as an instance of the required type IUriFactory.");
                }
            }

            output = (IGraph)Activator.CreateInstance(targetType, graphName, nodeFactory, uriFactory,
                tripleCollection, false);
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
        sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyFromGraph)));
        foreach (INode source in sources)
        {
            ConfigurationLoader.CheckCircularReference(objNode, source, "dnr:fromGraph");

            var graph = ConfigurationLoader.LoadObject(g, source);
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
        sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyFromEmbedded)));
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
        sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyFromFile)));
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
        sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyFromString)));
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

        // Load from Stores
        IEnumerable<INode> stores = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyFromStore)));
        stores.All(s => !ConfigurationLoader.CheckCircularReference(objNode, s, "dnr:fromStore"));
        IEnumerable<object> connections = stores.Select(s => ConfigurationLoader.LoadObject(g, s));
        sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyWithUri)));
        foreach (object store in connections)
        {
            if (store is IStorageProvider storageProvider)
            {
                foreach (INode source in sources)
                {
                    if (source.NodeType == NodeType.Uri || source.NodeType == NodeType.Literal)
                    {
                        storageProvider.LoadGraph(output, source.ToString());
                    } 
                    else 
                    {
                        throw new DotNetRdfConfigurationException("Unable to load data from a Generic Store for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:withUri property is not a URI/Literal Node as required");
                    }
                }
            }
            else if (store is ITripleStore tripleStore)
            {
                foreach (INode source in sources)
                {
                    if (source.NodeType == NodeType.Uri)
                    {
                        output.Merge(tripleStore[(IUriNode)source]);
                    }
                    else if (source.NodeType == NodeType.Literal)
                    {
                        output.Merge(tripleStore[new UriNode(g.UriFactory.Create(((ILiteralNode)source).Value))]);
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
        IEnumerable<INode> ds = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyFromDataset)));
        ds.All(d => !ConfigurationLoader.CheckCircularReference(objNode, d, ConfigurationLoader.PropertyFromDataset));
        IEnumerable<object> datasets = ds.Select(d => ConfigurationLoader.LoadObject(g, d));
        sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyWithUri)));
        foreach (object dataset in datasets)
        {
            if (dataset is ISparqlDataset sparqlDataset)
            {
                foreach (INode source in sources)
                {
                    switch (source.NodeType)
                    {
                        case NodeType.Uri:
                            output.Merge(sparqlDataset[(IUriNode)sources]);
                            break;
                        case NodeType.Literal:
                            output.Merge(sparqlDataset[new UriNode(g.UriFactory.Create(((ILiteralNode)source).Value))]);
                            break;
                        default:
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
        sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyFromUri)));
        foreach (INode source in sources)
        {
            switch (source.NodeType)
            {
                case NodeType.Uri:
                    ConfigurationLoader.Loader.LoadGraph(output, ((IUriNode)source).Uri);
                    break;
                case NodeType.Literal:
                    ConfigurationLoader.Loader.LoadGraph(output, g.UriFactory.Create(((ILiteralNode) source).Value));
                    break;
                default:
                    throw new DotNetRdfConfigurationException("Unable to load data from a URI for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:fromUri property is not a URI/Literal Node as required");
            }
        }
        
        // Then are we assigning a Base URI to this Graph which overrides any existing Base URI?
        INode baseUri = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyAssignUri)));
        if (baseUri != null)
        {
            switch (baseUri.NodeType)
            {
                case NodeType.Uri:
                    output.BaseUri = ((IUriNode)baseUri).Uri;
                    break;
                case NodeType.Literal:
                    output.BaseUri = g.UriFactory.Create(((ILiteralNode)baseUri).Value);
                    break;
                default:
                    throw new DotNetRdfConfigurationException("Unable to assign a new Base URI for the Graph identified by the Node '" + objNode.ToString() + "' as the value for the dnr:assignUri property is not a URI/Literal Node as required");
            }
        }

        // Finally we'll apply any reasoners
        /* TODO: Find a way to support this in the new structure - may need some post processing or factory extension
        IEnumerable<INode> reasoners = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyReasoner)));
        foreach (INode reasoner in reasoners)
        {
            object temp = ConfigurationLoader.LoadObject(g, reasoner);
            if (temp is IInferenceEngine inferenceEngine)
            {
                inferenceEngine.Apply(output);
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to apply a reasoner for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:reasoner property points to an Object which cannot be loaded as an object which implements the IInferenceEngine interface");
            }
        }
        */

        obj = output;
        return true;
    }

    /// <summary>
    /// Gets whether this Factory can load objects of the given Type.
    /// </summary>
    /// <param name="t">Type.</param>
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
