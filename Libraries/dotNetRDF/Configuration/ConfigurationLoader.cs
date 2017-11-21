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
using System.Linq;
using System.Net;
using System.Reflection;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Operators;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// The Configuration Loader is responsible for the loading of Configuration information and objects based upon information encoded in a Graph but more generally may be used for the loading of any type of object whose configuration has been loaded in a Graph and for which a relevant <see cref="IObjectFactory">IObjectFactory</see> is available.
    /// </summary>
    /// <remarks>
    /// <para></para>
    /// </remarks>
    public class ConfigurationLoader : IConfigurationLoader
    {
        #region Constants

        /// <summary>
        /// Configuration Namespace URI
        /// </summary>
        public const String ConfigurationNamespace = "http://www.dotnetrdf.org/configuration#";

        /// <summary>
        /// Constants for URI Schemes with special meaning within the Configuration API
        /// </summary>
        public const String UriSchemeAppSettings = "appsetting",
                            UriSchemeConfigureOptions = "dotnetrdf-configure";

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String PropertyType = ConfigurationNamespace + "type",
                            PropertyImports = ConfigurationNamespace + "imports",
                            PropertyConfigure = ConfigurationNamespace  + "configure",
                            PropertyEnabled = ConfigurationNamespace + "enabled",
                            PropertyUser = ConfigurationNamespace + "user",
                            PropertyPassword = ConfigurationNamespace + "password",
                            PropertyCredentials = ConfigurationNamespace + "credentials",
                            PropertyUseCredentialsForProxy = ConfigurationNamespace + "useCredentialsForProxy",
                            // Manager connection properties
                            PropertyServer = ConfigurationNamespace + "server",
                            PropertyPort = ConfigurationNamespace + "port",
                            PropertyDatabase = ConfigurationNamespace + "database",
                            PropertyCatalog = ConfigurationNamespace + "catalogID",
                            PropertyStore = ConfigurationNamespace + "storeID",
                            PropertyQueryPath = ConfigurationNamespace + "queryPath",
                            PropertyUpdatePath = ConfigurationNamespace + "updatePath",
                            // Manager connection options
                            PropertyReadOnly = ConfigurationNamespace + "readOnly",
                            PropertyEnableUpdates = ConfigurationNamespace + "enableUpdates",
                            PropertyAsync = ConfigurationNamespace + "async",
                            PropertyLoadMode = ConfigurationNamespace + "loadMode",
                            PropertyEncryptConnection = ConfigurationNamespace + "encryptConnection",
                            PropertySkipParsing = ConfigurationNamespace + "skipParsing",
                            // Properties for associating Managers with other things
                            PropertyStorageProvider = ConfigurationNamespace + "storageProvider",
                            // Properties for associating Processors with other things
                            PropertyQueryProcessor = ConfigurationNamespace + "queryProcessor",
                            PropertyUpdateProcessor = ConfigurationNamespace + "updateProcessor",
                            PropertyProtocolProcessor = ConfigurationNamespace + "protocolProcessor",
                            PropertyUsingDataset = ConfigurationNamespace + "usingDataset",
                            // Properties for associating Stores and Graphs with other things
                            PropertyUsingStore = ConfigurationNamespace + "usingStore",
                            PropertyUsingGraph = ConfigurationNamespace + "usingGraph",
                            // Properties for setting low level storage for Triple Stores and Graphs
                            PropertyUsingTripleCollection = ConfigurationNamespace + "usingTripleCollection",
                            PropertyUsingGraphCollection = ConfigurationNamespace + "usingGraphCollection",
                            // Properties for defining where data comes from
                            PropertyFromFile = ConfigurationNamespace + "fromFile",
                            PropertyFromEmbedded = ConfigurationNamespace + "fromEmbedded",
                            PropertyFromUri = ConfigurationNamespace + "fromUri",
                            PropertyFromString = ConfigurationNamespace + "fromString",
                            PropertyFromDataset = ConfigurationNamespace + "fromDataset",
                            PropertyFromStore = ConfigurationNamespace + "fromStore",
                            PropertyFromGraph = ConfigurationNamespace + "fromGraph",
                            PropertyWithUri = ConfigurationNamespace + "withUri",
                            PropertyAssignUri = ConfigurationNamespace + "assignUri",
                            // Properties for Endpoints
                            PropertyEndpoint = ConfigurationNamespace + "endpoint",
                            PropertyEndpointUri = ConfigurationNamespace + "endpointUri",
                            PropertyQueryEndpointUri = ConfigurationNamespace + "queryEndpointUri",
                            PropertyUpdateEndpointUri = ConfigurationNamespace + "updateEndpointUri",
                            PropertyQueryEndpoint = ConfigurationNamespace + "queryEndpoint",
                            PropertyUpdateEndpoint = ConfigurationNamespace + "updateEndpoint",
                            PropertyDefaultGraphUri = ConfigurationNamespace + "defaultGraphUri",
                            PropertyNamedGraphUri = ConfigurationNamespace + "namedGraphUri",
                            PropertyUnionDefaultGraph = ConfigurationNamespace + "unionDefaultGraph",
                            PropertyProxy = ConfigurationNamespace + "proxy",
                            // Properties for reasoners
                            PropertyReasoner = ConfigurationNamespace + "reasoner",
                            PropertyOwlReasoner = ConfigurationNamespace + "owlReasoner",
                            // Properties for permissions
                            PropertyUserGroup = ConfigurationNamespace + "userGroup",
                            PropertyMember = ConfigurationNamespace + "member",
                            PropertyRequiresAuthentication = ConfigurationNamespace + "requiresAuthentication",
                            PropertyPermissionModel = ConfigurationNamespace + "permissionModel",
                            PropertyAllow = ConfigurationNamespace + "allow",
                            PropertyDeny = ConfigurationNamespace + "deny",
                            PropertyAction = ConfigurationNamespace + "action",
                            // Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
                            PropertyEnableCors = ConfigurationNamespace + "enableCors",
                            PropertySyntax = ConfigurationNamespace + "syntax",
                            PropertyTimeout = ConfigurationNamespace + "timeout",
                            PropertyPartialResults = ConfigurationNamespace + "partialResults",
                            PropertyShowErrors = ConfigurationNamespace + "showErrors",
                            PropertyHaltOnError = ConfigurationNamespace + "haltOnError",
                            PropertyShowQueryForm = ConfigurationNamespace + "showQueryForm",
                            PropertyShowUpdateForm = ConfigurationNamespace + "showUpdateForm",
                            PropertyDefaultQueryFile = ConfigurationNamespace + "defaultQueryFile",
                            PropertyDefaultUpdateFile = ConfigurationNamespace + "defaultUpdateFile",
                            PropertyIntroFile = ConfigurationNamespace + "introText",
                            PropertyStylesheet = ConfigurationNamespace + "stylesheet",
                            PropertyCacheDuration = ConfigurationNamespace + "cacheDuration",
                            PropertyCacheSliding = ConfigurationNamespace + "cacheSliding",
                            PropertyExpressionFactory = ConfigurationNamespace + "expressionFactory",
                            PropertyFunctionFactory = ConfigurationNamespace + "propertyFunctionFactory",
                            PropertyDescribeAlgorithm = ConfigurationNamespace + "describeAlgorithm",
                            PropertyServiceDescription = ConfigurationNamespace + "serviceDescription",
                            PropertyQueryOptimiser = ConfigurationNamespace + "queryOptimiser",
                            PropertyAlgebraOptimiser = ConfigurationNamespace + "algebraOptimiser",
                            // Properties for writers
                            PropertyCompressionLevel = ConfigurationNamespace + "compressionLevel",
                            PropertyPrettyPrinting = ConfigurationNamespace + "prettyPrinting",
                            PropertyHighSpeedWriting = ConfigurationNamespace + "highSpeedWriting",
                            PropertyDtdWriting = ConfigurationNamespace + "dtdWriting",
                            PropertyAttributeWriting = ConfigurationNamespace + "attributeWriting",
                            PropertyMultiThreadedWriting = ConfigurationNamespace + "multiThreadedWriting",
                            PropertyImportNamespacesFrom = ConfigurationNamespace + "importNamespacesFrom"
                            ;

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String ClassObjectFactory = ConfigurationNamespace + "ObjectFactory",
                            // Classes for Triple Stores and Graphs and their associated low level storage
                            ClassTripleStore = ConfigurationNamespace + "TripleStore",
                            ClassGraphCollection = ConfigurationNamespace + "GraphCollection",
                            ClassGraph = ConfigurationNamespace + "Graph",
                            ClassTripleCollection = ConfigurationNamespace + "TripleCollection",
                            // Classes for Storage Providers and Servers
                            ClassStorageServer = ConfigurationNamespace + "StorageServer",
                            ClassStorageProvider = ConfigurationNamespace + "StorageProvider",
                            // Classes for ASP.Net integration
                            ClassHttpHandler = ConfigurationNamespace + "HttpHandler",
                            // Classes for SPARQL features
                            ClassSparqlEndpoint = ConfigurationNamespace + "SparqlEndpoint",
                            ClassSparqlQueryEndpoint = ConfigurationNamespace + "SparqlQueryEndpoint",
                            ClassSparqlUpdateEndpoint = ConfigurationNamespace + "SparqlUpdateEndpoint",
                            ClassSparqlQueryProcessor = ConfigurationNamespace + "SparqlQueryProcessor",
                            ClassSparqlUpdateProcessor = ConfigurationNamespace + "SparqlUpdateProcessor",
                            ClassSparqlHttpProtocolProcessor = ConfigurationNamespace + "SparqlHttpProtocolProcessor",
                            ClassSparqlExpressionFactory = ConfigurationNamespace + "SparqlExpressionFactory",
                            ClassSparqlPropertyFunctionFactory = ConfigurationNamespace + "SparqlPropertyFunctionFactory",
                            ClassSparqlDataset = ConfigurationNamespace + "SparqlDataset",
                            ClassQueryOptimiser = ConfigurationNamespace + "QueryOptimiser",
                            ClassAlgebraOptimiser = ConfigurationNamespace + "AlgebraOptimiser",
                            ClassSparqlOperator = ConfigurationNamespace + "SparqlOperator",
                            // Classes for reasoners
                            ClassReasoner = ConfigurationNamespace + "Reasoner",
                            ClassOwlReasoner = ConfigurationNamespace + "OwlReasoner",
                            ClassProxy = ConfigurationNamespace + "Proxy",
                            // Classes for Users and permissions
                            ClassUserGroup = ConfigurationNamespace + "UserGroup",
                            ClassUser = ConfigurationNamespace + "User",
                            ClassPermission = ConfigurationNamespace + "Permission",
                            // Classes for Parsers and Serializers
                            ClassRdfParser = ConfigurationNamespace + "RdfParser",
                            ClassDatasetParser = ConfigurationNamespace + "DatasetParser",
                            ClassSparqlResultsParser = ConfigurationNamespace + "SparqlResultsParser",
                            ClassRdfWriter = ConfigurationNamespace + "RdfWriter",
                            ClassDatasetWriter = ConfigurationNamespace + "DatasetWriter",
                            ClassSparqlResultsWriter = ConfigurationNamespace + "SparqlResultsWriter";

        /// <summary>
        /// QName Constants for Default Types for some configuration classes
        /// </summary>
        public const String DefaultTypeTripleStore = "VDS.RDF.TripleStore",
                            DefaultTypeGraphCollection  = "VDS.RDF.GraphCollection",
                            DefaultTypeGraph = "VDS.RDF.Graph",
                            DefaultTypeTripleCollection = "VDS.RDF.TreeIndexedTripleCollection",
                            DefaultTypeSparqlQueryProcessor = "VDS.RDF.Query.LeviathanQueryProcessor",
                            DefaultTypeSparqlUpdateProcessor = "VDS.RDF.Update.LeviathanUpdateProcessor",
                            DefaultTypeSparqlHttpProtocolProcessor = "VDS.RDF.Update.Protocol.LeviathanProtocolProcessor",
                            DefaultTypeUserGroup = "VDS.RDF.Configuration.Permissions";

        #endregion

        #region Member Variables

        /// <summary>
        /// Cache for loaded objects
        /// </summary>
        private static Dictionary<CachedObjectKey, Object> _cache = new Dictionary<CachedObjectKey, object>();

        /// <summary>
        /// Set of built-in object factories that are automatically registered and used
        /// </summary>
        private static List<IObjectFactory> _factories = new List<IObjectFactory>
        {
            // Default Data Factories
            new GraphFactory(),
            new StoreFactory(),
            new CollectionFactory(),
            // Default Manager Factories
            new StorageFactory(),
            new DatasetFactory(),
            // Endpoint Factories
            new SparqlEndpointFactory(),
            // Processor Factories
            new QueryProcessorFactory(),
            new UpdateProcessorFactory(),
            new ProtocolProcessorFactory(),
            // User and Permission related Factories
            new UserGroupFactory(),
            new PermissionFactory(),
            new CredentialsFactory(),
            new ProxyFactory(),

            // SPARQL Extension related Factories
            new OptimiserFactory(),
            new ReasonerFactory(),
            new ExpressionFactoryFactory(),
            new PropertyFunctionFactoryFactory(),
            new OperatorFactory(),
            // ObjectFactory Factory
            new ObjectFactoryFactory(),
            // Parser and Writer Factories
            new ParserFactory(),
            new WriterFactory(),
        };
        /// <summary>
        /// Path resolver
        /// </summary>
        private static IPathResolver _resolver;

        /// <summary>
        /// Gets or sets the provider of external settings
        /// </summary>
        /// <remarks>On .NET Framework defaults to a reader of &lt;appSettings&gt; configuration section</remarks>
        public static ISettingsProvider SettingsProvider { get; set; }

        #endregion

        static ConfigurationLoader()
        {
#if NET40
            SettingsProvider = new ConfigurationManagerSettingsProvider();
#endif
        }

        #region Graph Loading and Auto-Configuration

        /// <summary>
        /// Loads a Configuration Graph and applies auto-configuration
        /// </summary>
        /// <param name="u">URI to load from</param>
        /// <returns></returns>
        public static IGraph LoadConfiguration(Uri u)
        {
            return LoadConfiguration(u, true);
        }

        /// <summary>
        /// Loads a Configuration Graph and applies auto-configuration if desired
        /// </summary>
        /// <param name="u">URI to load from</param>
        /// <param name="autoConfigure">Whether to apply auto-configuration</param>
        /// <returns></returns>
        public static IGraph LoadConfiguration(Uri u, bool autoConfigure)
        {
            Graph g = new Graph();
            UriLoader.Load(g, u);
            return LoadCommon(g, g.CreateUriNode(u), autoConfigure);
        }

        /// <summary>
        /// Loads a Configuration Graph and applies auto-configuration
        /// </summary>
        /// <param name="file">File to load from</param>
        /// <returns></returns>
        public static IGraph LoadConfiguration(String file)
        {
            return LoadConfiguration(file, true);
        }

        /// <summary>
        /// Loads a Configuration Graph and applies auto-configuration if desired
        /// </summary>
        /// <param name="file">File to load from</param>
        /// <param name="autoConfigure">Whether to apply auto-configuration</param>
        /// <returns></returns>
        public static IGraph LoadConfiguration(String file, bool autoConfigure)
        {
            Graph g = new Graph();
            FileLoader.Load(g, file);
            return LoadCommon(g, new INode[] { g.CreateLiteralNode(file), g.CreateLiteralNode(Path.GetFileName(file)) }, autoConfigure);
        }

        /// <summary>
        /// Loads a Configuration Graph and applies auto-configuration
        /// </summary>
        /// <param name="resource">Embedded Resource to load</param>
        /// <returns></returns>
        public static IGraph LoadEmbeddedConfiguration(String resource)
        {
            return LoadEmbeddedConfiguration(resource, true);
        }

        /// <summary>
        /// Loads a Configuration Graph and applies auto-configuration if desired
        /// </summary>
        /// <param name="resource">Embedded Resource to load</param>
        /// <param name="autoConfigure">Whether to apply auto-configuration</param>
        /// <returns></returns>
        public static IGraph LoadEmbeddedConfiguration(String resource, bool autoConfigure)
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, resource);
            return LoadCommon(g, g.CreateLiteralNode(resource), autoConfigure);
        }

        /// <summary>
        /// Common loader for Configuration Graphs, handles the resolution of dnr:imports and applies the auto-configuration if selected
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="source">Source the graph originated from</param>
        /// <param name="autoConfigure">Whether to apply auto-configuration</param>
        /// <returns></returns>
        private static IGraph LoadCommon(IGraph g, INode source, bool autoConfigure)
        {
            return LoadCommon(g, source.AsEnumerable(), autoConfigure);
        }

        /// <summary>
        /// Common loader for Configuration Graphs, handles the resolution of dnr:imports and applies the auto-configuration if selected
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="sources">Sources the graph originated from</param>
        /// <param name="autoConfigure">Whether to apply auto-configuration</param>
        /// <returns></returns>
        private static IGraph LoadCommon(IGraph g, IEnumerable<INode> sources, bool autoConfigure)
        {
            // Add initial sources to already imported list
            HashSet<INode> imported = new HashSet<INode>();
            foreach (INode source in sources)
            {
                imported.Add(source);
            }

            // Find initial imports
            INode imports = g.CreateUriNode(UriFactory.Create(PropertyImports));
            Queue<INode> importQueue = new Queue<INode>();
            foreach (INode importData in g.GetTriplesWithPredicate(imports).Select(t => t.Object))
            {
                importQueue.Enqueue(importData);
            }

            while (importQueue.Count > 0)
            {
                // Load data from imported configuration graph
                INode importData = importQueue.Dequeue();
                Graph data = new Graph();
                switch (importData.NodeType)
                {
                    case NodeType.Uri:
                        importData = ResolveAppSetting(g, importData);
                        if (!imported.Contains(importData))
                        {
                            UriLoader.Load(data, ((IUriNode)importData).Uri);
                            imported.Add(importData);
                        }
                        break;

                    case NodeType.Literal:
                        if (!imported.Contains(importData))
                        {
                            FileLoader.Load(data, ResolvePath(((ILiteralNode)importData).Value));
                            imported.Add(importData);
                        }
                        break;

                    default:
                        throw new DotNetRdfConfigurationException("Invalid dnr:imports target " + importData.ToString() + ", dnr:imports may only be used to point to an object which is a URI/Literal.  If sing Silverlight only Literals are currently permitted.");
                }

                // Scan for nested imports
                foreach (INode nestedImport in data.GetTriplesWithPredicate(imports).Select(t => t.Object))
                {
                    if (!imported.Contains(nestedImport)) importQueue.Enqueue(nestedImport);
                }
                // Merge into final graph
                g.Merge(data);
            }

            // Apply auto-configuration if requested
            if (autoConfigure) AutoConfigure(g);

            return g;
        }

        /// <summary>
        /// Given a Configuration Graph applies all available auto-configuration based on the contents of the graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        public static void AutoConfigure(IGraph g)
        {
            AutoConfigureObjectFactories(g);
            AutoConfigureReadersAndWriters(g);
            AutoConfigureSparqlOperators(g);
            AutoConfigureStaticOptions(g);
        }

        /// <summary>
        /// Given a Configuration Graph will detect and configure Object Factories defined in the configuration
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        public static void AutoConfigureObjectFactories(IGraph g)
        {
            IUriNode rdfType = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode objLoader = g.CreateUriNode(UriFactory.Create(ClassObjectFactory));

            foreach (INode objNode in g.GetTriplesWithPredicateObject(rdfType, objLoader).Select(t => t.Subject))
            {
                Object temp = LoadObject(g, objNode);
                if (temp is IObjectFactory)
                {
                    AddObjectFactory((IObjectFactory)temp);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-detection of Object Loaders failed as the Node '" + objNode.ToString() + "' was stated to be rdf:type of dnr:ObjectFactory but failed to load as an object which implements the IObjectFactory interface");
                }
            }
        }

        /// <summary>
        /// Given a Configuration Graph will detect and configure static options that are specified using the dnr:configure property with special &lt;dotnetrdf-configure:Class/Property&gt; subject URIs
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <remarks>
        /// <para>
        /// An example of using this mechanism to configure a static option is as follows:
        /// </para>
        /// <pre>
        /// &lt;dotnetrdf-configure:VDS.RDF.Options#UsePLinqEvaluation&gt; dnr:configure false .
        /// </pre>
        /// <para>
        /// Class and property names must be fully qualified, to specify static options outside of dotNetRDF itself you can add an additional path segment with the assembly name after the initial configure keyword.  If the class/property does not exist or the value of the literal cannot be appropriately converted to the type of the property then an exception will be thrown.  If there is a problem setting the property (e.g. it does not have a public setter) then an exception will be thrown.
        /// </para>
        /// </remarks>
        public static void AutoConfigureStaticOptions(IGraph g)
        {
            IUriNode dnrConfigure = g.CreateUriNode(UriFactory.Create(PropertyConfigure));

            foreach (Triple t in g.GetTriplesWithPredicate(dnrConfigure))
            {
                if (t.Subject.NodeType == NodeType.Uri)
                {
                    Uri propertyUri = ((IUriNode)t.Subject).Uri;
                    if (propertyUri.Scheme.Equals(UriSchemeConfigureOptions))
                    {
                        // Parse the Class and Property out of the URI
                        String className = propertyUri.AbsolutePath;
                        if (propertyUri.Fragment.Length <= 1) throw new DotNetRdfConfigurationException("Malformed Configure Options URI used as subject for a dnr:configure triple, <" + propertyUri.AbsoluteUri + "> is missing the fragment identifier to specify the property name");
                        String propName = propertyUri.Fragment.Substring(1);

                        // Get the Value we are setting to this property
                        INode value = t.Object;

                        // Get the type whose static option we are attempting to change
                        Type type = Type.GetType(className);
                        if (type == null) throw new DotNetRdfConfigurationException("Malformed Configure Options URI used as a subject for a dnr:configure triple, <" + propertyUri.AbsoluteUri + "> specifies a class '" + className + "' which could not be loaded.  Please ensure the type name is fully qualified");

                        // Get the property in question
                        PropertyInfo property = type.GetProperty(propName);
                        if (property == null) throw new DotNetRdfConfigurationException("Malformed Configure Options URI used as a subject for a dnr:configure triple, <" + propertyUri.AbsoluteUri + "> specifies a property '" + propName + "' which does not exist or is not static");
                        if (!property.GetSetMethod().IsStatic) throw new DotNetRdfConfigurationException("Malformed Configure Options URI used as a subject for a dnr:configure triple, <" + propertyUri.AbsoluteUri + "> specifies a property '" + propName + "' which is not static");
                        Type valueType = property.PropertyType;
                        try
                        {
                            IValuedNode valueNode = value.AsValuedNode();
                            if (valueType.Equals(typeof(int)))
                            {
                                int intValue = (int)valueNode.AsInteger();
                                property.SetValue(null, intValue, null);
                            }
                            else if (valueType.Equals(typeof(long)))
                            {
                                long longValue = valueNode.AsInteger();
                                property.SetValue(null, longValue, null);
                            }
                            else if (valueType.Equals(typeof(bool)))
                            {
                                bool boolValue = valueNode.AsBoolean();
                                property.SetValue(null, boolValue, null);
                            }
                            else if (valueType.Equals(typeof(String)))
                            {
                                property.SetValue(null, valueNode.AsString(), null);
                            }
                            else if (valueType.Equals(typeof(Uri)))
                            {
                                Uri uriValue = (value.NodeType == NodeType.Uri ? ((IUriNode)value).Uri : UriFactory.Create(valueNode.AsString()));
                                property.SetValue(null, uriValue, null);
                            }
#if NETCORE
                            else if (valueType.IsEnum())
#else
                            else if (valueType.IsEnum)
#endif
                            {
                                if (value.NodeType != NodeType.Literal) throw new DotNetRdfConfigurationException("Malformed dnf:configure triple - " + t + " - the object must be a literal when the property being set has a enumeration type");
                                Object enumVal = Enum.Parse(valueType, valueNode.AsString(), true);
                                property.SetValue(null, enumVal, null);
                            }
                            else
                            {
                                throw new DotNetRdfConfigurationException("Configure Options URIs can currently only be used to configure static properties with int, long, bool, String, URI or enumeration typed values.  The URI <" + propertyUri.AbsoluteUri + "> points to a property with the unsupported type " + valueType.FullName);
                            }
                        }
                        catch (DotNetRdfConfigurationException)
                        {
                            // Don't rewrap
                            throw;
                        }
                        catch (Exception ex)
                        {
                            // Rewrap as Configuration error
                            throw new DotNetRdfConfigurationException("Unexpected error trying to set the static property identified by the Configure Options URI <" + propertyUri.AbsoluteUri + ">, please ensure that the lexical form of the value being set is valid for the property you are trying to set", ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Given a Configuration Graph will detect Readers and Writers for RDF and SPARQL syntaxes and register them with <see cref="MimeTypesHelper">MimeTypesHelper</see>.  This will cause the library defaults to be overridden where appropriate.
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        public static void AutoConfigureReadersAndWriters(IGraph g)
        {
            IUriNode rdfType = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode desiredType = g.CreateUriNode(UriFactory.Create(ClassRdfParser));
            INode formatMimeType = g.CreateUriNode(UriFactory.Create("http://www.w3.org/ns/formats/media_type"));
            INode formatExtension = g.CreateUriNode(UriFactory.Create("http://www.w3.org/ns/formats/preferred_suffix"));
            Object temp;
            String[] mimeTypes, extensions;

            // Load RDF Parsers
            foreach (INode objNode in g.GetTriplesWithPredicateObject(rdfType, desiredType).Select(t => t.Subject))
            {
                temp = LoadObject(g, objNode);
                if (temp is IRdfReader)
                {
                    // Get the formats to associate this with
                    mimeTypes = GetConfigurationArray(g, objNode, formatMimeType);
                    if (mimeTypes.Length == 0) throw new DotNetRdfConfigurationException("Auto-configuration of Readers and Writers failed as the Parser specified by the Node '" + objNode.ToString() + "' is not associated with any MIME types");
                    extensions = GetConfigurationArray(g, objNode, formatExtension);

                    // Register
                    MimeTypesHelper.RegisterParser((IRdfReader)temp, mimeTypes, extensions);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-configuration of Readers and Writers failed as the Node '" + objNode.ToString() + "' was stated to be rdf:type of dnr:RdfParser but failed to load as an object which implements the required IRdfReader interface");
                }
            }

            // Load Dataset parsers
            desiredType = g.CreateUriNode(UriFactory.Create(ClassDatasetParser));
            foreach (INode objNode in g.GetTriplesWithPredicateObject(rdfType, desiredType).Select(t => t.Subject))
            {
                temp = LoadObject(g, objNode);
                if (temp is IStoreReader)
                {
                    // Get the formats to associate this with
                    mimeTypes = GetConfigurationArray(g, objNode, formatMimeType);
                    if (mimeTypes.Length == 0) throw new DotNetRdfConfigurationException("Auto-configuration of Readers and Writers failed as the Parser specified by the Node '" + objNode.ToString() + "' is not associated with any MIME types");
                    extensions = GetConfigurationArray(g, objNode, formatExtension);

                    // Register
                    MimeTypesHelper.RegisterParser((IStoreReader)temp, mimeTypes, extensions);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-configuration of Readers and Writers failed as the Node '" + objNode.ToString() + "' was stated to be rdf:type of dnr:DatasetParser but failed to load as an object which implements the required IStoreReader interface");
                }
            }

            // Load SPARQL Result parsers
            desiredType = g.CreateUriNode(UriFactory.Create(ClassSparqlResultsParser));
            foreach (INode objNode in g.GetTriplesWithPredicateObject(rdfType, desiredType).Select(t => t.Subject))
            {
                temp = LoadObject(g, objNode);
                if (temp is ISparqlResultsReader)
                {
                    // Get the formats to associate this with
                    mimeTypes = GetConfigurationArray(g, objNode, formatMimeType);
                    if (mimeTypes.Length == 0) throw new DotNetRdfConfigurationException("Auto-configuration of Readers and Writers failed as the Parser specified by the Node '" + objNode.ToString() + "' is not associated with any MIME types");
                    extensions = GetConfigurationArray(g, objNode, formatExtension);

                    // Register
                    MimeTypesHelper.RegisterParser((ISparqlResultsReader)temp, mimeTypes, extensions);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-configuration of Readers and Writers failed as the Node '" + objNode.ToString() + "' was stated to be rdf:type of dnr:SparqlResultsParser but failed to load as an object which implements the required ISparqlResultsReader interface");
                }
            }

            // Load RDF Writers
            desiredType = g.CreateUriNode(UriFactory.Create(ClassRdfWriter));
            foreach (INode objNode in g.GetTriplesWithPredicateObject(rdfType, desiredType).Select(t => t.Subject))
            {
                temp = LoadObject(g, objNode);
                if (temp is IRdfWriter)
                {
                    // Get the formats to associate this with
                    mimeTypes = GetConfigurationArray(g, objNode, formatMimeType);
                    if (mimeTypes.Length == 0) throw new DotNetRdfConfigurationException("Auto-configuration of Readers and Writers failed as the Writer specified by the Node '" + objNode.ToString() + "' is not associated with any MIME types");
                    extensions = GetConfigurationArray(g, objNode, formatExtension);

                    // Register
                    MimeTypesHelper.RegisterWriter((IRdfWriter)temp, mimeTypes, extensions);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-configuration of Readers and Writers failed as the Node '" + objNode.ToString() + "' was stated to be rdf:type of dnr:RdfWriter but failed to load as an object which implements the required IRdfWriter interface");
                }
            }

            // Load Dataset Writers
            desiredType = g.CreateUriNode(UriFactory.Create(ClassDatasetWriter));
            foreach (INode objNode in g.GetTriplesWithPredicateObject(rdfType, desiredType).Select(t => t.Subject))
            {
                temp = LoadObject(g, objNode);
                if (temp is IStoreWriter)
                {
                    // Get the formats to associate this with
                    mimeTypes = GetConfigurationArray(g, objNode, formatMimeType);
                    if (mimeTypes.Length == 0) throw new DotNetRdfConfigurationException("Auto-configuration of Readers and Writers failed as the Writer specified by the Node '" + objNode.ToString() + "' is not associated with any MIME types");
                    extensions = GetConfigurationArray(g, objNode, formatExtension);

                    // Register
                    MimeTypesHelper.RegisterWriter((IStoreWriter)temp, mimeTypes, extensions);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-configuration of Readers and Writers failed as the Node '" + objNode.ToString() + "' was stated to be rdf:type of dnr:DatasetWriter but failed to load as an object which implements the required IStoreWriter interface");
                }
            }

            // Load SPARQL Result Writers
            desiredType = g.CreateUriNode(UriFactory.Create(ClassDatasetWriter));
            foreach (INode objNode in g.GetTriplesWithPredicateObject(rdfType, desiredType).Select(t => t.Subject))
            {
                temp = LoadObject(g, objNode);
                if (temp is ISparqlResultsWriter)
                {
                    // Get the formats to associate this with
                    mimeTypes = GetConfigurationArray(g, objNode, formatMimeType);
                    if (mimeTypes.Length == 0) throw new DotNetRdfConfigurationException("Auto-configuration of Readers and Writers failed as the Writer specified by the Node '" + objNode.ToString() + "' is not associated with any MIME types");
                    extensions = GetConfigurationArray(g, objNode, formatExtension);

                    // Register
                    MimeTypesHelper.RegisterWriter((ISparqlResultsWriter)temp, mimeTypes, extensions);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-configuration of Readers and Writers failed as the Node '" + objNode.ToString() + "' was stated to be rdf:type of dnr:SparqlResultsWriter but failed to load as an object which implements the required ISparqlResultsWriter interface");
                }
            }
        }

        /// <summary>
        /// Given a Configuration Graph will detect and configure SPARQL Operators
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        public static void AutoConfigureSparqlOperators(IGraph g)
        {
            INode rdfType = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType)),
                  operatorClass = g.CreateUriNode(UriFactory.Create(ClassSparqlOperator)),
                  enabled = g.CreateUriNode(UriFactory.Create(PropertyEnabled));

            foreach (Triple t in g.GetTriplesWithPredicateObject(rdfType, operatorClass))
            {
                Object temp = LoadObject(g, t.Subject);
                if (temp is ISparqlOperator)
                {
                    bool enable = GetConfigurationBoolean(g, t.Subject, enabled, true);
                    if (enable)
                    {
                        SparqlOperators.AddOperator((ISparqlOperator)temp);
                    }
                    else
                    {
                        SparqlOperators.RemoveOperatorByType((ISparqlOperator)temp);
                    }
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-configuration of SPARQL Operators failed as the Operator specified by the Node '" + t.Subject.ToString() + "' does not implement the required ISparqlOperator interface");
                }
            }
        }

        #endregion

        #region Object Loading

        /// <summary>
        /// Checks for circular references and throws an error if there is one
        /// </summary>
        /// <param name="a">Object you are attempting to load</param>
        /// <param name="b">Object being referenced</param>
        /// <param name="property">QName for the property that makes the reference</param>
        /// <remarks>
        /// <para>
        /// If the Object you are trying to load and the Object you need to load are equal then this is a circular reference and an error is thrown
        /// </para>
        /// <para>
        /// The <see cref="ConfigurationLoader">ConfigurationLoader</see> is not currently capable of detecting more subtle circular references
        /// </para>
        /// </remarks>
        public static bool CheckCircularReference(INode a, INode b, String property)
        {
            if (a.Equals(b))
            {
                throw new DotNetRdfConfigurationException("Unable to load the Object identified by the Node '" + a.ToString() + "' as one of the values for the " + property + " property is a circular reference to the Object we are attempting to load");
            }
            return false;
        }

        /// <summary>
        /// Creates a URI Node that refers to some Configuration property/type
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="qname">QName of the property/type</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// The QName provides should be of the form <strong>dnr:qname</strong> - the <strong>dnr</strong> prefix will be automatically be considered to be to the Configuration Namespace which is defined by the <see cref="ConfigurationLoader.ConfigurationNamespace">ConfigurationNamespace</see> constant.
        /// </para>
        /// <para>
        /// This function uses caching to ensure that URI Nodes aren't needlessly recreated in order to save memory.
        /// </para>
        /// </remarks>
        [Obsolete("This method is obsolete and should no longer be used, constants are now URIs so you should just create URI Nodes directly on your Configuration Graph", false)]
        public static INode CreateConfigurationNode(IGraph g, String qname)
        {
            return g.CreateUriNode(UriFactory.Create(qname));
        }

        /// <summary>
        /// Clears the Object Loader cache (this is not recommended)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method should only be invoked in cases where you have attempted to load an object and some error occurred which was external to dotNetRDF e.g. network connectivity problem and 
        /// </para>
        /// </remarks>
        public static void ClearCache()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Gets all the values given for a property of a given Object in the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="property">Property Node</param>
        /// <returns>
        /// Enumeration of values given for the property for the Object
        /// </returns>
        public static IEnumerable<INode> GetConfigurationData(IGraph g, INode objNode, INode property)
        {
            return g.GetTriplesWithSubjectPredicate(objNode, property).Select(t => ResolveAppSetting(g, t.Object));
        }

        /// <summary>
        /// Gets all the literal values given for a property of a given Object in the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="property">Property Node</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Only returns the value part of Literal Nodes which are given as values for the property i.e. ignores all non-Literals and discards any language/data type from Literals
        /// </para>
        /// </remarks>
        public static String[] GetConfigurationArray(IGraph g, INode objNode, INode property)
        {
            return g.GetTriplesWithSubjectPredicate(objNode, property).Select(t => t.Object).Where(n => n.NodeType == NodeType.Literal).Select(n => ((ILiteralNode)n).Value).ToArray();
        }

        /// <summary>
        /// Gets the first value given for a property of a given Object in the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="property">Property Node</param>
        /// <returns>
        /// First value given for the property of the Object
        /// </returns>
        public static INode GetConfigurationNode(IGraph g, INode objNode, INode property)
        {
            INode temp = g.GetTriplesWithSubjectPredicate(objNode, property).Select(t => t.Object).FirstOrDefault();
            return ResolveAppSetting(g, temp);
        }

        /// <summary>
        /// Gets the first value given for the first found property of a given Object in the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="properties">Properties</param>
        /// <returns>
        /// First value given for the first property of the Object which is matched
        /// </returns>
        public static INode GetConfigurationNode(IGraph g, INode objNode, IEnumerable<INode> properties)
        {
            return properties.Select(p => GetConfigurationNode(g, objNode, p)).Where(n => n != null).FirstOrDefault();
        }

        /// <summary>
        /// Gets the String value or null of the first instance of a property for a given Object in the Configuration Graph where the value for the property is a Literal Node
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="property">Property Node</param>
        /// <returns>
        /// <para>
        /// String value of the first instance of the property or a null if no values or not a literal value
        /// </para>
        /// <para>
        /// If you want the String value regardless of Node type then use the <see cref="ConfigurationLoader.GetConfigurationValue(IGraph,INode,INode)">GetConfigurationValue</see> function instead
        /// </para>
        /// </returns>
        public static String GetConfigurationString(IGraph g, INode objNode, INode property)
        {
            INode n = g.GetTriplesWithSubjectPredicate(objNode, property).Select(t => t.Object).FirstOrDefault();
            if (n == null) return null;
            if (n.NodeType == NodeType.Literal)
            {
                return ((ILiteralNode)n).Value;
            }
            INode temp = ResolveAppSetting(g, n);
            if (temp == null) return null;
            if (temp.NodeType == NodeType.Literal)
            {
                return ((ILiteralNode)temp).Value;
            }
            return null;
        }

        /// <summary>
        /// Gets the String value or null of the first instance of the first property for a given Object in the Configuration Graph where the value for the property is a Literal Node
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="properties">Property Nodes</param>
        /// <returns>
        /// <para>
        /// String value of the first instance of the first property or a null if no values or not a literal value
        /// </para>
        /// <para>
        /// If you want the String value regardless of Node type then use the <see cref="ConfigurationLoader.GetConfigurationValue(IGraph,INode,IEnumerable{INode})">GetConfigurationValue</see> function instead
        /// </para>
        /// </returns>
        public static String GetConfigurationString(IGraph g, INode objNode, IEnumerable<INode> properties)
        {
            return properties.Select(p => GetConfigurationString(g, objNode, p)).Where(s => s != null).FirstOrDefault();
        }

        /// <summary>
        /// Gets the String value or null of the first instance of a property for a given Object in the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="property">Property Node</param>
        /// <returns></returns>
        public static String GetConfigurationValue(IGraph g, INode objNode, INode property)
        {
            INode n = g.GetTriplesWithSubjectPredicate(objNode, property).Select(t => t.Object).FirstOrDefault();
            if (n == null) return null;
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    return n.ToString();
                case NodeType.Literal:
                    return ((ILiteralNode)n).Value;
                case NodeType.Uri:
                    INode temp = ResolveAppSetting(g, n);
                    if (temp == null) return null;
                    if (temp.NodeType == NodeType.Literal)
                    {
                        return ((ILiteralNode)temp).Value;
                    }
                    else
                    {
                        return temp.ToString();
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the String value or null of the first instance of the first property for a given Object in the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="properties">Property Nodes</param>
        /// <returns></returns>
        public static String GetConfigurationValue(IGraph g, INode objNode, IEnumerable<INode> properties)
        {
            return properties.Select(p => GetConfigurationValue(g, objNode, p)).Where(s => s != null).FirstOrDefault();
        }

        /// <summary>
        /// Gets the Boolean value or a given default of the first instance of a property for a given Object in the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="property">Property Node</param>
        /// <param name="defValue">Default Value to return if there is no valid boolean value</param>
        /// <returns>
        /// If there is a valid boolean value for the property then that is returned, in any other case the given <paramref name="defValue">Default Value</paramref> is returned
        /// </returns>
        public static bool GetConfigurationBoolean(IGraph g, INode objNode, INode property, bool defValue)
        {
            INode n = g.GetTriplesWithSubjectPredicate(objNode, property).Select(t => t.Object).FirstOrDefault();
            if (n == null) return defValue;

            // Resolve AppSettings
            if (n.NodeType != NodeType.Literal)
            {
                n = ResolveAppSetting(g, n);
                if (n == null) return defValue;
            }

            if (n.NodeType == NodeType.Literal)
            {
                if (bool.TryParse(((ILiteralNode)n).Value, out var temp))
                {
                    return temp;
                }
                return defValue;
            }
            return defValue;
        }

        /// <summary>
        /// Gets the Boolean value or a given default of the first instance of the first property for a given Object in the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="properties">Property Nodes</param>
        /// <param name="defValue">Default Value to return if there is no valid boolean value</param>
        /// <returns>
        /// If there is a valid boolean value for any property then that is returned, in any other case the given <paramref name="defValue">Default Value</paramref> is returned
        /// </returns>
        public static bool GetConfigurationBoolean(IGraph g, INode objNode, IEnumerable<INode> properties, bool defValue)
        {
            foreach (INode property in properties)
            {
                INode n = g.GetTriplesWithSubjectPredicate(objNode, property).Select(t => t.Object).FirstOrDefault();
                if (n == null) continue;

                // Resolve AppSettings
                if (n.NodeType != NodeType.Literal)
                {
                    n = ResolveAppSetting(g, n);
                    if (n == null) continue;
                }

                if (n.NodeType == NodeType.Literal)
                {
                    if (bool.TryParse(((ILiteralNode)n).Value, out var temp))
                    {
                        return temp;
                    }
                }
            }
            return defValue;
        }

        /// <summary>
        /// Gets the 64 bit Integer value or a given default of the first instance of a property for a given Object in the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="property">Property Node</param>
        /// <param name="defValue">Default Value to return if there is no valid boolean value</param>
        /// <returns>
        /// If there is a valid integer value for the property then that is returned, in any other case the given <paramref name="defValue">Default Value</paramref> is returned
        /// </returns>
        public static long GetConfigurationInt64(IGraph g, INode objNode, INode property, long defValue)
        {
            INode n = g.GetTriplesWithSubjectPredicate(objNode, property).Select(t => t.Object).FirstOrDefault();
            if (n == null) return defValue;

            // Resolve AppSettings
            if (n.NodeType != NodeType.Literal)
            {
                n = ResolveAppSetting(g, n);
                if (n == null) return defValue;
            }

            if (n.NodeType == NodeType.Literal)
            {
                if (long.TryParse(((ILiteralNode)n).Value, out var temp))
                {
                    return temp;
                }
                return defValue;
            }
            return defValue;
        }

        /// <summary>
        /// Gets the 64 bit Integer value or a given default of the first instance of the first property for a given Object in the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="properties">Property Nodes</param>
        /// <param name="defValue">Default Value to return if there is no valid boolean value</param>
        /// <returns>
        /// If there is a valid integer value for any property then that is returned, in any other case the given <paramref name="defValue">Default Value</paramref> is returned
        /// </returns>
        public static long GetConfigurationInt64(IGraph g, INode objNode, IEnumerable<INode> properties, long defValue)
        {
            foreach (INode property in properties)
            {
                INode n = g.GetTriplesWithSubjectPredicate(objNode, property).Select(t => t.Object).FirstOrDefault();
                if (n == null) continue;

                // Resolve AppSettings
                if (n.NodeType != NodeType.Literal)
                {
                    n = ResolveAppSetting(g, n);
                    if (n == null) continue;
                }

                if (n.NodeType == NodeType.Literal)
                {
                    if (long.TryParse(((ILiteralNode)n).Value, out var temp))
                    {
                        return temp;
                    }
                }
            }
            return defValue;
        }

        /// <summary>
        /// Gets the 64 bit Integer value or a given default of the first instance of a property for a given Object in the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="property">Property Node</param>
        /// <param name="defValue">Default Value to return if there is no valid boolean value</param>
        /// <returns>
        /// If there is a valid integer value for the property then that is returned, in any other case the given <paramref name="defValue">Default Value</paramref> is returned
        /// </returns>
        public static int GetConfigurationInt32(IGraph g, INode objNode, INode property, int defValue)
        {
            INode n = g.GetTriplesWithSubjectPredicate(objNode, property).Select(t => t.Object).FirstOrDefault();
            if (n == null) return defValue;

            // Resolve AppSettings
            if (n.NodeType != NodeType.Literal)
            {
                n = ResolveAppSetting(g, n);
                if (n == null) return defValue;
            }

            if (n.NodeType == NodeType.Literal)
            {
                if (int.TryParse(((ILiteralNode)n).Value, out var temp))
                {
                    return temp;
                }
                return defValue;
            }
            return defValue;
        }

        /// <summary>
        /// Gets the 64 bit Integer value or a given default of the first instance of the first property for a given Object in the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="properties">Property Nodes</param>
        /// <param name="defValue">Default Value to return if there is no valid boolean value</param>
        /// <returns>
        /// If there is a valid integer value for any property then that is returned, in any other case the given <paramref name="defValue">Default Value</paramref> is returned
        /// </returns>
        public static int GetConfigurationInt32(IGraph g, INode objNode, IEnumerable<INode> properties, int defValue)
        {
            foreach (INode property in properties)
            {
                INode n = g.GetTriplesWithSubjectPredicate(objNode, property).Select(t => t.Object).FirstOrDefault();
                if (n == null) continue;

                // Resolve AppSettings
                if (n.NodeType != NodeType.Literal)
                {
                    n = ResolveAppSetting(g, n);
                    if (n == null) continue;
                }

                if (n.NodeType == NodeType.Literal)
                {
                    if (int.TryParse(((ILiteralNode)n).Value, out var temp))
                    {
                        return temp;
                    }
                }
            }
            return defValue;
        }

        /// <summary>
        /// Gets the Username and Password specified for a given Object
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="allowCredentials">Whether settings may be specified using the dnr:credentials property</param>
        /// <param name="user">Username</param>
        /// <param name="pwd">Password</param>
        /// <remarks>
        /// Username and/or Password will be null if there is no value specified for the relevant properties
        /// </remarks>
        public static void GetUsernameAndPassword(IGraph g, INode objNode, bool allowCredentials, out String user, out String pwd)
        {
            INode propUser = g.CreateUriNode(UriFactory.Create(PropertyUser)),
                  propPwd = g.CreateUriNode(UriFactory.Create(PropertyPassword));

            user = GetConfigurationString(g, objNode, propUser);
            pwd = GetConfigurationString(g, objNode, propPwd);
            if ((user == null || pwd == null) && allowCredentials)
            {
                // Have they been specified as credentials instead?
                INode propCredentials = g.CreateUriNode(UriFactory.Create(PropertyCredentials));
                INode credObj = GetConfigurationNode(g, objNode, propCredentials);
                if (credObj != null)
                {
                    NetworkCredential credentials = (NetworkCredential)LoadObject(g, credObj, typeof(NetworkCredential));
                    user = credentials.UserName;
                    pwd = credentials.Password;
                }
            }
        }

        /// <summary>
        /// Gets whether the given Object has already been loaded and cached
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <returns></returns>
        /// <remarks>
        /// If this returns true then loading that object again should be essentially instantaneous as it will come from the cache
        /// </remarks>
        public static bool IsCached(IGraph g, INode objNode)
        {
            CachedObjectKey key = new CachedObjectKey(objNode, g);
            return _cache.ContainsKey(key);
        }

        /// <summary>
        /// Loads the Object identified by the given Node as an object of the given type based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Callers of this method should be careful to check that the Object returned is of a usable type to them.  The Target Type parameter does not guarantee that the return value is of that type it is only used to determine which registered instances of <see cref="IObjectFactory">IObjectFactory</see> are potentially capable of creating the desired Object
        /// </para>
        /// <para>
        /// Callers should also take care that any Objects returned from this method are disposed of when the caller no longer has a use for them as otherwise the reference kept in the cache here will cause the Object to remain in-memory consuming resources
        /// </para>
        /// </remarks>
        public static Object LoadObject(IGraph g, INode objNode, Type targetType)
        {
            if (targetType == null) throw new DotNetRdfConfigurationException("Unable to load the Object identified by the Node '" + objNode.ToString() + "' as a null target type was provided - this may be due to a failure to specify a fully qualified type name with the dnr:type property for this object");
            if (objNode == null) throw new DotNetRdfConfigurationException("Unable to load an Object as a null Object Node was provided");

            if (objNode.NodeType == NodeType.GraphLiteral || objNode.NodeType == NodeType.Literal)
            {
                throw new DotNetRdfConfigurationException("Unable to load an Object as the Object Node was not a URI/Blank Node as required");
            }

            // Use an Object caching mechanism to avoid instantiating the same thing multiple times since this could be VERY costly
            CachedObjectKey key = new CachedObjectKey(objNode, g);
            if (_cache.ContainsKey(key))
            {
                if (_cache[key] == null)
                {
                    // This means we've begun trying to cache the Object but haven't loaded it yet
                    // i.e. we've encountered an indirect circular reference or the caller failed to check
                    // for direct circular references with the CheckCircularReference() method
                    throw new DotNetRdfConfigurationException("Unable to load the Object identified by the Node '" + objNode.ToString() + "' as we have already started trying to load this Object which indicates that your Configuration Graph contains a circular reference");
                }
                if (_cache[key] is UnloadableObject)
                {
                    // We don't retry loading if we fail
                    throw new DotNetRdfConfigurationException("Unable to load the Object identified by the Node '" + objNode.ToString() + "' as previous attempt(s) to load the Object failed.  Call ClearCache() before attempting loading if you wish to retry loading");
                }
                // Return from Cache
                return _cache[key];
            }
            _cache.Add(key, null);

            Object temp = null;

            // Try and find an Object Loader that can load this object
            try
            {
                foreach (IObjectFactory loader in _factories)
                {
                    if (loader.CanLoadObject(targetType))
                    {
                        if (loader.TryLoadObject(g, objNode, targetType, out temp)) break;
                    }
                }
            }
            catch (DotNetRdfConfigurationException)
            {
                _cache[key] = new UnloadableObject();
                throw;
            }
            catch (Exception ex)
            {
                _cache[key] = new UnloadableObject();
                throw new DotNetRdfConfigurationException("Unable to load the Object identified by the Node '" + objNode.ToString() + "' as an error occurred in the Object Loader which attempted to load it", ex);
            }

            // Error or return
            _cache[key] = temp ?? throw new DotNetRdfConfigurationException("Unable to load the Object identified by the Node '" + objNode.ToString() + "' as an instance of type '" + targetType + "' since no Object Loaders are able to load this type");
            return temp;
        }

        /// <summary>
        /// Loads the Object identified by the given Node based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Use this overload when you have a Node which identifies an Object and you don't know what the type of that Object is.  This function looks up the <strong>dnr:type</strong> property for the given Object and then calls the other version of this function providing it with the relevant type information.
        /// </para>
        /// </remarks>
        public static Object LoadObject(IGraph g, INode objNode)
        {
            String typeName = GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(PropertyType)));
            if (typeName == null)
            {
                typeName = GetDefaultType(g, objNode);
                if (typeName == null)
                {
                    throw new DotNetRdfConfigurationException("Unable to load the Object identified by the Node '" + objNode.ToString() + "' since there is no dnr:type property associated with it");
                }
                return LoadObject(g, objNode, Type.GetType(typeName));
            }
            return LoadObject(g, objNode, Type.GetType(typeName));
        }

        /// <summary>
        /// Attempts to find the Default Type to load an Object as when no explicit dnr:type property has been declared but an rdf:type property has been declared giving a valid Configuration Class
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> Only some configuration classes have corresponding default types, in general it is recommended that Configuration Graphs should always use the dnr:type property to explicitly state the intended type of an Object
        /// </para>
        /// </remarks>
        public static String GetDefaultType(IGraph g, INode objNode)
        {
            IUriNode rdfType = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode declaredType = GetConfigurationNode(g, objNode, rdfType);
            if (declaredType == null) return null; //Fixes Bug CORE-98
            if (declaredType.NodeType == NodeType.Uri)
            {
                String typeUri = declaredType.ToString();
                if (typeUri.StartsWith(ConfigurationNamespace))
                {
                    return GetDefaultType(typeUri);
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// Attempts to return the Default Type to load an Object as when there is no dnr:type property but there is a rdf:type property
        /// </summary>
        /// <param name="typeUri">Type URI declared by the rdf:type property</param>
        /// <returns></returns>
        public static String GetDefaultType(String typeUri)
        {
            switch (typeUri)
            {
                case ClassGraph:
                    return DefaultTypeGraph;
                case ClassGraphCollection:
                    return DefaultTypeGraphCollection;
                case ClassSparqlHttpProtocolProcessor:
                    return DefaultTypeSparqlHttpProtocolProcessor;
                case ClassSparqlQueryProcessor:
                    return DefaultTypeSparqlQueryProcessor;
                case ClassSparqlUpdateProcessor:
                    return DefaultTypeSparqlUpdateProcessor;
                case ClassTripleCollection:
                    return DefaultTypeTripleCollection;
                case ClassTripleStore:
                    return DefaultTypeTripleStore;
                case ClassUser:
                    return typeof(NetworkCredential).AssemblyQualifiedName;
                case ClassUserGroup:
                    return DefaultTypeUserGroup;
                case ClassProxy:
                    var proxyType = Type.GetType("System.Net.WebProxy, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false);
                    if (proxyType != null) return proxyType.AssemblyQualifiedName;
                    return null;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Attempts to resolve special &lt;appsettings&gt; URIs into actual values
        /// </summary>
        /// <param name="g"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// These special URIs have the form &lt;appsetting:Key&gt; where <strong>Key</strong> is the key for an appSetting in your applications configuration file.  When used these URIs are resolved at load time into the actual values from your configuration file.  This allows you to avoid spreading configuration data over multiple files since you can specify things like connection settings in the Application Config file and then simply reference them in the dotNetRDF configuration file.
        /// </para>
        /// <para>
        /// <strong>Warning: </strong> This feature is not supported in the Silverlight build 
        /// </para>
        /// </remarks>
        public static INode ResolveAppSetting(IGraph g, INode n)
        {
            if (n == null) return null;
            if (n.NodeType != NodeType.Uri) return n;

            Uri uri = ((IUriNode)n).Uri;
            if (!uri.Scheme.Equals(UriSchemeAppSettings)) return n;

            String strUri = uri.AbsoluteUri;
            String key = strUri.Substring(strUri.IndexOf(':') + 1);

            var setting = SettingsProvider.GetSetting(key);
            if (setting == null)
            {
                return null;
            }
            return g.CreateLiteralNode(setting);
        }

        #endregion

        #region Instance methods

        private readonly IGraph _configGraph;

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurationLoader" />, which
        /// loads an existing configuration graph and applies auto-configuration
        /// </summary>
        public ConfigurationLoader(IGraph configGraph)
            : this(configGraph, true)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurationLoader" />, which
        /// loads an existing configuration graph and optionally applies auto-configuration
        /// </summary>
        public ConfigurationLoader(IGraph configGraph, bool autoConfigure)
        {
            if (autoConfigure)
            {
                AutoConfigure(configGraph);
            }
            _configGraph = configGraph;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurationLoader" />, which
        /// loads an existing configuration graph and applies auto-configuration
        /// </summary>
        public ConfigurationLoader(string file)
            : this(file, true)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurationLoader" />, which
        /// loads an existing configuration graph and optionally applies auto-configuration
        /// </summary>
        public ConfigurationLoader(string file, bool autoConfigure)
        {
            _configGraph = LoadConfiguration(file, autoConfigure);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurationLoader" />, which
        /// loads an existing configuration graph from file and applies auto-configuration
        /// </summary>
        public ConfigurationLoader(Uri graphUri)
            : this(graphUri, true)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurationLoader" />, which
        /// loads an existing configuration graph and optionally applies auto-configuration
        /// </summary>
        public ConfigurationLoader(Uri graphUri, bool autoConfigure)
        {
            _configGraph = LoadConfiguration(graphUri, autoConfigure);
        }

        /// <summary>
        /// Loads the Object identified by the given blank node identifier as an object of the given type based on information from the Configuration Graph
        /// </summary>
        /// <remarks>
        /// See remarks under <see cref="LoadObject(VDS.RDF.IGraph,VDS.RDF.INode)"/> 
        /// </remarks>
        public T LoadObject<T>(string blankNodeIdentifier)
        {
            return (T)LoadObject(blankNodeIdentifier);
        }

        /// <summary>
        /// Loads the Object identified by the given URI as an object of the given type based on information from the Configuration Graph
        /// </summary>
        /// <remarks>
        /// See remarks under <see cref="LoadObject(VDS.RDF.IGraph,VDS.RDF.INode)"/> 
        /// </remarks>
        public T LoadObject<T>(Uri objectIdentifier)
        {
            return (T)LoadObject(objectIdentifier);
        }

        /// <summary>
        /// Loads the Object identified by the given blank node identifier as an <see cref="Object"/>
        /// </summary>
        /// <remarks>
        /// See remarks under <see cref="LoadObject(VDS.RDF.IGraph,VDS.RDF.INode)"/> 
        /// </remarks>
        public object LoadObject(string blankNodeIdentifier)
        {
            IBlankNode blankNode = _configGraph.GetBlankNode(blankNodeIdentifier);
            if (blankNode == null)
            {
                throw new ArgumentException(string.Format("Resource _:{0} was not found is configuration graph", blankNodeIdentifier));
            }

            return LoadObject(_configGraph, blankNode);
        }

        /// <summary>
        /// Loads the Object identified by the given URI as an <see cref="Object"/>
        /// </summary>
        /// <remarks>
        /// See remarks under <see cref="LoadObject(VDS.RDF.IGraph,VDS.RDF.INode)"/> 
        /// </remarks>
        public object LoadObject(Uri objectIdentifier)
        {
            IUriNode uriNode = _configGraph.GetUriNode(objectIdentifier);
            if (uriNode == null)
            {
                throw new ArgumentException(string.Format("Resource <{0}> was not found is configuration graph", objectIdentifier));
            }

            return LoadObject(_configGraph, uriNode);
        }

        #endregion

        /// <summary>
        /// Registers an Object Factory with the Configuration Loader
        /// </summary>
        /// <param name="factory">Object Factory</param>
        public static void AddObjectFactory(IObjectFactory factory)
        {
            Type loaderType = factory.GetType();
            if (!_factories.Any(l => l.GetType().Equals(loaderType)))
            {
                _factories.Add(factory);
            }
        }

        /// <summary>
        /// Gets/Sets the in-use Path Resolver
        /// </summary>
        public static IPathResolver PathResolver
        {
            get
            {
                return _resolver;
            }
            set
            {
                _resolver = value;
            }
        }

        /// <summary>
        /// Resolves a Path using the in-use path-resolver
        /// </summary>
        /// <param name="path">Path to resolve</param>
        /// <returns></returns>
        public static String ResolvePath(String path)
        {
            if (_resolver == null) return path;
            return _resolver.ResolvePath(path);
        }
    }

    /// <summary>
    /// Marker class used in the <see cref="ConfigurationLoader">ConfigurationLoader</see> Object cache to mark objects which are unloadable due to some errors to stop the loader repeatedly trying to load an Object whose configuration is invalid, incomplete or otherwise erroneous.
    /// </summary>
    struct UnloadableObject
    {

    }
}
