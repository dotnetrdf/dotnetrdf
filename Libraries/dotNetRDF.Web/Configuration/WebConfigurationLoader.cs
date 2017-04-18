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
            // Whenever the Configuration Graph is loaded set the Path Resolver to be the WebConfigurationPathResolver
            ConfigurationLoader.PathResolver = new WebConfigurationPathResolver(context.Server);

            // Use a caching mechanism for Config Graphs
            if (context.Cache[WebConfigGraphCacheKey + Path.GetFileName(configFile)] == null)
            {
                IGraph g = ConfigurationLoader.LoadConfiguration(configFile);
                context.Cache.Add(WebConfigGraphCacheKey + Path.GetFileName(configFile), g, new System.Web.Caching.CacheDependency(configFile), System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, WebConfigGraphCacheDuration, 0), System.Web.Caching.CacheItemPriority.Normal, null);

                return g;
            }
            else
            {
                Object temp = context.Cache[WebConfigGraphCacheKey + Path.GetFileName(configFile)];
                if (temp is IGraph)
                {
                    // Q: Do we need to call the AutoDetectX() methods again here or not?
                    // ConfigurationLoader.AutoConfigure((IGraph)temp);
                    return (IGraph)temp;
                }
                else
                {
                    IGraph g = ConfigurationLoader.LoadConfiguration(configFile);
                    context.Cache.Add(WebConfigGraphCacheKey + Path.GetFileName(configFile), g, new System.Web.Caching.CacheDependency(configFile), System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, WebConfigGraphCacheDuration, 0), System.Web.Caching.CacheItemPriority.Normal, null);

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
                INode temp = g.GetUriNode(UriFactory.Create(objUri));
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
    public class WebConfigurationPathResolver
        : IPathResolver
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
