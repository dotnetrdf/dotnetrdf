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
using System.Net;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Factory class for producing SPARQL Endpoints from Configuration Graphs
    /// </summary>
    public class SparqlEndpointFactory 
        : IObjectFactory
    {
        private const String QueryEndpoint = "VDS.RDF.Query.SparqlRemoteEndpoint",
                             UpdateEndpoint = "VDS.RDF.Update.SparqlRemoteUpdateEndpoint",
                             FederatedEndpoint = "VDS.RDF.Query.FederatedSparqlRemoteEndpoint";

        /// <summary>
        /// Tries to load a SPARQL Endpoint based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            BaseEndpoint endpoint = null;
            obj = null;

            switch (targetType.FullName)
            {
                case QueryEndpoint:
                    String queryEndpointUri = ConfigurationLoader.GetConfigurationValue(g, objNode, new INode[] { g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyQueryEndpointUri)), g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyEndpointUri)) });
                    if (queryEndpointUri == null) return false;

                    // Get Default/Named Graphs if specified
                    IEnumerable<String> defaultGraphs = from n in ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyDefaultGraphUri)))
                                                        select n.ToString();
                    IEnumerable<String> namedGraphs = from n in ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyNamedGraphUri)))
                                                      select n.ToString();
                    endpoint = new SparqlRemoteEndpoint(UriFactory.Create(queryEndpointUri), defaultGraphs, namedGraphs);
                    break;

                case UpdateEndpoint:
                    String updateEndpointUri = ConfigurationLoader.GetConfigurationValue(g, objNode, new INode[] { g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUpdateEndpointUri)), g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyEndpointUri)) });
                    if (updateEndpointUri == null) return false;

                    endpoint = new SparqlRemoteUpdateEndpoint(UriFactory.Create(updateEndpointUri));
                    break;

                case FederatedEndpoint:
                    IEnumerable<INode> endpoints = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyQueryEndpoint)))
                        .Concat(ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyEndpoint))));
                    foreach (INode e in endpoints)
                    {
                        Object temp = ConfigurationLoader.LoadObject(g, e);
                        if (temp is SparqlRemoteEndpoint)
                        {
                            if (endpoint == null)
                            {
                                endpoint = new FederatedSparqlRemoteEndpoint((SparqlRemoteEndpoint)temp);
                            }
                            else
                            {
                                ((FederatedSparqlRemoteEndpoint)endpoint).AddEndpoint((SparqlRemoteEndpoint)temp);
                            }
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the SPARQL Endpoint identified by the Node '" + e.ToString() + "' as one of the values for the dnr:queryEndpoint/dnr:endpoint property points to an Object which cannot be loaded as an object which is a SparqlRemoteEndpoint");
                        }
                    }
                    break;
            }

            if (endpoint != null)
            {
                // Are there any credentials specified?
                String user, pwd;
                ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);
                if (user != null && pwd != null)
                {
                    endpoint.SetCredentials(user, pwd);
                }

                // Is there a Proxy Server specified
                INode proxyNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyProxy)));
                if (proxyNode != null)
                {
                    Object proxy = ConfigurationLoader.LoadObject(g, proxyNode);
                    if (proxy is IWebProxy)
                    {
                        endpoint.Proxy = (IWebProxy)proxy;

                        // Are we supposed to use the same credentials for the proxy as for the endpoint?
                        bool useCredentialsForProxy = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUseCredentialsForProxy)), false);
                        if (useCredentialsForProxy)
                        {
                            endpoint.UseCredentialsForProxy = true;
                        }
                      }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load SPARQL Endpoint identified by the Node '" + objNode.ToString() + "' as the value for the dnr:proxy property points to an Object which cannot be loaded as an object of type WebProxy");
                    }
                }
            }

            obj = endpoint;
            return (endpoint != null);
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
                case QueryEndpoint:
                case UpdateEndpoint:
                case FederatedEndpoint:
                    return true;
                default:
                    return false;
            }
        }
    }
}
