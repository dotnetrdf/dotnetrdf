/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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

namespace VDS.RDF.Configuration
{
    public class ConfigurationVocabulary
    {
        /// <summary>
        /// Configuration Namespace URI
        /// </summary>
        public const String ConfigurationNamespace = "http://www.dotnetrdf.org/configuration#";

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String PropertyType = ConfigurationNamespace + "type"
                            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String PropertyImports = ConfigurationNamespace + "imports"
                            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String PropertyConfigure = ConfigurationNamespace + "configure"
                            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String PropertyEnabled = ConfigurationNamespace + "enabled"
                            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String PropertyUser = ConfigurationNamespace + "user"
                            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String PropertyPassword = ConfigurationNamespace + "password"
                            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String PropertyCredentials = ConfigurationNamespace + "credentials"
                            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String PropertyUseCredentialsForProxy = ConfigurationNamespace + "useCredentialsForProxy"
                            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            PropertyServer = ConfigurationNamespace + "server"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            PropertyPort = ConfigurationNamespace + "port"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String
            PropertyDatabase = ConfigurationNamespace + "database"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String
            PropertyCatalog = ConfigurationNamespace + "catalogID"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String
            PropertyStore = ConfigurationNamespace + "storeID"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String
            PropertyQueryPath = ConfigurationNamespace + "queryPath"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String
            PropertyUpdatePath = ConfigurationNamespace + "updatePath"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String
            PropertyReadOnly = ConfigurationNamespace + "readOnly"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String
            PropertyEnableUpdates = ConfigurationNamespace + "enableUpdates"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String
            PropertyAsync = ConfigurationNamespace + "async"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String
            PropertyLoadMode = ConfigurationNamespace + "loadMode"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String
            PropertyEncryptConnection = ConfigurationNamespace + "encryptConnection"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String
            PropertySkipParsing = ConfigurationNamespace + "skipParsing"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String
            PropertyStorageProvider = ConfigurationNamespace + "storageProvider"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String
            PropertyQueryProcessor = ConfigurationNamespace + "queryProcessor"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            PropertyUpdateProcessor = ConfigurationNamespace + "updateProcessor"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            PropertyProtocolProcessor = ConfigurationNamespace + "protocolProcessor"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            PropertyUsingDataset = ConfigurationNamespace + "usingDataset"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            PropertyUsingStore = ConfigurationNamespace + "usingStore"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            PropertyUsingGraph = ConfigurationNamespace + "usingGraph"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            PropertyUsingTripleCollection = ConfigurationNamespace + "usingTripleCollection"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            PropertyUsingGraphCollection = ConfigurationNamespace + "usingGraphCollection"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            PropertyFromFile = ConfigurationNamespace + "fromFile"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            PropertyFromEmbedded = ConfigurationNamespace + "fromEmbedded"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            PropertyFromUri = ConfigurationNamespace + "fromUri"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            PropertyFromString = ConfigurationNamespace + "fromString"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            PropertyFromDataset = ConfigurationNamespace + "fromDataset"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            PropertyFromStore = ConfigurationNamespace + "fromStore"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            PropertyFromGraph = ConfigurationNamespace + "fromGraph"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            PropertyWithUri = ConfigurationNamespace + "withUri"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            PropertyAssignUri = ConfigurationNamespace + "assignUri"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            PropertyEndpoint = ConfigurationNamespace + "endpoint"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            PropertyEndpointUri = ConfigurationNamespace + "endpointUri"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            PropertyQueryEndpointUri = ConfigurationNamespace + "queryEndpointUri"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            PropertyUpdateEndpointUri = ConfigurationNamespace + "updateEndpointUri"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            PropertyQueryEndpoint = ConfigurationNamespace + "queryEndpoint"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            PropertyUpdateEndpoint = ConfigurationNamespace + "updateEndpoint"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            PropertyDefaultGraphUri = ConfigurationNamespace + "defaultGraphUri"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            PropertyNamedGraphUri = ConfigurationNamespace + "namedGraphUri"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            PropertyUnionDefaultGraph = ConfigurationNamespace + "unionDefaultGraph"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            PropertyProxy = ConfigurationNamespace + "proxy"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            PropertyReasoner = ConfigurationNamespace + "reasoner"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            PropertyOwlReasoner = ConfigurationNamespace + "owlReasoner"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            PropertyUserGroup = ConfigurationNamespace + "userGroup"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            PropertyMember = ConfigurationNamespace + "member"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            PropertyRequiresAuthentication = ConfigurationNamespace + "requiresAuthentication"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            PropertyPermissionModel = ConfigurationNamespace + "permissionModel"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            PropertyAllow = ConfigurationNamespace + "allow"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            PropertyDeny = ConfigurationNamespace + "deny"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            PropertyAction = ConfigurationNamespace + "action"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyEnableCors = ConfigurationNamespace + "enableCors"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertySyntax = ConfigurationNamespace + "syntax"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyTimeout = ConfigurationNamespace + "timeout"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyPartialResults = ConfigurationNamespace + "partialResults"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyShowErrors = ConfigurationNamespace + "showErrors"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyHaltOnError = ConfigurationNamespace + "haltOnError"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyShowQueryForm = ConfigurationNamespace + "showQueryForm"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyShowUpdateForm = ConfigurationNamespace + "showUpdateForm"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyDefaultQueryFile = ConfigurationNamespace + "defaultQueryFile"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyDefaultUpdateFile = ConfigurationNamespace + "defaultUpdateFile"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyIntroFile = ConfigurationNamespace + "introText"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyStylesheet = ConfigurationNamespace + "stylesheet"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyCacheDuration = ConfigurationNamespace + "cacheDuration"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyCacheSliding = ConfigurationNamespace + "cacheSliding"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyExpressionFactory = ConfigurationNamespace + "expressionFactory"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyFunctionFactory = ConfigurationNamespace + "propertyFunctionFactory"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyDescribeAlgorithm = ConfigurationNamespace + "describeAlgorithm"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyServiceDescription = ConfigurationNamespace + "serviceDescription"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyQueryOptimiser = ConfigurationNamespace + "queryOptimiser"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            PropertyAlgebraOptimiser = ConfigurationNamespace + "algebraOptimiser"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            //Properties for writers
            PropertyCompressionLevel = ConfigurationNamespace + "compressionLevel"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            //Properties for writers
            PropertyPrettyPrinting = ConfigurationNamespace + "prettyPrinting"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            //Properties for writers
            PropertyHighSpeedWriting = ConfigurationNamespace + "highSpeedWriting"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            //Properties for writers
            PropertyDtdWriting = ConfigurationNamespace + "dtdWriting"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            //Properties for writers
            PropertyAttributeWriting = ConfigurationNamespace + "attributeWriting"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            //Properties for writers
            PropertyMultiThreadedWriting = ConfigurationNamespace + "multiThreadedWriting"
            ;

        /// <summary>
        /// URI Constants for configuration properties
        /// </summary>
        public const String //Manager connection properties
            //Manager connection options
            //Properties for associating Managers with other things
            //Properties for associating Processors with other things
            //Properties for associating Stores and Graphs with other things
            //Properties for setting low level storage for Triple Stores and Graphs
            //Properties for defining where data comes from
            //Properties for Endpoints
            //Properties for reasoners
            //Properties for permissions
            //Properties for HTTP Handler configuration primarily around SPARQL endpoint configuration
            //Properties for writers
            PropertyImportNamespacesFrom = ConfigurationNamespace + "importNamespacesFrom"
            ;

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String ClassObjectFactory = ConfigurationNamespace + "ObjectFactory";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            ClassTripleStore = ConfigurationNamespace + "TripleStore";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            ClassGraphCollection = ConfigurationNamespace + "GraphCollection";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            ClassGraph = ConfigurationNamespace + "Graph";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            ClassTripleCollection = ConfigurationNamespace + "TripleCollection";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            ClassStorageServer = ConfigurationNamespace + "StorageServer";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            ClassStorageProvider = ConfigurationNamespace + "StorageProvider";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            ClassHttpHandler = ConfigurationNamespace + "HttpHandler";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            ClassSparqlEndpoint = ConfigurationNamespace + "SparqlEndpoint";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            ClassSparqlQueryEndpoint = ConfigurationNamespace + "SparqlQueryEndpoint";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            ClassSparqlUpdateEndpoint = ConfigurationNamespace + "SparqlUpdateEndpoint";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            ClassSparqlQueryProcessor = ConfigurationNamespace + "SparqlQueryProcessor";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            ClassSparqlUpdateProcessor = ConfigurationNamespace + "SparqlUpdateProcessor";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            ClassSparqlHttpProtocolProcessor = ConfigurationNamespace + "SparqlHttpProtocolProcessor";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            ClassSparqlExpressionFactory = ConfigurationNamespace + "SparqlExpressionFactory";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            ClassSparqlPropertyFunctionFactory = ConfigurationNamespace + "SparqlPropertyFunctionFactory";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            ClassSparqlDataset = ConfigurationNamespace + "SparqlDataset";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            ClassQueryOptimiser = ConfigurationNamespace + "QueryOptimiser";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            ClassAlgebraOptimiser = ConfigurationNamespace + "AlgebraOptimiser";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            ClassSparqlOperator = ConfigurationNamespace + "SparqlOperator";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            //Classes for reasoners
            ClassReasoner = ConfigurationNamespace + "Reasoner";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            //Classes for reasoners
            ClassOwlReasoner = ConfigurationNamespace + "OwlReasoner";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            //Classes for reasoners
            ClassProxy = ConfigurationNamespace + "Proxy";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            //Classes for reasoners
            //Classes for Users and permissions
            ClassUserGroup = ConfigurationNamespace + "UserGroup";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            //Classes for reasoners
            //Classes for Users and permissions
            ClassUser = ConfigurationNamespace + "User";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            //Classes for reasoners
            //Classes for Users and permissions
            ClassPermission = ConfigurationNamespace + "Permission";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            //Classes for reasoners
            //Classes for Users and permissions
            //Classes for Parsers and Serializers
            ClassRdfParser = ConfigurationNamespace + "RdfParser";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            //Classes for reasoners
            //Classes for Users and permissions
            //Classes for Parsers and Serializers
            ClassDatasetParser = ConfigurationNamespace + "DatasetParser";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            //Classes for reasoners
            //Classes for Users and permissions
            //Classes for Parsers and Serializers
            ClassSparqlResultsParser = ConfigurationNamespace + "SparqlResultsParser";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            //Classes for reasoners
            //Classes for Users and permissions
            //Classes for Parsers and Serializers
            ClassRdfWriter = ConfigurationNamespace + "RdfWriter";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            //Classes for reasoners
            //Classes for Users and permissions
            //Classes for Parsers and Serializers
            ClassDatasetWriter = ConfigurationNamespace + "DatasetWriter";

        /// <summary>
        /// URI Constants for configuration classes
        /// </summary>
        public const String //Classes for Triple Stores and Graphs and their associated low level storage
            //Classes for Storage Providers and Servers
            //Classes for ASP.Net integration
            //Classes for SPARQL features
            //Classes for reasoners
            //Classes for Users and permissions
            //Classes for Parsers and Serializers
            ClassSparqlResultsWriter = ConfigurationNamespace + "SparqlResultsWriter";

        public ConfigurationVocabulary()
        {
        }
    }
}