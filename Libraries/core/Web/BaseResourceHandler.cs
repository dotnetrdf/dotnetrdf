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
using System.Web;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace VDS.RDF.Web
{
    /// <summary>
    /// A Base HTTP Handler for serving Resources at URIs on your server
    /// </summary>
    /// <remarks>
    /// Handlers derived from this currently support only 1 Handler per Web Application but that should hopefully change at some point in the 0.3.x releases
    /// </remarks>
    [Obsolete("This class is considered obsolete - it is recommended that you use a ProtocolHandler instead to achieve similar functionality", true)]
    public abstract class BaseResourceHandler : IHttpHandler
    {
        /// <summary>
        /// List of Uri Rewriting Rules for Resource URIs
        /// </summary>
        protected List<ResourceURIRewriteRule> _rewriteRules = new List<ResourceURIRewriteRule>();

        /// <summary>
        /// Indicates that the Handler is reusable
        /// </summary>
        public virtual bool IsReusable
        {
            get 
            { 
                return true;
            }
        }

        /// <summary>
        /// Handles the Resource Request
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        public abstract void ProcessRequest(HttpContext context);

        /// <summary>
        /// Determines the Cache Key and Config Prefix of the Handler and calls the <see cref="BaseResourceHandler.LoadConfigInternal">LoadConfigInternal</see> method, also loads in the URL Rewriting Rules
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        protected virtual void LoadConfig(HttpContext context)
        {
            //Get the Type Name
            //Use this to get the Prefix that we'll use for our Config variables lookup
            String handlerType = this.GetType().ToString();

            String configPrefix;
            try
            {
                configPrefix = ConfigurationManager.AppSettings[handlerType];
            }
            catch
            {
                configPrefix = String.Empty;
            }   

            //Load Rewrite Rules
            int i = 1;
            while (ConfigurationManager.AppSettings[configPrefix + "RewriteRuleFind" + i] != null)
            {
                try
                {
                    Regex re = new Regex(ConfigurationManager.AppSettings[configPrefix + "RewriteRuleFind" + i]);
                    String replace = ConfigurationManager.AppSettings[configPrefix + "RewriteRuleReplace" + i];
                    if (replace == null) replace = "$0";

                    ResourceURIRewriteRule rule = new ResourceURIRewriteRule(re, replace);
                    this._rewriteRules.Add(rule);
                }
                catch
                {
                    //Do Nothing
                }
                i++;
            }

            this.LoadConfigInternal(context, handlerType, configPrefix);
        }

        /// <summary>
        /// Abstract Configuration Loading method which will be called by the <see cref="BaseResourceHandler.LoadConfig">LoadConfig()</see> method and should be used to do derived class specific Configuration loading
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="cacheKey">Cache Key</param>
        /// <param name="prefix">Config Variable Prefix</param>
        protected abstract void LoadConfigInternal(HttpContext context, String cacheKey, String prefix);

        /// <summary>
        /// Applies the Uri Rewriting Rules to the given Uri
        /// </summary>
        /// <param name="uri">Uri to rewrite</param>
        /// <returns></returns>
        /// <remarks>May be overridden in derived classes to provide more powerful Uri rewriting</remarks>
        protected virtual String RewriteURI(String uri)
        {
            foreach (ResourceURIRewriteRule rule in this._rewriteRules)
            {
                uri = rule.Find.Replace(uri, rule.Replace);
            }

            return uri;
        }
    }

    /// <summary>
    /// Uri Rewriting Rule
    /// </summary>
    public class ResourceURIRewriteRule 
    {
        private Regex _find;
        private String _replace;

        /// <summary>
        /// Creates a new Rewriting Rule
        /// </summary>
        /// <param name="find">Regular Expression to match</param>
        /// <param name="replace">Replace to make for matching URIs</param>
        public ResourceURIRewriteRule(Regex find, String replace) 
        {
            this._find = find;
            this._replace = replace;
        }

        /// <summary>
        /// Gets the Regular Expression to match
        /// </summary>
        public Regex Find 
        {
            get 
            {
                return this._find;
            }
        }

        /// <summary>
        /// Gets the Replace to make for matching URIs
        /// </summary>
        public String Replace 
        {
            get 
            {
                return this._replace;
            }
        }
    }
}

#endif