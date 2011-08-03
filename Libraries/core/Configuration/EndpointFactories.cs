/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VDS.RDF.Query;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Factory class for producing SPARQL Endpoints from Configuration Graphs
    /// </summary>
    public class SparqlEndpointFactory : IObjectFactory
    {
        private const String Endpoint = "VDS.RDF.Query.SparqlRemoteEndpoint",
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
                case Endpoint:
                    String endpointUri = ConfigurationLoader.GetConfigurationValue(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyEndpointUri));
                    if (endpointUri == null) return false;

                    //Get Default/Named Graphs if specified
                    IEnumerable<String> defaultGraphs = from n in ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyDefaultGraphUri))
                                                        select n.ToString();
                    IEnumerable<String> namedGraphs = from n in ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyNamedGraphUri))
                                                      select n.ToString();
                    endpoint = new SparqlRemoteEndpoint(new Uri(endpointUri), defaultGraphs, namedGraphs);

                    break;

#if !SILVERLIGHT
                case FederatedEndpoint:
                    IEnumerable<INode> endpoints = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyEndpoint));
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
                            throw new DotNetRdfConfigurationException("Unable to load the SPARQL Endpoint identified by the Node '" + e.ToString() + "' as one of the values for the dnr:endpoint property points to an Object which cannot be loaded as an object which is a SparqlRemoteEndpoint");
                        }
                    }
                    break;
#endif
            }

            if (endpoint != null)
            {
                //Are there any credentials specified?
                String user, pwd;
                ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);
                if (user != null && pwd != null)
                {
                    endpoint.SetCredentials(user, pwd);
                }

#if !NO_PROXY
                //Is there a Proxy Server specified
                INode proxyNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyProxy));
                if (proxyNode != null)
                {
                    Object proxy = ConfigurationLoader.LoadObject(g, proxyNode);
                    if (proxy is WebProxy)
                    {
                        endpoint.Proxy = (WebProxy)proxy;

                        //Are we supposed to use the same credentials for the proxy as for the endpoint?
                        bool useCredentialsForProxy = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUseCredentialsForProxy), false);
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
#endif
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
                case Endpoint:
#if !SILVERLIGHT
                case FederatedEndpoint:
#endif
                    return true;
                default:
                    return false;
            }
        }
    }
}
