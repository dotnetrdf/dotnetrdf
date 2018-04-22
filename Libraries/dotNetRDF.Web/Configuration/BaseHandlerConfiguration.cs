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
using System.IO;
using VDS.RDF.Configuration;
using VDS.RDF.Configuration.Permissions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.PropertyFunctions;

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
        /// List of Property Function Factories which have been specified in the Handler Configuration
        /// </summary>
        protected List<IPropertyFunctionFactory> _propertyFunctionFactories = new List<IPropertyFunctionFactory>();

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
        /// It is acceptable for the <paramref name="context">context</paramref> parameter to be null
        /// </para>
        /// </remarks>
        public BaseHandlerConfiguration(IHttpContext context, IGraph g, INode objNode)
            : this(g, objNode) { }

        /// <summary>
        /// Creates a new Base Handler Configuration which loads common Handler settings from a Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public BaseHandlerConfiguration(IGraph g, INode objNode)
        {
            // Are there any User Groups associated with this Handler?
            IEnumerable<INode> groups = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUserGroup)));
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

            // General Handler Settings
            this._showErrors = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyShowErrors)), this._showErrors);
            String introFile = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyIntroFile)));
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
            this._stylesheet = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyStylesheet)))?.ToString() ?? string.Empty;
            this._corsEnabled = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyEnableCors)), true);

            // Cache Settings
            this._cacheDuration = ConfigurationLoader.GetConfigurationInt32(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyCacheDuration)), this._cacheDuration);
            if (this._cacheDuration < MinimumCacheDuration) this._cacheDuration = MinimumCacheDuration;
            if (this._cacheDuration > MaximumCacheDuration) this._cacheDuration = MaximumCacheDuration;
            this._cacheSliding = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyCacheSliding)), this._cacheSliding);

            // SPARQL Expression Factories
            IEnumerable<INode> factories = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyExpressionFactory)));
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

            // SPARQL Property Function Factories
            factories = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyFunctionFactory)));
            foreach (INode factory in factories)
            {
                Object temp = ConfigurationLoader.LoadObject(g, factory);
                if (temp is IPropertyFunctionFactory)
                {
                    this._propertyFunctionFactories.Add((IPropertyFunctionFactory)temp);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load Handler Configuration as the RDF Configuration file specifies a value for the Handlers dnr:propertyFunctionFactory property which cannot be loaded as an object which is a SPARQL Property Function Factory");
                }
            }

            // Writer Properties
            this._writerCompressionLevel = ConfigurationLoader.GetConfigurationInt32(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyCompressionLevel)), this._writerCompressionLevel);
            this._writerDtds = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyDtdWriting)), this._writerDtds);
            this._writerHighSpeed = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyHighSpeedWriting)), this._writerHighSpeed);
            this._writerMultiThreading = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyMultiThreadedWriting)), this._writerMultiThreading);
            this._writerPrettyPrinting = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPrettyPrinting)), this._writerPrettyPrinting);
            this._writerAttributes = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyAttributeWriting)), this._writerAttributes);

            // Load in the Default Namespaces if specified
            INode nsNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyImportNamespacesFrom)));
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
        /// Gets whether there are any custom property function factories registered for this Handler
        /// </summary>
        public bool HasPropertyFunctionFactories
        {
            get
            {
                return (this._propertyFunctionFactories.Count > 0);
            }
        }

        /// <summary>
        /// Gets the custom property function factories registered for this Handler
        /// </summary>
        public IEnumerable<IPropertyFunctionFactory> PropertyFunctionFactories
        {
            get
            {
                return this._propertyFunctionFactories;
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
