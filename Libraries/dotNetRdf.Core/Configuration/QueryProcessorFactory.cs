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
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;

namespace VDS.RDF.Configuration;

/// <summary>
/// Factory class for producing SPARQL Query Processors from Configuration Graphs.
/// </summary>
public class QueryProcessorFactory : IObjectFactory
{
    private const string SimpleQueryProcessor = "VDS.RDF.Query.SimpleQueryProcessor",
                         GenericQueryProcessor = "VDS.RDF.Query.GenericQueryProcessor",
                         RemoteQueryProcessor = "VDS.RDF.Query.RemoteQueryProcessor",
                         LeviathanQueryProcessor = "VDS.RDF.Query.LeviathanQueryProcessor";

    /// <summary>
    /// Tries to load a SPARQL Query Processor based on information from the Configuration Graph.
    /// </summary>
    /// <param name="g">Configuration Graph.</param>
    /// <param name="objNode">Object Node.</param>
    /// <param name="targetType">Target Type.</param>
    /// <param name="obj">Output Object.</param>
    /// <returns></returns>
    public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
    {
        obj = null;
        ISparqlQueryProcessor processor = null;
        INode storeObj;
        object temp;

        INode propStorageProvider = g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyStorageProvider));

        switch (targetType.FullName)
        {
            case SimpleQueryProcessor:
                storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingStore)));
                if (storeObj == null) return false;
                temp = ConfigurationLoader.LoadObject(g, storeObj);
                if (temp is INativelyQueryableStore)
                {
                    processor = new SimpleQueryProcessor((INativelyQueryableStore)temp);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load the Simple Query Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingStore property points to an Object that cannot be loaded as an object which implements the INativelyQueryableStore interface");
                }
                break;

            case GenericQueryProcessor:
                INode managerObj = ConfigurationLoader.GetConfigurationNode(g, objNode, propStorageProvider);
                if (managerObj == null) return false;
                temp = ConfigurationLoader.LoadObject(g, managerObj);
                if (temp is IQueryableStorage)
                {
                    processor = new GenericQueryProcessor((IQueryableStorage)temp);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load the Generic Query Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:genericManager property points to an Object that cannot be loaded as an object which implements the IQueryableStorage interface");
                }
                break;

            case RemoteQueryProcessor:
                INode endpointObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyEndpoint)));
                if (endpointObj == null) return false;
                temp = ConfigurationLoader.LoadObject(g, endpointObj);
#pragma warning disable 618
                if (temp is SparqlRemoteEndpoint queryEndpoint)
                {
                    processor = new RemoteQueryProcessor(queryEndpoint);
#pragma warning restore 618
                }
                else if (temp is SparqlQueryClient queryClient)
                {
                    processor = new RemoteQueryProcessor(queryClient);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load the Remote Query Processor identified by the Node '" + objNode.ToSafeString() + "' as the value given for the dnr:endpoint property points to an Object that cannot be loaded as an object which is a SparqlRemoteEndpoint");
                }
                break;

            case LeviathanQueryProcessor:
                INode datasetObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingDataset)));
                if (datasetObj != null)
                {
                    temp = ConfigurationLoader.LoadObject(g, datasetObj);
                    if (temp is ISparqlDataset dataset)
                    {
                        processor = new LeviathanQueryProcessor(dataset);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Leviathan Query Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingDataset property points to an Object that cannot be loaded as an object which implements the ISparqlDataset interface");
                    }
                }
                else
                {
                    // If no dnr:usingDataset try dnr:usingStore instead
                    storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingStore)));
                    if (storeObj == null) return false;
                    temp = ConfigurationLoader.LoadObject(g, storeObj);
                    if (temp is IInMemoryQueryableStore store)
                    {
                        processor = new LeviathanQueryProcessor(store);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Leviathan Query Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingStore property points to an Object that cannot be loaded as an object which implements the IInMemoryQueryableStore interface");
                    }
                }
                break;
        }

        obj = processor;
        return (processor != null);
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
            case SimpleQueryProcessor:
            case GenericQueryProcessor:
            case RemoteQueryProcessor:
            case LeviathanQueryProcessor:
                return true;
            default:
                return false;
        }
    }
}
