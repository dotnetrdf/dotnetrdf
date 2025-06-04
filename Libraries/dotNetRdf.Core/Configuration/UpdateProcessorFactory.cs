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
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF.Configuration;

/// <summary>
/// Factory class for producing SPARQL Update Processors from Configuration Graphs.
/// </summary>
public class UpdateProcessorFactory
    : IObjectFactory
{
    private const string SimpleUpdateProcessor = "VDS.RDF.Update.SimpleUpdateProcessor",
        LeviathanUpdateProcessor = "VDS.RDF.Update.LeviathanUpdateProcessor",
        GenericUpdateProcessor = "VDS.RDF.Update.GenericUpdateProcessor";

    /// <summary>
    /// Tries to load a SPARQL Update based on information from the Configuration Graph.
    /// </summary>
    /// <param name="g">Configuration Graph.</param>
    /// <param name="objNode">Object Node.</param>
    /// <param name="targetType">Target Type.</param>
    /// <param name="obj">Output Object.</param>
    /// <returns></returns>
    public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
    {
        obj = null;
        ISparqlUpdateProcessor processor = null;
        INode storeObj;
        object temp;

        INode propStorageProvider = g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyStorageProvider));

        switch (targetType.FullName)
        {
            case SimpleUpdateProcessor:
                storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingStore)));
                if (storeObj == null) return false;
                temp = ConfigurationLoader.LoadObject(g, storeObj);
                if (temp is IUpdateableTripleStore tripleStore)
                {
                    processor = new SimpleUpdateProcessor(tripleStore);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load the Simple Update Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingStore property points to an Object that cannot be loaded as an object which implements the IUpdateableTripleStore interface");
                }
                break;

            case LeviathanUpdateProcessor:
                INode datasetObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingDataset)));
                if (datasetObj != null)
                {
                    temp = ConfigurationLoader.LoadObject(g, datasetObj);
                    if (temp is ISparqlDataset dataset)
                    {
                        processor = new LeviathanUpdateProcessor(dataset);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Leviathan Update Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingDataset property points to an Object that cannot be loaded as an object which implements the ISparqlDataset interface");
                    }
                }
                else
                {
                    storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingStore)));
                    if (storeObj == null) return false;
                    temp = ConfigurationLoader.LoadObject(g, storeObj);
                    if (temp is IInMemoryQueryableStore store)
                    {
                        processor = new LeviathanUpdateProcessor(store);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Leviathan Update Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingStore property points to an Object that cannot be loaded as an object which implements the IInMemoryQueryableStore interface");
                    }
                }
                break;
            case GenericUpdateProcessor:
                INode managerObj = ConfigurationLoader.GetConfigurationNode(g, objNode, propStorageProvider);
                if (managerObj == null) return false;
                temp = ConfigurationLoader.LoadObject(g, managerObj);
                if (temp is IStorageProvider storageProvider)
                {
                    processor = new GenericUpdateProcessor(storageProvider);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load the Generic Update Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:genericManager property points to an Object that cannot be loaded as an object which implements the IStorageProvider interface");
                }

                break;
        }

        obj = processor;
        return processor != null;
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
            case SimpleUpdateProcessor:
            case LeviathanUpdateProcessor:
            case GenericUpdateProcessor:
                return true;
            default:
                return false;
        }
    }
}
