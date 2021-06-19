/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Net;
using System.Net.Http;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;
using VDS.RDF.Update;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Factory class for producing <see cref="IStorageProvider">IStorageProvider</see> and <see cref="IStorageServer"/> instances from Configuration Graphs.
    /// </summary>
    public class StorageFactory
        : IObjectFactory
    {
        private const string DatasetFile = "VDS.RDF.Storage.DatasetFileManager",
                             InMemory = "VDS.RDF.Storage.InMemoryManager";

        /// <summary>
        /// Tries to load a Generic IO Manager based on information from the Configuration Graph.
        /// </summary>
        /// <param name="g">Configuration Graph.</param>
        /// <param name="objNode">Object Node.</param>
        /// <param name="targetType">Target Type.</param>
        /// <param name="obj">Output Object.</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            IStorageProvider storageProvider = null;
            obj = null;
            object temp;

            // Create the URI Nodes we're going to use to search for things
            INode propAsync = g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyAsync));

            switch (targetType.FullName)
            {
                
                case DatasetFile:
                    // Get the Filename and whether the loading should be done asynchronously
                    var file = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyFromFile)));
                    if (file == null) return false;
                    file = ConfigurationLoader.ResolvePath(file);
                    var isAsync = ConfigurationLoader.GetConfigurationBoolean(g, objNode, propAsync, false);
                    storageProvider = new DatasetFileManager(file, isAsync);
                    break;

                case InMemory:
                    // Get the Dataset/Store
                    INode datasetObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingDataset)));
                    if (datasetObj != null)
                    {
                        temp = ConfigurationLoader.LoadObject(g, datasetObj);
                        if (temp is ISparqlDataset)
                        {
                            storageProvider = new InMemoryManager((ISparqlDataset) temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the In-Memory Manager identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingDataset property points to an Object that cannot be loaded as an object which implements the ISparqlDataset interface");
                        }
                    }
                    else
                    {
                        // If no dnr:usingDataset try dnr:usingStore instead
                        INode storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingStore)));
                        if (storeObj != null)
                        {
                            temp = ConfigurationLoader.LoadObject(g, storeObj);
                            if (temp is IInMemoryQueryableStore)
                            {
                                storageProvider = new InMemoryManager((IInMemoryQueryableStore) temp);
                            }
                            else
                            {
                                throw new DotNetRdfConfigurationException("Unable to load the In-Memory Manager identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingStore property points to an Object that cannot be loaded as an object which implements the IInMemoryQueryableStore interface");
                            }
                        }
                        else
                        {
                            // If no dnr:usingStore either then create a new empty store
                            storageProvider = new InMemoryManager();
                        }
                    }
                    break;


            }
            

            // Set the return object if one has been loaded
            if (storageProvider != null)
            {
                obj = storageProvider;
            }

            return obj != null;

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
                case DatasetFile:
                case InMemory:
                    return true;
                default:
                    return false;
            }
        }
    }
}