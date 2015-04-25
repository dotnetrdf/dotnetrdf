/*

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Configuration;
using System.Web;
using VDS.RDF.Configuration;
using VDS.RDF.Query.Spin.Web.Configuration;
using VDS.RDF.Web;
using VDS.RDF.Web.Configuration;
using VDS.RDF.Web.Configuration.Server;

namespace VDS.RDF.Query.Spin.Web
{
    /// <summary>
    /// A direct SparqlEndpoint implementation that wraps the SPIN framework around a RDF storage
    /// </summary>
    /// <remarks>
    /// This class is meant to provide direct SPARQL 1.1 protocol handling against a StorageProvider
    /// </remarks>
    /// TODO provide ExpressionFactories, AlgebraOptimizers and PropertyFunctionFactory from the configuration
    /// TODO try to define how we can notify that the current context depends on the user
    /// TODO check how to handle connection based on the httpContext(using session/cookies...)
    ///         then check how to provide client side transaction management
    public class SparqlEndpointServer
        : VDS.RDF.Web.BaseSparqlServer
    {
        private String _cachePath;

        // TODO define the policy for connection management
        private Connection _connection;

        /// <summary>
        /// Loads the Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="basePath">Base Path for the Server</param>
        /// <returns></returns>
        protected override BaseSparqlServerConfiguration LoadConfig(HttpContext context, out string basePath)
        {
            //Check the Configuration File is specified
            String configFile = context.Server.MapPath(ConfigurationManager.AppSettings["dotNetRDFConfig"]);
            if (configFile == null) throw new DotNetRdfConfigurationException("Unable to load Graph Handler Configuration as the Web.Config file does not specify a 'dotNetRDFConfig' AppSetting to specify the RDF configuration file to use");
            IGraph g = WebConfigurationLoader.LoadConfigurationGraph(context, configFile);

            //Then check there is configuration associated with the expected URI
            INode objNode = WebConfigurationLoader.FindObject(g, context.Request.Url, out basePath);
            this._cachePath = basePath;
            if (objNode == null) throw new DotNetRdfConfigurationException("Unable to load Graph Handler Configuration as the RDF configuration file does not have any configuration associated with the URI <dotnetrdf:" + context.Request.Path + "> as required");

            //Is our Configuration already cached?
            Object temp = context.Cache[basePath];
            if (temp != null)
            {
                if (temp is BaseSparqlServerConfiguration)
                {
                    basePath = basePath.Substring(0, basePath.Length - 1);
                    return (BaseSparqlServerConfiguration)temp;
                }
                else
                {
                    context.Cache.Remove(basePath);
                }
            }

            // TODO create a server configuration using the connection
            SparqlEndpointConfiguration config = new SparqlEndpointConfiguration(new WebContext(context), g, objNode);
            _connection = config.Connection;

            //Finally cache the Configuration before returning it
            if (config.CacheSliding)
            {
                context.Cache.Add(basePath, config, new System.Web.Caching.CacheDependency(configFile), System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, config.CacheDuration, 0), System.Web.Caching.CacheItemPriority.Normal, null);
            }
            else
            {
                context.Cache.Add(basePath, config, new System.Web.Caching.CacheDependency(configFile), DateTime.Now.AddMinutes(config.CacheDuration), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
            }
            basePath = basePath.Substring(0, basePath.Length - 1);
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
                if (context.Cache[context.Request.Path] != null) context.Cache.Remove(context.Request.Path);
            }
            else
            {
                if (context.Cache[this._cachePath] != null)
                {
                    context.Cache[this._cachePath] = this._config;
                }
                else
                {
                    String configFile = context.Server.MapPath(ConfigurationManager.AppSettings["dotNetRDFConfig"]);
                    System.Web.Caching.CacheDependency dependency = (configFile != null) ? new System.Web.Caching.CacheDependency(configFile) : null;

                    if (this._config.CacheSliding)
                    {
                        context.Cache.Add(this._cachePath, this._config, dependency, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, this._config.CacheDuration, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                    }
                    else
                    {
                        context.Cache.Add(this._cachePath, this._config, dependency, DateTime.Now.AddMinutes(this._config.CacheDuration), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
                    }
                }
            }
        }

        protected override object ProcessQuery(SparqlQuery query)
        {
            _connection.Open();
            return _connection.Query(query);
        }

        protected override void ProcessUpdates(Update.SparqlUpdateCommandSet cmds)
        {
            _connection.Open();
            _connection.Update(cmds);
        }
    }
}