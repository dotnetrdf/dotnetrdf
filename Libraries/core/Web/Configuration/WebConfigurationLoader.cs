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

#if !NO_WEB && !NO_ASP

using System;
using System.IO;
using System.Web;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Web.Configuration
{
    /// <summary>
    /// Static Helper class for Configuration loading for use in ASP.Net applicatons
    /// </summary>
    public static class WebConfigurationLoader
    {
        /// <summary>
        /// Base Cache Key for Configuration Graph caching
        /// </summary>
        public const String WebConfigGraphCacheKey = "dotNetRDFConfigGraph/";
        /// <summary>
        /// Cache Duration for Configuration Graph caching
        /// </summary>
        public const int WebConfigGraphCacheDuration = 15;

        /// <summary>
        /// Gets the Configuration Graph with the given Filename returns it
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="configFile">Configuration File</param>
        /// <returns></returns>
        public static IGraph LoadConfigurationGraph(HttpContext context, String configFile)
        {
            //Whenever the Configuration Graph is loaded set the Path Resolver to be the WebConfigurationPathResolver
            ConfigurationLoader.PathResolver = new WebConfigurationPathResolver(context.Server);

            //Use a caching mechanism for Config Graphs
            if (context.Cache[WebConfigGraphCacheKey + Path.GetFileName(configFile)] == null)
            {
                Graph g = new Graph();
                FileLoader.Load(g, configFile);
                context.Cache.Add(WebConfigGraphCacheKey + Path.GetFileName(configFile), g, new System.Web.Caching.CacheDependency(configFile), System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, WebConfigGraphCacheDuration, 0), System.Web.Caching.CacheItemPriority.Normal, null);

                //Make sure to auto-detect any Object Factories and custom Parsers/Writers from the Graph
                ConfigurationLoader.AutoDetectObjectFactories(g);
                ConfigurationLoader.AutoDetectReadersAndWriters(g);

                return g;
            }
            else
            {
                Object temp = context.Cache[WebConfigGraphCacheKey + Path.GetFileName(configFile)];
                if (temp is IGraph)
                {
                    //Q: Do we need to call the AutoDetectX() methods again here or not?
                    //ConfigurationLoader.AutoDetectObjectFactories((IGraph)temp);
                    return (IGraph)temp;
                }
                else
                {
                    Graph g = new Graph();
                    FileLoader.Load(g, configFile);
                    context.Cache.Add(WebConfigGraphCacheKey + Path.GetFileName(configFile), g, new System.Web.Caching.CacheDependency(configFile), System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, WebConfigGraphCacheDuration, 0), System.Web.Caching.CacheItemPriority.Normal, null);

                    //Make sure to auto-detect any Object Factories and custom Parsers/Writers from the Graph
                    ConfigurationLoader.AutoDetectObjectFactories(g);
                    ConfigurationLoader.AutoDetectReadersAndWriters(g);

                    return g;
                }
            }
        }

        /// <summary>
        /// Finds whether there is any Handler Configuration for a wildcard path that the current request path matches
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="requestUri">Request URI</param>
        /// <param name="matchedPath">The resulting matched path</param>
        /// <returns></returns>
        public static INode FindObject(IGraph g, Uri requestUri, out String matchedPath)
        {
            int pathSegments = requestUri.Segments.Length;
            for (int s = pathSegments; s > -1; s--)
            {
                String path = String.Join(String.Empty, requestUri.Segments, 0, s) + "*";
                String objUri = "dotnetrdf:" + path;
                INode temp = g.GetUriNode(new Uri(objUri));
                if (temp != null)
                {
                    matchedPath = path;
                    return temp;
                }
            }

            matchedPath = null;
            return null;
        }
    }

    /// <summary>
    /// Path Resolver for Web Configuration loading
    /// </summary>
    public class WebConfigurationPathResolver : IPathResolver
    {
        private HttpServerUtility _server;

        /// <summary>
        /// Creates a new Web Configuration Path Resolver
        /// </summary>
        /// <param name="server">HTTP Server Utility</param>
        public WebConfigurationPathResolver(HttpServerUtility server)
        {
            this._server = server;
        }

        /// <summary>
        /// Resolves a Path by calling MapPath() where appropriate
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns></returns>
        public string ResolvePath(string path)
        {
            if (path.StartsWith("~"))
            {
                return this._server.MapPath(path);
            }
            else
            {
                return path;
            }
        }
    }
}

#endif