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
using System.Collections.Generic;
using System.IO;
using System.Web;
using VDS.RDF.Configuration;
using VDS.RDF.Configuration.Permissions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Writing;

namespace VDS.RDF.Web.Configuration
{
    /// <summary>
    /// Abstract Base Class for Handler Configuration
    /// </summary>
    public abstract class BaseHandlerConfiguration
    {
        /// <summary>
        /// Minimum Cache Duration setting permitted
        /// </summary>
        public const int MinimumCacheDuration = 0;
        /// <summary>
        /// Maximum Cache Duration setting permitted
        /// </summary>
        public const int MaximumCacheDuration = 120;

        private List<UserGroup> _userGroups = new List<UserGroup>();
        private int _cacheDuration = 15;
        private bool _cacheSliding = true;

        /// <summary>
        /// Whether errors are shown to the User
        /// </summary>
        protected bool _showErrors = true;
        /// <summary>
        /// Stylesheet for formatting the Query Form and HTML format results
        /// </summary>
        protected String _stylesheet = String.Empty;
        /// <summary>
        /// Introduction Text for the Query Form
        /// </summary>
        protected String _introText = String.Empty;
        /// <summary>
        /// List of Custom Expression Factories which have been specified in the Handler Configuration
        /// </summary>
        protected List<ISparqlCustomExpressionFactory> _expressionFactories = new List<ISparqlCustomExpressionFactory>();

        /// <summary>
        /// Sets whether CORS headers are output
        /// </summary>
        protected bool _corsEnabled = true;

        /// <summary>
        /// Writer Compression Level
        /// </summary>
        protected int _writerCompressionLevel = Options.DefaultCompressionLevel;
        /// <summary>
        /// Writer Pretty Printing Mode
        /// </summary>
        protected bool _writerPrettyPrinting = true;
        /// <summary>
        /// Writer High Speed Mode permitted?
        /// </summary>
        protected bool _writerHighSpeed = true;
        /// <summary>
        /// XML Writers can use DTDs?
        /// </summary>
        protected bool _writerDtds = false;
        /// <summary>
        /// Multi-threaded writers can write multi-threaded?
        /// </summary>
        protected bool _writerMultiThreading = true;
        /// <summary>
        /// XML Writers can compress literal objects to attributes?
        /// </summary>
        protected bool _writerAttributes = true;
        /// <summary>
        /// Default Namespaces for appropriate writers
        /// </summary>
        protected INamespaceMapper _defaultNamespaces = new NamespaceMapper();

        /// <summary>
        /// Creates a new Base Handler Configuration which loads common Handler settings from a Configuration Graph
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <remarks>
        /// <para>
        /// It is acceptable for the <paramref name="content">context</paramref> parameter to be null
        /// </para>
        /// </remarks>
        public BaseHandlerConfiguration(HttpContext context, IGraph g, INode objNode)
            : this(g, objNode) { }

        /// <summary>
        /// Creates a new Base Handler Configuration which loads common Handler settings from a Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public BaseHandlerConfiguration(IGraph g, INode objNode)
        {
            //Are there any User Groups associated with this Handler?
            IEnumerable<INode> groups = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, "dnr:userGroup"));
            foreach (INode group in groups)
            {
                Object temp = ConfigurationLoader.LoadObject(g, group);
                if (temp is UserGroup)
                {
                    this._userGroups.Add((UserGroup)temp);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load Handler Configuration as the RDF Configuration file specifies a value for the Handlers dnr:userGroup property which cannot be loaded as an object which is a UserGroup");
                }
            }

            //General Handler Settings
            this._showErrors = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyShowErrors), this._showErrors);
            String introFile = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyIntroFile));
            if (introFile != null)
            {
                introFile = ConfigurationLoader.ResolvePath(introFile);
                if (File.Exists(introFile))
                {
                    using (StreamReader reader = new StreamReader(introFile))
                    {
                        this._introText = reader.ReadToEnd();
                        reader.Close();
                    }
                }
            }
            this._stylesheet = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyStylesheet)).ToSafeString();
            this._corsEnabled = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyEnableCors), true);

            //Cache Settings
            this._cacheDuration = ConfigurationLoader.GetConfigurationInt32(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyCacheDuration), this._cacheDuration);
            if (this._cacheDuration < MinimumCacheDuration) this._cacheDuration = MinimumCacheDuration;
            if (this._cacheDuration > MaximumCacheDuration) this._cacheDuration = MaximumCacheDuration;
            this._cacheSliding = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyCacheSliding), this._cacheSliding);

            //SPARQL Expression Factories
            IEnumerable<INode> factories = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyExpressionFactory));
            foreach (INode factory in factories)
            {
                Object temp = ConfigurationLoader.LoadObject(g, factory);
                if (temp is ISparqlCustomExpressionFactory)
                {
                    this._expressionFactories.Add((ISparqlCustomExpressionFactory)temp);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load Handler Configuration as the RDF Configuration file specifies a value for the Handlers dnr:expressionFactory property which cannot be loaded as an object which is a SPARQL Expression Factory");
                }
            }

            //Writer Properties
            this._writerCompressionLevel = ConfigurationLoader.GetConfigurationInt32(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyCompressionLevel), this._writerCompressionLevel);
            this._writerDtds = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyDtdWriting), this._writerDtds);
            this._writerHighSpeed = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyHighSpeedWriting), this._writerHighSpeed);
            this._writerMultiThreading = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyMultiThreadedWriting), this._writerMultiThreading);
            this._writerPrettyPrinting = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyPrettyPrinting), this._writerPrettyPrinting);
            this._writerAttributes = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyAttributeWriting), this._writerAttributes);

            //Load in the Default Namespaces if specified
            INode nsNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyImportNamespacesFrom));
            if (nsNode != null)
            {
                Object nsTemp = ConfigurationLoader.LoadObject(g, nsNode);
                if (nsTemp is IGraph)
                {
                    this._defaultNamespaces.Import(((IGraph)nsTemp).NamespaceMap);
                }
            }
        }

        /// <summary>
        /// Gets the User Groups for the Handler
        /// </summary>
        public IEnumerable<UserGroup> UserGroups
        {
            get
            {
                return this._userGroups;
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
        /// Gets whether CORS (Cross Origin Resource Sharing) headers are sent to the client in HTTP responses
        /// </summary>
        public bool IsCorsEnabled
        {
            get
            {
                return this._corsEnabled;
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
        /// Gets the Cache Duration in minutes to use
        /// </summary>
        /// <para>
        /// The SPARQL Handlers use the ASP.Net <see cref="Cache">Cache</see> object to cache information and they specify the caching duration as a Sliding Duration by default.  This means that each time the cache is accessed the expiration time increases again.  Set the <see cref="BaseQueryHandlerConfiguration.CacheSliding">CacheSliding</see> property to false if you'd prefer an absolute expiration
        /// </para>
        /// <para>
        /// This defaults to 15 minutes and the Handlers will only allow you to set a value between the <see cref="MinimumCacheDuration">MinimumCacheDuration</see> and <see cref="MaximumCacheDuration">MaximumCacheDuration</see>.  We think that 15 minutes is a good setting and we use this as the default setting unless a duration is specified explicitly.
        /// </para>
        public int CacheDuration
        {
            get
            {
                return this._cacheDuration;
            }
        }

        /// <summary>
        /// Gets whether Sliding Cache expiration is used
        /// </summary>
        /// <remarks>
        /// <para>
        /// The SPARQL Handlers use the ASP.Net <see cref="Cache">Cache</see> object to cache information and they specify the cache duration as a Sliding Duration by default.  Set this property to false if you'd prefer absolute expiration
        /// </para>
        /// </remarks>
        public bool CacheSliding
        {
            get
            {
                return this._cacheSliding;
            }
        }

        /// <summary>
        /// Gets whether any Custom Expression Factories are registered in the Config for this Handler
        /// </summary>
        public bool HasExpressionFactories
        {
            get
            {
                return (this._expressionFactories.Count > 0);
            }
        }

        /// <summary>
        /// Gets the Custom Expression Factories which are in the Config for this Handler
        /// </summary>
        public IEnumerable<ISparqlCustomExpressionFactory> ExpressionFactories
        {
            get
            {
                return this._expressionFactories;
            }
        }

        /// <summary>
        /// Gets the Writer Compression Level to use
        /// </summary>
        public int WriterCompressionLevel
        {
            get
            {
                return this._writerCompressionLevel;
            }
        }

        /// <summary>
        /// Gets whether XML Writers can use DTDs
        /// </summary>
        public bool WriterUseDtds
        {
            get
            {
                return this._writerDtds;
            }
        }

        /// <summary>
        /// Gets whether XML Writers can compress literal objects as attributes
        /// </summary>
        public bool WriterUseAttributes
        {
            get
            {
                return this._writerAttributes;
            }
        }

        /// <summary>
        /// Gets whether some writers can use high-speed mode when they detect that Graphs are ill-suited to syntax compression
        /// </summary>
        public bool WriterHighSpeedMode
        {
            get
            {
                return this._writerHighSpeed;
            }
        }

        /// <summary>
        /// Gets whether multi-threaded writers are allowed to use multi-threaded mode
        /// </summary>
        public bool WriterMultiThreading
        {
            get
            {
                return this._writerMultiThreading;
            }
        }

        /// <summary>
        /// Gets whether Pretty Printing is enabled
        /// </summary>
        public bool WriterPrettyPrinting
        {
            get
            {
                return this._writerPrettyPrinting;
            }
        }

        /// <summary>
        /// Gets the Default Namespaces used for writing
        /// </summary>
        public INamespaceMapper DefaultNamespaces
        {
            get
            {
                return this._defaultNamespaces;
            }
        }
    }
}

#endif