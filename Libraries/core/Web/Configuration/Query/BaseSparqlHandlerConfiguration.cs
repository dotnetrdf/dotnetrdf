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
using System.Configuration;
using System.IO;
using System.Web;
using VDS.RDF.Query;

namespace VDS.RDF.Web.Configuration.Query
{
    /// <summary>
    /// Abstract base class for representing SPARQL Handler Configuration
    /// </summary>
    [Obsolete("This class is obseleted and has been superseded by BaseQueryHandlerConfiguration", true)]
    public abstract class BaseSparqlHandlerConfiguration
    {
        /// <summary>
        /// Minimum Cache Duration setting permitted
        /// </summary>
        public const int MinimumCacheDuration = 0;
        /// <summary>
        /// Maximum Cache Duration setting permitted
        /// </summary>
        public const int MaximumCacheDuration = 120;

        /// <summary>
        /// Default Graph Uri for queries
        /// </summary>
        protected String _defaultGraph = String.Empty;
        /// <summary>
        /// Default Timeout for Queries
        /// </summary>
        protected long _defaultTimeout = 30000;
        /// <summary>
        /// Default Partial Results on Timeout behaviour
        /// </summary>
        protected bool _defaultPartialResults = false;
        /// <summary>
        /// Whether the Handler supports Timeouts
        /// </summary>
        protected bool _supportsTimeout = false;
        /// <summary>
        /// Whether the Handler supports Partial Results on Timeout
        /// </summary>
        protected bool _supportsPartialResults = false;
        /// <summary>
        /// Querystring Field name for the Timeout setting
        /// </summary>
        protected String _timeoutField = "timeout";
        /// <summary>
        /// Querystring Field name for the Partial Results setting
        /// </summary>
        protected String _partialResultsField = "partialResults";
        /// <summary>
        /// Whether errors are shown to the User
        /// </summary>
        protected bool _showErrors = true;
        /// <summary>
        /// Whether a Query Form should be shown to the User
        /// </summary>
        protected bool _showQueryForm = true;
        /// <summary>
        /// Default Sparql Query
        /// </summary>
        protected String _defaultQuery = String.Empty;
        /// <summary>
        /// Stylesheet for formatting the Query Form and HTML format results
        /// </summary>
        protected String _stylesheet = String.Empty;
        /// <summary>
        /// Number of minutes to Cache stuff for
        /// </summary>
        protected int _cacheDuration = 15;
        /// <summary>
        /// Introduction Text for the Query Form
        /// </summary>
        protected String _introText = String.Empty;
        /// <summary>
        /// Indicates whether full Triple indexing should be used by Handlers which use the in-memory Sparql engine
        /// </summary>
        protected bool _fullIndexing = true;
        /// <summary>
        /// SPARQL Engine to be used
        /// </summary>
        protected SparqlEngine _queryEngine = SparqlEngine.Leviathan;

        /// <summary>
        /// Creates a new instance of the Base Configuration which loads all the standard settings from Configuration
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="cacheKey">Cache Key</param>
        /// <param name="configPrefix">Config Prefix</param>
        public BaseSparqlHandlerConfiguration(HttpContext context, String cacheKey, String configPrefix)
        {
            //Try to get Configuration
            String defaultGraph, defaultQuery;
            String introText;
            long timeout;
            bool partialResults, showErrors, showForm, fullIndexing;

            try
            {
                //SPARQL Engine Config
                if (ConfigurationManager.AppSettings[configPrefix + "Engine"] != null)
                {
                    this._queryEngine = (SparqlEngine)Enum.Parse(typeof(SparqlEngine), ConfigurationManager.AppSettings[configPrefix + "Engine"]);
                    Options.QueryEngine = this._queryEngine;
                }

                //Sparql Query Default Config
                if (ConfigurationManager.AppSettings[configPrefix + "DefaultGraph"] != null)
                {
                    defaultGraph = ConfigurationManager.AppSettings[configPrefix + "DefaultGraph"];
                    this._defaultGraph = defaultGraph;
                }
                if (Int64.TryParse(ConfigurationManager.AppSettings[configPrefix + "DefaultTimeout"], out timeout))
                {
                    this._defaultTimeout = timeout;
                }
                if (Boolean.TryParse(ConfigurationManager.AppSettings[configPrefix + "DefaultPartialResults"], out partialResults))
                {
                    this._defaultPartialResults = partialResults;
                }

                //In-Memory Query Engine Config
                if (Boolean.TryParse(ConfigurationManager.AppSettings[configPrefix + "FullIndexing"], out fullIndexing))
                {
                    this._fullIndexing = fullIndexing;
                }

                //Handler Config
                if (Boolean.TryParse(ConfigurationManager.AppSettings[configPrefix + "ShowErrors"], out showErrors))
                {
                    this._showErrors = showErrors;
                }
                if (Boolean.TryParse(ConfigurationManager.AppSettings[configPrefix + "ShowQueryForm"], out showForm))
                {
                    this._showQueryForm = showForm;
                }
                if (ConfigurationManager.AppSettings[configPrefix + "DefaultQueryFile"] != null)
                {
                    defaultQuery = context.Server.MapPath(ConfigurationManager.AppSettings[configPrefix + "DefaultQueryFile"]);
                    if (File.Exists(defaultQuery))
                    {
                        StreamReader temp = new StreamReader(defaultQuery);
                        this._defaultQuery = temp.ReadToEnd();
                        temp.Close();
                        temp = null;
                    }
                }
                if (ConfigurationManager.AppSettings[configPrefix + "IntroText"] != null)
                {
                    introText = context.Server.MapPath(ConfigurationManager.AppSettings[configPrefix + "IntroText"]);
                    if (File.Exists(introText))
                    {
                        StreamReader temp = new StreamReader(introText);
                        this._introText = temp.ReadToEnd();
                        temp.Close();
                        temp = null;
                    }
                }
                if (ConfigurationManager.AppSettings[configPrefix + "Stylesheet"] != null)
                {
                    this._stylesheet = ConfigurationManager.AppSettings[configPrefix + "Stylesheet"];
                }
                if (ConfigurationManager.AppSettings[configPrefix + "CacheDuration"] != null)
                {
                    int duration;
                    if (Int32.TryParse(ConfigurationManager.AppSettings[configPrefix + "CacheDuration"], out duration))
                    {
                        if (duration >= MinimumCacheDuration && duration <= MaximumCacheDuration)
                        {
                            this._cacheDuration = duration;
                        }
                    }
                }
            }
            catch
            {
                throw new RdfQueryException("SPARQL Handler Configuration could not be found/was invalid");
            }
        }

        /// <summary>
        /// Forces persistent global options to be set to the Configuration values
        /// </summary>
        public void SetPersistentProperties()
        {
            //Set Triple Indexing
            Options.FullTripleIndexing = this.FullTripleIndexing;

            //Set Query Engine
            Options.QueryEngine = this.QueryEngine;
        }

        /// <summary>
        /// Gets the Query Engine used for Queries
        /// </summary>
        public SparqlEngine QueryEngine
        {
            get
            {
                return this._queryEngine;
            }
        }

        /// <summary>
        /// Gets the Default Graph Uri
        /// </summary>
        public String DefaultGraphURI
        {
            get
            {
                return this._defaultGraph;
            }
        }

        /// <summary>
        /// Whether the Remote Endpoint supports specifying Query Timeout as a querystring parameter
        /// </summary>
        public bool SupportsTimeout
        {
            get
            {
                return this._supportsTimeout;
            }
        }

        /// <summary>
        /// Gets the Default Query Execution Timeout
        /// </summary>
        public long DefaultTimeout
        {
            get
            {
                return this._defaultTimeout;
            }
        }

        /// <summary>
        /// Querystring field name for the Query Timeout for Remote Endpoints which support it
        /// </summary>
        public String TimeoutField
        {
            get
            {
                return this._timeoutField;
            }
        }

        /// <summary>
        /// Whether the Remote Endpoint supports specifying Partial Results on Timeout behaviour as a querystring parameter
        /// </summary>
        public bool SupportsPartialResults
        {
            get
            {
                return this._supportsPartialResults;
            }
        }

        /// <summary>
        /// Gets the Default Partial Results on Timeout behaviour
        /// </summary>
        public bool DefaultPartialResults
        {
            get
            {
                return this._defaultPartialResults;
            }
        }

        /// <summary>
        /// Querystring field name for the Partial Results on Timeout setting for Remote Endpoints which support it
        /// </summary>
        public String PartialResultsField
        {
            get
            {
                return this._partialResultsField;
            }
        }

        /// <summary>
        /// Gets whether Error Messages should be shown to users
        /// </summary>
        public bool ShowErrors
        {
            get
            {
                return this._showErrors;
            }
        }

        /// <summary>
        /// Gets whether the Query Form should be shown to users
        /// </summary>
        public bool ShowQueryForm
        {
            get
            {
                return this._showQueryForm;
            }
        }

        /// <summary>
        /// Gets the Default Query for the Query Form
        /// </summary>
        public String DefaultQuery
        {
            get
            {
                return this._defaultQuery;
            }
        }

        /// <summary>
        /// Gets the Stylesheet for formatting HTML Results
        /// </summary>
        public String Stylesheet
        {
            get
            {
                return this._stylesheet;
            }
        }

        /// <summary>
        /// Gets how many minutes stuff should be cached for by Handlers
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Sparql Handlers use the ASP.Net <see cref="Cache">Cache</see> object to cache information and they specify the caching duration as a Sliding Duration.  This means that each time the cache is accessed the expiration time increases again.
        /// </para>
        /// <para>
        /// This defaults to 15 minutes and the Handlers will only allow you to set a value between the <see cref="MinimumCacheDuration">MinimumCacheDuration</see> and <see cref="MaximumCacheDuration">MaximumCacheDuration</see>.  We think that 15 minutes is a good setting and we use this as the default setting unless a duration is specified explicitly.
        /// </para>
        /// </remarks>
        public int CacheDuration
        {
            get
            {
                return this._cacheDuration;
            }
        }

        /// <summary>
        /// Gets the Introduction Text for the Query Form
        /// </summary>
        public String IntroductionText
        {
            get
            {
                return this._introText;
            }
        }

        /// <summary>
        /// Gets whether full Triple indexing should be used by Handlers which use the in-memory Sparql engine
        /// </summary>
        public bool FullTripleIndexing
        {
            get
            {
                return this._fullIndexing;
            }
        }
    }
}

#endif