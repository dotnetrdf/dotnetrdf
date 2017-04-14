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
using System.Configuration;
using System.Web;
using System.Web.Caching;
using VDS.RDF.Configuration;
using VDS.RDF.Web.Configuration;
using VDS.RDF.Web.Configuration.Protocol;

namespace VDS.RDF.Web
{
    /// <summary>
    /// HTTP Handler for adding SPARQL Graph Store HTTP Protocol for RDF Graph Management endpoints to ASP.Net applications
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used to create a Protocol endpoint at a Base URL with any URL under this handled by this Handler
    /// </para>
    /// <para>
    /// This Handler is configured using the new Configuration API introduced in the 0.3.0 release.  This requires just one setting to be defined in the &lt;appSettings&gt; section of your Web.config file which points to a Configuration Graph like so:
    /// <code>&lt;add key="dotNetRDFConfig" value="~/App_Data/config.ttl" /&gt;</code>
    /// The Configuration Graph must then contain Triples like the following to specify a Protocol Endpoint:
    /// <code>
    /// &lt;dotnetrdf:/folder/protocol/*&gt; a dnr:HttpHandler ;
    ///                                      dnr:type "VDS.RDF.Web.WildcardProtocolHandler" ;
    ///                                      dnr:protocolProcessor _:proc .
    ///                                 
    /// _:proc a dnr:SparqlHttpProtocolProcessor ;
    ///        dnr:type "VDS.RDF.Update.Protocol.LeviathanProtocolProcessor" ;
    ///        dnr:usingStore _:store .
    ///        
    /// _:store a dnr:TripleStore ;
    ///         dnr:type "VDS.RDF.TripleStore" .
    /// </code>
    /// </para>
    /// </remarks>
    public class WildcardProtocolHandler : BaseSparqlHttpProtocolHandler
    {

        /// <summary>
        /// Loads the Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="basePath">Base Path of the Handler which this method will determine</param>
        /// <returns></returns>
        protected override BaseProtocolHandlerConfiguration LoadConfig(HttpContext context, out String basePath)
        {
            // Check the Configuration File is specified
            String configFile = context.Server.MapPath(ConfigurationManager.AppSettings["dotNetRDFConfig"]);
            if (configFile == null) throw new DotNetRdfConfigurationException("Unable to load Wildcard Protocol Handler Configuration as the Web.Config file does not specify a 'dotNetRDFConfig' AppSetting to specify the RDF configuration file to use");
            IGraph g = WebConfigurationLoader.LoadConfigurationGraph(context, configFile);

            // Then check there is configuration associated with the expected URI
            INode objNode = WebConfigurationLoader.FindObject(g, context.Request.Url, out basePath);
            if (objNode == null) throw new DotNetRdfConfigurationException("Unable to load Wildcard Protocol Handler Configuration as the RDF configuration file does not have any configuration associated with an appropriate wildcard URI");
            this._basePath = basePath;

            // Is our Configuration already cached?
            Object temp = context.Cache[this._basePath];
            if (temp != null)
            {
                if (temp is BaseProtocolHandlerConfiguration)
                {
                    return (BaseProtocolHandlerConfiguration)temp;
                }
                else
                {
                    context.Cache.Remove(this._basePath);
                }
            }

            ProtocolHandlerConfiguration config = new ProtocolHandlerConfiguration(new WebContext(context), g, objNode);

            // Finally cache the Configuration before returning it
            if (config.CacheSliding)
            {
                context.Cache.Add(this._basePath, config, new System.Web.Caching.CacheDependency(configFile), System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, config.CacheDuration, 0), System.Web.Caching.CacheItemPriority.Normal, null);
            }
            else
            {
                context.Cache.Add(this._basePath, config, new System.Web.Caching.CacheDependency(configFile), DateTime.Now.AddMinutes(config.CacheDuration), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
            }
            return config;
        }

        /// <summary>
        /// Updates the Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        protected override void UpdateConfig(HttpContext context)
        {
            if (this._config.CacheDuration == 0)
            {
                if (context.Cache[this._basePath] != null) context.Cache.Remove(this._basePath);
            }
            else
            {
                if (context.Cache[this._basePath] != null)
                {
                    context.Cache[this._basePath] = this._config;
                }
                else
                {
                    String configFile = context.Server.MapPath(ConfigurationManager.AppSettings["dotNetRDFConfig"]);
                    System.Web.Caching.CacheDependency dependency = (configFile != null) ? new System.Web.Caching.CacheDependency(configFile) : null;

                    if (this._config.CacheSliding)
                    {
                        context.Cache.Add(this._basePath, this._config, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, this._config.CacheDuration, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                    }
                    else
                    {
                        context.Cache.Add(this._basePath, this._config, null, DateTime.Now.AddMinutes(this._config.CacheDuration), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
                    }
                }
            }
        }
    }
}
