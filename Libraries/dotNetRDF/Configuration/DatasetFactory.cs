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
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// An Object Factory for creating SPARQL Datasets
    /// </summary>
    public class DatasetFactory 
        : IObjectFactory
    {
        private const String InMemoryDataset = "VDS.RDF.Query.Datasets.InMemoryDataset",
                             InMemoryQuadDataset = "VDS.RDF.Query.Datasets.InMemoryQuadDataset",
                             WebDemandDataset = "VDS.RDF.Query.Datasets.WebDemandDataset";

        /// <summary>
        /// Tries to load a SPARQL Dataset based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            bool unionDefGraph = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUnionDefaultGraph)), false);
            INode defaultGraphNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyDefaultGraphUri)));
            Uri defaultGraph = (defaultGraphNode != null && defaultGraphNode.NodeType == NodeType.Uri ? ((IUriNode)defaultGraphNode).Uri : null);

            INode storeNode;
            switch (targetType.FullName)
            {
                case InMemoryDataset:
                    storeNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingStore)));
                    if (storeNode == null)
                    {
                        obj = new InMemoryDataset();
                    }
                    else
                    {
                        Object temp = ConfigurationLoader.LoadObject(g, storeNode);
                        if (temp is IInMemoryQueryableStore)
                        {
                            if (unionDefGraph)
                            {
                                obj = new InMemoryDataset((IInMemoryQueryableStore)temp, unionDefGraph);
                            }
                            else if (defaultGraph != null)
                            {
                                obj = new InMemoryDataset((IInMemoryQueryableStore)temp, defaultGraph);
                            }
                            else
                            {
                                obj = new InMemoryDataset((IInMemoryQueryableStore)temp);
                            }
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the In-Memory Dataset identified by the Node '" + objNode.ToString() + "' since the Object pointed to by the dnr:usingStore property could not be loaded as an object which implements the IInMemoryQueryableStore interface");
                        }
                    }
                    break;

                case InMemoryQuadDataset:
                    storeNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingStore)));
                    if (storeNode == null)
                    {
                        obj = new InMemoryQuadDataset();
                    }
                    else
                    {
                        Object temp = ConfigurationLoader.LoadObject(g, storeNode);
                        if (temp is IInMemoryQueryableStore)
                        {
                            obj = new InMemoryQuadDataset((IInMemoryQueryableStore)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the In-Memory Dataset identified by the Node '" + objNode.ToString() + "' since the Object pointed to by the dnr:usingStore property could not be loaded as an object which implements the IInMemoryQueryableStore interface");
                        }
                    }
                    break;

                case WebDemandDataset:
                    storeNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingDataset)));
                    if (storeNode == null)
                    {
                        obj = new WebDemandDataset(new InMemoryQuadDataset());
                    }
                    else
                    {
                        Object temp = ConfigurationLoader.LoadObject(g, storeNode);
                        if (temp is ISparqlDataset)
                        {
                            obj = new WebDemandDataset((ISparqlDataset)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the Web Demand Dataset identified by the Node '" + objNode.ToString() + "' since the Object pointed to by the dnr:usingDataset property could not be loaded as an object which implements the ISparqlDataset interface");
                        }
                    }
                    break;
            } 

            return (obj != null);
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
                case InMemoryDataset:
                case InMemoryQuadDataset:
                case WebDemandDataset:
                    return true;
                default:
                    return false;
            }
        }
    }
}
