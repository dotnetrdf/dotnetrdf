using System;
using System.Collections.Generic;
using SysConfig = System.Configuration;
using System.Linq;
using System.Net;
using VDS.RDF.Parsing;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// The Configuration Loader is responsible for the loading of Configuration information and objects based upon information encoded in a Graph but more generally may be used for the loading of any type of object whose configuration has been loaded in a Graph and for which a relevant <see cref="IObjectLoader">IObjectLoader</see> is available.
    /// </summary>
    /// <remarks>
    /// <para></para>
    /// </remarks>
    public static class ConfigurationLoader
    {
        #region Constants

        /// <summary>
        /// Configuration Namespace URI
        /// </summary>
        public const String ConfigurationNamespace = "http://www.dotnetrdf.org/configuration#";

        /// <summary>
        /// QName Constants for configuration properties for use with the CreateConfigurationNode function
        /// </summary>
        public const String PropertyType = "dnr:type",
                            PropertyUser = "dnr:user",
                            PropertyPassword = "dnr:password",
                            PropertyCredentials = "dnr:credentials",
                            PropertyUseCredentialsForProxy = "dnr:useCredentialsForProxy",
                            //Manager connection properties
                            PropertyServer = "dnr:server",
                            PropertyPort = "dnr:port",
                            PropertyDatabase = "dnr:database",
                            PropertyCatalog = "dnr:catalogID",
                            PropertyStore = "dnr:storeID",
                            PropertyQueryPath = "dnr:queryPath",
                            PropertyUpdatePath = "dnr:updatePath",
                            //Manager connection options
                            PropertyReadOnly = "dnr:readOnly",
                            PropertyEnableUpdates = "dnr:enableUpdates",
                            PropertyAsync = "dnr:async",
                            PropertyLoadMode = "dnr:loadMode",
                            PropertyEncryptConnection = "dnr:encryptConnection",
                            //Properties for associating Managers with other things
                            PropertySqlManager = "dnr:sqlManager",
                            PropertyGenericManager = "dnr:genericManager",
                            //Properties for associating Processors with other things
                            PropertyQueryProcessor = "dnr:queryProcessor",
                            PropertyUpdateProcessor = "dnr:updateProcessor",
                            PropertyProtocolProcessor = "dnr:protocolProcessor",
                            PropertyUsingDataset = "dnr:usingDataset",
                            //Properties for associating Stores and Graphs with other things
                            PropertyUsingStore = "dnr:usingStore",
                            PropertyUsingGraph = "dnr:usingGraph",
                            //Properties for defining where data comes from
                            PropertyFromFile = "dnr:fromFile",
                            PropertyFromEmbedded = "dnr:fromEmbedded",
                            PropertyFromUri = "dnr:fromUri",
                            PropertyFromString = "dnr:fromString",
                            PropertyFromDatabase = "dnr:fromDatabase",
                            PropertyFromStore = "dnr:fromStore",
                            PropertyFromGraph = "dnr:fromGraph",
                            PropertyWithUri = "dnr:withUri",
                            PropertyAssignUri = "dnr:assignUri",
                            //Properties for Endpoints
                            PropertyEndpoint = "dnr:endpoint",
                            PropertyEndpointUri = "dnr:endpointUri",
                            PropertyDefaultGraphUri = "dnr:defaultGraphUri",
                            PropertyNamedGraphUri = "dnr:namedGraphUri",
                            PropertyProxy = "dnr:proxy",
                            //Properties for reasoners
                            PropertyReasoner = "dnr:reasoner",
                            PropertyOwlReasoner = "dnr:owlReasoner",
                            //Properties for permissions
                            PropertyUserGroup = "dnr:userGroup",
                            PropertyMember = "dnr:member",
                            PropertyRequiresAuthentication = "dnr:requiresAuthentication",
                            PropertyPermissionModel = "dnr:permissionModel",
                            PropertyAllow = "dnr:allow",
                            PropertyDeny = "dnr:deny",
                            PropertyAction = "dnr:action",
                            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
                            PropertyEnableCors = "dnr:enableCors",
                            PropertySyntax = "dnr:syntax",
                            PropertyTimeout = "dnr:timeout",
                            PropertyPartialResults = "dnr:partialResults",
                            PropertyShowErrors = "dnr:showErrors",
                            PropertyHaltOnError = "dnr:haltOnError",
                            PropertyShowQueryForm = "dnr:showQueryForm",
                            PropertyShowUpdateForm = "dnr:showUpdateForm",
                            PropertyDefaultQueryFile = "dnr:defaultQueryFile",
                            PropertyDefaultUpdateFile = "dnr:defaultUpdateFile",
                            PropertyIntroFile = "dnr:introText",
                            PropertyStylesheet = "dnr:stylesheet",
                            PropertyCacheDuration = "dnr:cacheDuration",
                            PropertyCacheSliding = "dnr:cacheSliding",
                            PropertyExpressionFactory = "dnr:expressionFactory",
                            PropertyDescribeAlgorithm = "dnr:describeAlgorithm",
                            PropertyServiceDescription = "dnr:serviceDescription",
                            PropertyQueryOptimiser = "dnr:queryOptimiser",
                            PropertyAlgebraOptimiser = "dnr:algebraOptimiser",
                            //Properties for writers
                            PropertyCompressionLevel = "dnr:compressionLevel",
                            PropertyPrettyPrinting = "dnr:prettyPrinting",
                            PropertyHighSpeedWriting = "dnr:highSpeedWriting",
                            PropertyDtdWriting = "dnr:dtdWriting",
                            PropertyAttributeWriting = "dnr:attributeWriting",
                            PropertyMultiThreadedWriting = "dnr:multiThreadedWriting",
                            PropertyImportNamespacesFrom = "dnr:importNamespacesFrom"
                            ;

        /// <summary>
        /// QName Constants for configuration classes
        /// </summary>
        public const String ClassObjectFactory = "dnr:ObjectFactory",
                            ClassTripleStore = "dnr:TripleStore",
                            ClassGraph = "dnr:Graph",
                            ClassSqlManager = "dnr:SqlIOManager",
                            ClassGenericManager = "dnr:GenericIOManager",
                            ClassHttpHandler = "dnr:HttpHandler",
                            ClassSparqlEndpoint = "dnr:SparqlEndpoint",
                            ClassSparqlQueryProcessor = "dnr:SparqlQueryProcessor",
                            ClassSparqlUpdateProcessor = "dnr:SparqlUpdateProcessor",
                            ClassSparqlHttpProtocolProcessor = "dnr:SparqlHttpProtocolProcessor",
                            ClassSparqlExpressionFactory = "dnr:SparqlExpressionFactory",
                            ClassSparqlDataset = "dnr:SparqlDataset",
                            ClassQueryOptimiser = "dnr:QueryOptimiser",
                            ClassAlgebraOptimiser = "dnr:AlgebraOptimiser",
                            ClassReasoner = "dnr:Reasoner",
                            ClassOwlReasoner = "dnr:OwlReasoner",
                            ClassProxy = "dnr:Proxy",
                            ClassUserGroup = "dnr:UserGroup",
                            ClassUser = "dnr:User",
                            ClassPermission = "dnr:Permission",
                            ClassRdfParser = "dnr:RdfParser",
                            ClassDatasetParser = "dnr:DatasetParser",
                            ClassSparqlResultsParser = "dnr:SparqlResultsParser",
                            ClassRdfWriter = "dnr:RdfWriter",
                            ClassDatasetWriter = "dnr:DatasetWriter",
                            ClassSparqlResultsWriter = "dnr:SparqlResultsWriter";

        /// <summary>
        /// QName Constants for Default Types for some configuration classes
        /// </summary>
        public const String DefaultTypeTripleStore = "VDS.RDF.TripleStore",
                            DefaultTypeGraph = "VDS.RDF.Graph",
                            DefaultTypeSqlManager = "VDS.RDF.Storage.MicrosoftSqlStoreManager",
                            DefaultTypeSparqlQueryProcessor = "VDS.RDF.Query.LeviathanQueryProcessor",
                            DefaultTypeSparqlUpdateProcessor = "VDS.RDF.Update.LeviathanUpdateProcessor",
                            DefaultTypeSparqlHttpProtocolProcessor = "VDS.RDF.Update.Protocol.LeviathanProtocolProcessor",
                            DefaultTypeUserGroup = "VDS.RDF.Configuration.Permissions";

        #endregion

        #region Member Variables

        private static Dictionary<String, IUriNode> _nodeMap = new Dictionary<string,IUriNode>();
        private static Dictionary<CachedObjectKey, Object> _cache = new Dictionary<CachedObjectKey, object>();
        private static NamespaceMapper _nsmap = new NamespaceMapper(true);
        private static List<IObjectFactory> _factories = new List<IObjectFactory>()
        {
            //Default Data Factories
            new GraphFactory(),
            new StoreFactory(),
            //Default SQL and Generic Manager Factories
#if !NO_DATA && !NO_STORAGE
            new SqlManagerFactory(),
#endif
#if !NO_STORAGE
            new GenericManagerFactory(),
#endif
            new DatasetFactory(),
            //Endpoint Factories
            new SparqlEndpointFactory(),
            //Processor Factories
            new QueryProcessorFactory(),
            new UpdateProcessorFactory(),
#if !NO_WEB && !NO_ASP
            new ProtocolProcessorFactory(),
#endif
            //User and Permission related Factories
            new UserGroupFactory(),
            new PermissionFactory(),
            new CredentialsFactory(),
#if !NO_PROXY
            new ProxyFactory(),
#endif
            //SPARQL Extension related Factories
            new OptimiserFactory(),
            new ReasonerFactory(),
            new ExpressionFactoryFactory(),
            //ObjectFactory Factory
            new ObjectFactoryFactory(),
            //Parser and Writer Factories
            new ParserFactory(),
            new WriterFactory()
        };
        private static IPathResolver _resolver = null;

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
        /// Given a Configuration Graph will detect Object Factories defined in the Graph and add them to the list of available factories
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        public static void AutoDetectObjectFactories(IGraph g)
        {
            IUriNode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode objLoader = CreateConfigurationNode(g, ClassObjectFactory);

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
        /// Given a Configuration Graph will detect Readers and Writers for RDF and SPARQL syntaxes and register them with <see cref="MimeTypesHelper">MimeTypesHelper</see>.  This will cause the library defaults to be overridden where appropriate.
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        public static void AutoDetectReadersAndWriters(IGraph g)
        {
            IUriNode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode desiredType = CreateConfigurationNode(g, ClassRdfParser);
            INode formatMimeType = g.CreateUriNode(new Uri("http://www.w3.org/ns/formats/media_type"));
            INode formatExtension = g.CreateUriNode(new Uri("http://www.w3.org/ns/formats/preferred_suffix"));
            Object temp;
            String[] mimeTypes, extensions;

            //Load RDF Parsers
            foreach (INode objNode in g.GetTriplesWithPredicateObject(rdfType, desiredType).Select(t => t.Subject))
            {
                temp = LoadObject(g, objNode);
                if (temp is IRdfReader)
                {
                    //Get the formats to associate this with
                    mimeTypes = ConfigurationLoader.GetConfigurationArray(g, objNode, formatMimeType);
                    if (mimeTypes.Length == 0) throw new DotNetRdfConfigurationException("Auto-detection of Readers and Writers failed as the Parser specified by the Node '" + objNode.ToString() + "' is not associated with any MIME types");
                    extensions = ConfigurationLoader.GetConfigurationArray(g, objNode, formatExtension);

                    //Register
                    MimeTypesHelper.RegisterParser((IRdfReader)temp, mimeTypes, extensions);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-detection of Readers and Writers failed as the Node '" + objNode.ToString() + "' was stated to be rdf:type of dnr:RdfParser but failed to load as an object which implements the required IRdfReader interface");
                }
            }

            //Load Dataset parsers
            desiredType = CreateConfigurationNode(g, ClassDatasetParser);
            foreach (INode objNode in g.GetTriplesWithPredicateObject(rdfType, desiredType).Select(t => t.Subject))
            {
                temp = LoadObject(g, objNode);
                if (temp is IStoreReader)
                {
                    //Get the formats to associate this with
                    mimeTypes = ConfigurationLoader.GetConfigurationArray(g, objNode, formatMimeType);
                    if (mimeTypes.Length == 0) throw new DotNetRdfConfigurationException("Auto-detection of Readers and Writers failed as the Parser specified by the Node '" + objNode.ToString() + "' is not associated with any MIME types");
                    extensions = ConfigurationLoader.GetConfigurationArray(g, objNode, formatExtension);

                    //Register
                    MimeTypesHelper.RegisterParser((IStoreReader)temp, mimeTypes, extensions);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-detection of Readers and Writers failed as the Node '" + objNode.ToString() + "' was stated to be rdf:type of dnr:DatasetParser but failed to load as an object which implements the required IStoreReader interface");
                }
            }

            //Load SPARQL Result parsers
            desiredType = CreateConfigurationNode(g, ClassSparqlResultsParser);
            foreach (INode objNode in g.GetTriplesWithPredicateObject(rdfType, desiredType).Select(t => t.Subject))
            {
                temp = LoadObject(g, objNode);
                if (temp is ISparqlResultsReader)
                {
                    //Get the formats to associate this with
                    mimeTypes = ConfigurationLoader.GetConfigurationArray(g, objNode, formatMimeType);
                    if (mimeTypes.Length == 0) throw new DotNetRdfConfigurationException("Auto-detection of Readers and Writers failed as the Parser specified by the Node '" + objNode.ToString() + "' is not associated with any MIME types");
                    extensions = ConfigurationLoader.GetConfigurationArray(g, objNode, formatExtension);

                    //Register
                    MimeTypesHelper.RegisterParser((ISparqlResultsReader)temp, mimeTypes, extensions);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-detection of Readers and Writers failed as the Node '" + objNode.ToString() + "' was stated to be rdf:type of dnr:SparqlResultsParser but failed to load as an object which implements the required ISparqlResultsReader interface");
                }
            }

            //Load RDF Writers
            desiredType = CreateConfigurationNode(g, ClassRdfWriter);
            foreach (INode objNode in g.GetTriplesWithPredicateObject(rdfType, desiredType).Select(t => t.Subject))
            {
                temp = LoadObject(g, objNode);
                if (temp is IRdfWriter)
                {
                    //Get the formats to associate this with
                    mimeTypes = ConfigurationLoader.GetConfigurationArray(g, objNode, formatMimeType);
                    if (mimeTypes.Length == 0) throw new DotNetRdfConfigurationException("Auto-detection of Readers and Writers failed as the Writer specified by the Node '" + objNode.ToString() + "' is not associated with any MIME types");
                    extensions = ConfigurationLoader.GetConfigurationArray(g, objNode, formatExtension);

                    //Register
                    MimeTypesHelper.RegisterWriter((IRdfWriter)temp, mimeTypes, extensions);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-detection of Readers and Writers failed as the Node '" + objNode.ToString() + "' was stated to be rdf:type of dnr:RdfWriter but failed to load as an object which implements the required IRdfWriter interface");
                }
            }

            //Load Dataset Writers
            desiredType = CreateConfigurationNode(g, ClassDatasetWriter);
            foreach (INode objNode in g.GetTriplesWithPredicateObject(rdfType, desiredType).Select(t => t.Subject))
            {
                temp = LoadObject(g, objNode);
                if (temp is IStoreWriter)
                {
                    //Get the formats to associate this with
                    mimeTypes = ConfigurationLoader.GetConfigurationArray(g, objNode, formatMimeType);
                    if (mimeTypes.Length == 0) throw new DotNetRdfConfigurationException("Auto-detection of Readers and Writers failed as the Writer specified by the Node '" + objNode.ToString() + "' is not associated with any MIME types");
                    extensions = ConfigurationLoader.GetConfigurationArray(g, objNode, formatExtension);

                    //Register
                    MimeTypesHelper.RegisterWriter((IStoreWriter)temp, mimeTypes, extensions);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-detection of Readers and Writers failed as the Node '" + objNode.ToString() + "' was stated to be rdf:type of dnr:DatasetWriter but failed to load as an object which implements the required IStoreWriter interface");
                }
            }

            //Load SPARQL Result Writers
            desiredType = CreateConfigurationNode(g, ClassDatasetWriter);
            foreach (INode objNode in g.GetTriplesWithPredicateObject(rdfType, desiredType).Select(t => t.Subject))
            {
                temp = LoadObject(g, objNode);
                if (temp is ISparqlResultsWriter)
                {
                    //Get the formats to associate this with
                    mimeTypes = ConfigurationLoader.GetConfigurationArray(g, objNode, formatMimeType);
                    if (mimeTypes.Length == 0) throw new DotNetRdfConfigurationException("Auto-detection of Readers and Writers failed as the Writer specified by the Node '" + objNode.ToString() + "' is not associated with any MIME types");
                    extensions = ConfigurationLoader.GetConfigurationArray(g, objNode, formatExtension);

                    //Register
                    MimeTypesHelper.RegisterWriter((ISparqlResultsWriter)temp, mimeTypes, extensions);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-detection of Readers and Writers failed as the Node '" + objNode.ToString() + "' was stated to be rdf:type of dnr:SparqlResultsWriter but failed to load as an object which implements the required ISparqlResultsWriter interface");
                }
            }
        }

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
            else
            {
                return false;
            }
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
        public static INode CreateConfigurationNode(IGraph g, String qname)
        {
            //Is the Node already cached?
            if (_nodeMap.ContainsKey(qname)) return Tools.CopyNode(_nodeMap[qname], g);

            //Does our special namespace mapper know of the dnr prefix?
            if (!_nsmap.HasNamespace("dnr")) _nsmap.AddNamespace("dnr", new Uri(ConfigurationNamespace));

            //Create the URI Node
            Uri propertyUri = new Uri(Tools.ResolveQName(qname, _nsmap, null));
            IUriNode u = g.CreateUriNode(propertyUri);
            _nodeMap.Add(qname, u);
            return u;
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
        /// If you want the String value regardless of Node type then use the <see cref="ConfigurationLoader.GetConfigurationValue">GetConfigurationValue</see> function instead
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
            else
            {
                INode temp = ResolveAppSetting(g, n);
                if (temp == null) return null;
                if (temp.NodeType == NodeType.Literal)
                {
                    return ((ILiteralNode)temp).Value;
                }
                else
                {
                    return null;
                }
            }
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

            //Resolve AppSettings
            if (n.NodeType != NodeType.Literal)
            {
                n = ResolveAppSetting(g, n);
                if (n == null) return defValue;
            }

            if (n.NodeType == NodeType.Literal)
            {
                bool temp;
                if (Boolean.TryParse(((ILiteralNode)n).Value, out temp))
                {
                    return temp;
                }
                else
                {
                    return defValue;
                }
            }
            else
            {
                return defValue;
            }
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

            //Resolve AppSettings
            if (n.NodeType != NodeType.Literal)
            {
                n = ResolveAppSetting(g, n);
                if (n == null) return defValue;
            }

            if (n.NodeType == NodeType.Literal)
            {
                long temp;
                if (Int64.TryParse(((ILiteralNode)n).Value, out temp))
                {
                    return temp;
                }
                else
                {
                    return defValue;
                }
            }
            else
            {
                return defValue;
            }
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

            //Resolve AppSettings
            if (n.NodeType != NodeType.Literal)
            {
                n = ResolveAppSetting(g, n);
                if (n == null) return defValue;
            }

            if (n.NodeType == NodeType.Literal)
            {
                int temp;
                if (Int32.TryParse(((ILiteralNode)n).Value, out temp))
                {
                    return temp;
                }
                else
                {
                    return defValue;
                }
            }
            else
            {
                return defValue;
            }
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
            INode propUser = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUser),
                  propPwd = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyPassword);

            user = ConfigurationLoader.GetConfigurationString(g, objNode, propUser);
            pwd = ConfigurationLoader.GetConfigurationString(g, objNode, propPwd);
            if ((user == null || pwd == null) && allowCredentials)
            {
                //Have they been specified as credentials instead?
                INode propCredentials = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyCredentials);
                INode credObj = ConfigurationLoader.GetConfigurationNode(g, objNode, propCredentials);
                if (credObj != null)
                {
                    NetworkCredential credentials = (NetworkCredential)ConfigurationLoader.LoadObject(g, credObj, typeof(NetworkCredential));
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
        /// Callers of this method should be careful to check that the Object returned is of a usable type to them.  The Target Type parameter does not guarantee that the return value is of that type it is only used to determine which registered instances of <see cref="IObjectLoader">IObjectLoader</see> are potentially capable of creating the desired Object
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

            //Use an Object caching mechanism to avoid instantiating the same thing multiple times since this could be VERY costly
            CachedObjectKey key = new CachedObjectKey(objNode, g);
            if (_cache.ContainsKey(key))
            {
                if (_cache[key] == null)
                {
                    //This means we've begun trying to cache the Object but haven't loaded it yet
                    //i.e. we've encountered an indirect circular reference or the caller failed to check
                    //for direct circular references with the CheckCircularReference() method
                    throw new DotNetRdfConfigurationException("Unable to load the Object identified by the Node '" + objNode.ToString() + "' as we have already started trying to load this Object which indicates that your Configuration Graph contains a circular reference");
                }
                else if (_cache[key] is UnloadableObject)
                {
                    //We don't retry loading if we fail
                    throw new DotNetRdfConfigurationException("Unable to load the Object identified by the Node '" + objNode.ToString() + "' as previous attempt(s) to load the Object failed.  Call ClearCache() before attempting loading if you wish to retry loading");
                }
                else
                {
                    //Return from Cache
                    return _cache[key];
                }
            }
            else
            {
                _cache.Add(key, null);
            }

            Object temp = null;

            //Try and find an Object Loader that can load this object
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

            //Error or return
            if (temp == null) throw new DotNetRdfConfigurationException("Unable to load the Object identified by the Node '" + objNode.ToString() + "' as an instance of type '" + targetType.ToString() + "' since no Object Loaders are able to load this type");
            _cache[key] = temp;
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
            String typeName = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, "dnr:type"));
            if (typeName == null)
            {
                typeName = GetDefaultType(g, objNode);
                if (typeName == null)
                {
                    throw new DotNetRdfConfigurationException("Unable to load the Object identified by the Node '" + objNode.ToString() + "' since there is no dnr:type property associated with it");
                }
                else
                {
                    return ConfigurationLoader.LoadObject(g, objNode, Type.GetType(typeName));
                }
            }
            else
            {
                return ConfigurationLoader.LoadObject(g, objNode, Type.GetType(typeName));
            }
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
            IUriNode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode declaredType = ConfigurationLoader.GetConfigurationNode(g, objNode, rdfType);
            if (declaredType == null) return null; //Fixes Bug CORE-98
            if (declaredType.NodeType == NodeType.Uri)
            {
                String typeUri = declaredType.ToString();
                if (typeUri.StartsWith(ConfigurationNamespace))
                {
                    typeUri = typeUri.Replace(ConfigurationNamespace, "dnr:");
                    switch (typeUri)
                    {
                        case ClassGraph:
                            return DefaultTypeGraph;
                        case ClassSparqlHttpProtocolProcessor:
                            return DefaultTypeSparqlHttpProtocolProcessor;
                        case ClassSparqlQueryProcessor:
                            return DefaultTypeSparqlQueryProcessor;
                        case ClassSparqlUpdateProcessor:
                            return DefaultTypeSparqlUpdateProcessor;
                        case ClassSqlManager:
                            return DefaultTypeSqlManager;
                        case ClassTripleStore:
                            return DefaultTypeTripleStore;
                        case ClassUser:
                            return typeof(System.Net.NetworkCredential).AssemblyQualifiedName;
                        case ClassUserGroup:
                            return DefaultTypeUserGroup;
#if !NO_PROXY
                        case ClassProxy:
                            return typeof(System.Net.WebProxy).AssemblyQualifiedName;
#endif
                        default:
                            return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
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
#if SILVERLIGHT
            return n;
#else
            if (n == null) return null;
            if (n.NodeType != NodeType.Uri) return n;

            String uri = ((IUriNode)n).Uri.ToString();
            if (!uri.StartsWith("appsetting:")) return n;

            String key = uri.Substring(uri.IndexOf(':') + 1);
            if (SysConfig.ConfigurationManager.AppSettings[key] == null)
            {
                return null;
            }
            else
            {
                return g.CreateLiteralNode(SysConfig.ConfigurationManager.AppSettings[key]);
            }
#endif
        }
    }

    /// <summary>
    /// Marker class used in the <see cref="ConfigurationLoader">ConfigurationLoader</see> Object cache to mark objects which are unloadable due to some errors to stop the loader repeatedly trying to load an Object whose configuration is invalid, incomplete or otherwise erroneous.
    /// </summary>
    struct UnloadableObject
    {

    }
}
