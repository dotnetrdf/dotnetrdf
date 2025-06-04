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
using System.Net;
using System.Net.Http;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace VDS.RDF.Configuration;

/// <summary>
/// Factory class for producing SPARQL endpoints from configuration graphs.
/// </summary>
public class SparqlClientFactory : IObjectFactory
{
    private const string SparqlQueryClient = "VDS.RDF.Query.SparqlQueryClient",
        SparqlUpdateClient = "VDS.RDF.Update.SparqlUpdateClient",
        FederatedSparqlQueryClient = "VDS.RDF.Query.FederatedSparqlQueryClient";

    /// <inheritdoc />
    public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
    {
        obj = null;
        HttpClient httpClient = CreateClient(g, objNode);
        switch (targetType.FullName)
        {
            case SparqlQueryClient:
                var queryEndpointUri = ConfigurationLoader.GetConfigurationValue(g, objNode, new INode[] { g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyQueryEndpointUri)), g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyEndpointUri)) });
                if (queryEndpointUri == null) return false;

                // Get Default/Named Graphs if specified
                IEnumerable<string> defaultGraphs = from n in ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyDefaultGraphUri)))
                    select n.ToString();
                IEnumerable<string> namedGraphs = from n in ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyNamedGraphUri)))
                    select n.ToString();
                var client = new SparqlQueryClient(httpClient, new Uri(queryEndpointUri));
                client.DefaultGraphs.AddRange(defaultGraphs);
                client.NamedGraphs.AddRange(namedGraphs);
                obj = client;
                break;
            case SparqlUpdateClient:
                var updateEndpointUri = ConfigurationLoader.GetConfigurationValue(g, objNode, new INode[] { g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUpdateEndpointUri)), g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyEndpointUri)) });
                if (updateEndpointUri == null) return false;
                var updateClient = new SparqlUpdateClient(httpClient, new Uri(updateEndpointUri));
                obj = updateClient;
                break;
            case FederatedSparqlQueryClient:
                IEnumerable<INode> endpoints = ConfigurationLoader.GetConfigurationData(g, objNode,
                        g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyQueryEndpoint)))
                    .Concat(
                        ConfigurationLoader.GetConfigurationData(g, objNode,
                            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyEndpointUri))));
                var federatedClient =
                    new FederatedSparqlQueryClient(httpClient);
                foreach (INode endpointConfigurationNode in endpoints)
                {
                    object temp = ConfigurationLoader.LoadObject(g, endpointConfigurationNode);
                    switch (temp)
                    {
                        case SparqlQueryClient queryClient:
                            federatedClient.AddEndpoint(queryClient);
                            break;
                        case Uri endpointUri:
                            federatedClient.AddEndpoint(new SparqlQueryClient(httpClient, endpointUri));
                            break;
                    }
                }
                obj = federatedClient;
                break;
        }

        return obj != null;
    }

    private static HttpClient CreateClient(IGraph g, INode objNode)
    {
        ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out var user, out var pwd);
        INode proxyNode = ConfigurationLoader.GetConfigurationNode(g, objNode,
            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyProxy)));
        IWebProxy webProxy = null;
        var useCredentialsForProxy = ConfigurationLoader.GetConfigurationBoolean(g, objNode,
            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUseCredentialsForProxy)), false);

        if (proxyNode != null)
        {
            object proxy = ConfigurationLoader.LoadObject(g, proxyNode);
            webProxy = proxy as IWebProxy;
        }

        if ((user != null && pwd != null) || webProxy != null)
        {
            var messageHandler = new HttpClientHandler();
            if (user != null && pwd != null)
            {
                var credentials = new NetworkCredential(user, pwd);
                if (useCredentialsForProxy)
                {
                    if (webProxy != null) webProxy.Credentials = credentials;
                }
                else
                {
                    messageHandler.Credentials = credentials;
                }
            }

            if (webProxy != null) messageHandler.Proxy = webProxy;
            return new HttpClient(messageHandler);
        }

        return new HttpClient();
    }

    /// <inheritdoc/>
    public bool CanLoadObject(Type t)
    {
        switch (t.FullName)
        {
            case SparqlQueryClient:
            case SparqlUpdateClient:
            case FederatedSparqlQueryClient:
                return true;
            default:
                return false;
        }
    }
}
