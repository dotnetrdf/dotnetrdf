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
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Describe;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Update;
using VDS.RDF.Update.Protocol;

namespace VDS.RDF.Web.Configuration.Server
{
    /// <summary>
    /// Abstract Base class for Handler Configuration for SPARQL Servers
    /// </summary>
    public abstract class BaseSparqlServerConfiguration
        : BaseHandlerConfiguration
    {
        /// <summary>
        /// Query processor
        /// </summary>
        protected ISparqlQueryProcessor _queryProcessor;
        /// <summary>
        /// Update processor
        /// </summary>
        protected ISparqlUpdateProcessor _updateProcessor;
        /// <summary>
        /// Protocol processor
        /// </summary>
        protected ISparqlHttpProtocolProcessor _protocolProcessor;
        /// <summary>
        /// Service Description Graph
        /// </summary>
        protected IGraph _serviceDescription = null;

        #region Query Variables and Properties

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
        /// Whether a Query Form should be shown to the User
        /// </summary>
        protected bool _showQueryForm = true;
        /// <summary>
        /// Default Sparql Query
        /// </summary>
        protected String _defaultQuery = String.Empty;

        /// <summary>
        /// SPARQL Describe Algorithm to use (null indicates default is used)
        /// </summary>
        protected ISparqlDescribe _describer = null;
        /// <summary>
        /// SPARQL Syntax to use (defaults to library default which is SPARQL 1.1 unless changed)
        /// </summary>
        protected SparqlQuerySyntax _syntax = Options.QueryDefaultSyntax;

        /// <summary>
        /// Query Optimiser to use (null indicates default is used)
        /// </summary>
        protected IQueryOptimiser _queryOptimiser = null;
        /// <summary>
        /// Algebra Optimisers to use (empty list means only standard optimisers apply)
        /// </summary>
        protected List<IAlgebraOptimiser> _algebraOptimisers = new List<IAlgebraOptimiser>();

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
        /// Gets the SPARQL Describe Algorithm to be used
        /// </summary>
        public ISparqlDescribe DescribeAlgorithm
        {
            get
            {
                return this._describer;
            }
        }

        /// <summary>
        /// Gets the SPARQL Query Syntax to use
        /// </summary>
        public SparqlQuerySyntax QuerySyntax
        {
            get
            {
                return this._syntax;
            }
        }

        /// <summary>
        /// Gets the Query Optimiser associated with the Configuration
        /// </summary>
        public IQueryOptimiser QueryOptimiser
        {
            get
            {
                return this._queryOptimiser;
            }
        }

        /// <summary>
        /// Gets the Algebra Optimisers associated with the Configuration
        /// </summary>
        public IEnumerable<IAlgebraOptimiser> AlgebraOptimisers
        {
            get
            {
                return this._algebraOptimisers;
            }
        }

        #endregion

        #region Update Variables and Properties

        /// <summary>
        /// Whether Update Form should be shown
        /// </summary>
        protected bool _showUpdateForm = true;
        /// <summary>
        /// Default Update Text for the Update Form
        /// </summary>
        protected String _defaultUpdate = String.Empty;

        /// <summary>
        /// Gets whether to show the Update Form if no update is specified
        /// </summary>
        public bool ShowUpdateForm
        {
            get
            {
                return this._showUpdateForm;
            }
        }

        /// <summary>
        /// Gets the Default Update for the Update Form
        /// </summary>
        public String DefaultUpdate
        {
            get
            {
                return this._defaultUpdate;
            }
        }

        #endregion

        #region Protocol Variables and Properties

        #endregion

        /// <summary>
        /// Gets the Service Description Graph
        /// </summary>
        public IGraph ServiceDescription
        {
            get
            {
                return this._serviceDescription;
            }
        }

        /// <summary>
        /// Creates a new Base SPARQL Server Configuration based on information from a Configuration Graph
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public BaseSparqlServerConfiguration(IHttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode)
        {
            // Get the Query Processor to be used
            INode procNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyQueryProcessor)));
            if (procNode != null)
            {
                Object temp = ConfigurationLoader.LoadObject(g, procNode);
                if (temp is ISparqlQueryProcessor)
                {
                    this._queryProcessor = (ISparqlQueryProcessor)temp;
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load SPARQL Server Configuration as the RDF configuration file specifies a value for the Handlers dnr:queryProcessor property which cannot be loaded as an object which implements the ISparqlQueryProcessor interface");
                }
            }

            // SPARQL Query Default Config
            this._defaultGraph = ConfigurationLoader.GetConfigurationValue(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyDefaultGraphUri)))?.ToString() ?? string.Empty;
            this._defaultTimeout = ConfigurationLoader.GetConfigurationInt64(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyTimeout)), this._defaultTimeout);
            this._defaultPartialResults = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPartialResults)), this._defaultPartialResults);

            // Handler Configuration
            this._showQueryForm = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyShowQueryForm)), this._showQueryForm);
            String defQueryFile = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyDefaultQueryFile)));
            if (defQueryFile != null)
            {
                defQueryFile = ConfigurationLoader.ResolvePath(defQueryFile);
                if (File.Exists(defQueryFile))
                {
                    using (StreamReader reader = new StreamReader(defQueryFile))
                    {
                        this._defaultQuery = reader.ReadToEnd();
                        reader.Close();
                    }
                }
            }

            // Get Query Syntax to use
            try
            {
                String syntaxSetting = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertySyntax)));
                if (syntaxSetting != null)
                {
                    this._syntax = (SparqlQuerySyntax)Enum.Parse(typeof(SparqlQuerySyntax), syntaxSetting);
                }
            }
            catch (Exception ex)
            {
                throw new DotNetRdfConfigurationException("Unable to set the Syntax for the HTTP Handler identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:syntax property was not a valid value from the enum VDS.RDF.Parsing.SparqlQuerySyntax", ex);
            }

            // Get the SPARQL Describe Algorithm
            INode describeNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyDescribeAlgorithm)));
            if (describeNode != null)
            {
                if (describeNode.NodeType == NodeType.Literal)
                {
                    String algoClass = ((ILiteralNode)describeNode).Value;
                    try
                    {
                        Object desc = Activator.CreateInstance(Type.GetType(algoClass));
                        if (desc is ISparqlDescribe)
                        {
                            this._describer = (ISparqlDescribe)desc;
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to set the Describe Algorithm for the HTTP Handler identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:describeAlgorithm property was not a type name of a type that implements the ISparqlDescribe interface");
                        }
                    }
                    catch (DotNetRdfConfigurationException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new DotNetRdfConfigurationException("Unable to set the Describe Algorithm for the HTTP Handler identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:describeAlgorithm property was not a type name for a type that can be instantiated", ex);
                    }
                }
            }

            // Get the Query Optimiser
            INode queryOptNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyQueryOptimiser)));
            if (queryOptNode != null)
            {
                Object queryOpt = ConfigurationLoader.LoadObject(g, queryOptNode);
                if (queryOpt is IQueryOptimiser)
                {
                    this._queryOptimiser = (IQueryOptimiser)queryOpt;
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to set the Query Optimiser for the HTTP Handler identified by the Node '" + queryOptNode.ToString() + "' as the value given for the dnr:queryOptimiser property points to an Object which could not be loaded as an object which implements the required IQueryOptimiser interface");
                }
            }

            // Get the Algebra Optimisers
            foreach (INode algOptNode in ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyAlgebraOptimiser))))
            {
                Object algOpt = ConfigurationLoader.LoadObject(g, algOptNode);
                if (algOpt is IAlgebraOptimiser)
                {
                    this._algebraOptimisers.Add((IAlgebraOptimiser)algOpt);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to set the Algebra Optimiser for the HTTP Handler identified by the Node '" + algOptNode.ToString() + "' as the value given for the dnr:algebraOptimiser property points to an Object which could not be loaded as an object which implements the required IAlgebraOptimiser interface");
                }
            }

            // Then get the Update Processor to be used
            procNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUpdateProcessor)));
            if (procNode != null)
            {
                Object temp = ConfigurationLoader.LoadObject(g, procNode);
                if (temp is ISparqlUpdateProcessor)
                {
                    this._updateProcessor = (ISparqlUpdateProcessor)temp;
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load SPARQL Server Configuration as the RDF configuration file specifies a value for the Handlers dnr:updateProcessor property which cannot be loaded as an object which implements the ISparqlUpdateProcessor interface");
                }
            }

            // Handler Settings
            this._showUpdateForm = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyShowUpdateForm)), this._showUpdateForm);
            String defUpdateFile = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyDefaultUpdateFile)));
            if (defUpdateFile != null)
            {
                defUpdateFile = ConfigurationLoader.ResolvePath(defUpdateFile);
                if (File.Exists(defUpdateFile))
                {
                    using (StreamReader reader = new StreamReader(defUpdateFile))
                    {
                        this._defaultUpdate = reader.ReadToEnd();
                        reader.Close();
                    }
                }
            }

            // Then get the Protocol Processor to be used
            procNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyProtocolProcessor)));
            if (procNode != null)
            {
                Object temp = ConfigurationLoader.LoadObject(g, procNode);
                if (temp is ISparqlHttpProtocolProcessor)
                {
                    this._protocolProcessor = (ISparqlHttpProtocolProcessor)temp;
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load SPARQL Server Configuration as the RDF configuration file specifies a value for the Handlers dnr:protocolProcessor property which cannot be loaded as an object which implements the ISparqlHttpProtocolProcessor interface");
                }
            }

            if (this._queryProcessor == null && this._updateProcessor == null && this._protocolProcessor == null)
            {
                throw new DotNetRdfConfigurationException("Unable to load SPARQL Server Configuration as the RDF configuration file does not specify at least one of a Query/Update/Protocol processor for the server using the dnr:queryProcessor/dnr:updateProcessor/dnr:protocolProcessor properties");
            }

            // Get the Service Description Graph
            INode descripNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServiceDescription)));
            if (descripNode != null)
            {
                Object descrip = ConfigurationLoader.LoadObject(g, descripNode);
                if (descrip is IGraph)
                {
                    this._serviceDescription = (IGraph)descrip;
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to set the Service Description Graph for the HTTP Handler identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:serviceDescription property points to an Object which could not be loaded as an object which implements the required IGraph interface");
                }
            }
        }

        /// <summary>
        /// Creates a new Base SPARQL Server Configuration based on information from a Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public BaseSparqlServerConfiguration(IGraph g, INode objNode)
            : this(null, g, objNode) { }

        /// <summary>
        /// Gets the SPARQL Query Processor
        /// </summary>
        public ISparqlQueryProcessor QueryProcessor
        {
            get
            {
                return this._queryProcessor;
            }
        }

        /// <summary>
        /// Gets the SPARQL Update Processor
        /// </summary>
        public ISparqlUpdateProcessor UpdateProcessor
        {
            get
            {
                return this._updateProcessor;
            }
        }

        /// <summary>
        /// Gets the SPARQL Graph Store HTTP Protocol Processor
        /// </summary>
        public ISparqlHttpProtocolProcessor ProtocolProcessor
        {
            get
            {
                return this._protocolProcessor;
            }
        }

        /// <summary>
        /// Adds Description of Features for the given Handler Configuration
        /// </summary>
        /// <param name="g">Service Description Graph</param>
        /// <param name="queryNode">Node for the SPARQL Query service</param>
        /// <param name="updateNode">Node for the SPARQL Update service</param>
        /// <param name="protocolNode">Node for the SPARQL Graph Store HTTP Protocol service</param>
        public virtual void AddFeatureDescription(IGraph g, INode queryNode, INode updateNode, INode protocolNode)
        {
            IUriNode extensionFunction = g.CreateUriNode("sd:" + SparqlServiceDescriber.PropertyExtensionFunction);
            IUriNode extensionAggregate = g.CreateUriNode("sd:" + SparqlServiceDescriber.PropertyExtensionAggregate);

            if (queryNode != null)
            {
                // Add Local Extension Function definitions
                foreach (ISparqlCustomExpressionFactory factory in this._expressionFactories)
                {
                    foreach (Uri u in factory.AvailableExtensionFunctions)
                    {
                        g.Assert(queryNode, extensionFunction, g.CreateUriNode(u));
                    }
                    foreach (Uri u in factory.AvailableExtensionAggregates)
                    {
                        g.Assert(queryNode, extensionAggregate, g.CreateUriNode(u));
                    }
                }
            }

            if (updateNode != null)
            {
                // Add Local Extension Function definitions
                foreach (ISparqlCustomExpressionFactory factory in this._expressionFactories)
                {
                    foreach (Uri u in factory.AvailableExtensionFunctions)
                    {
                        g.Assert(updateNode, extensionFunction, g.CreateUriNode(u));
                    }
                    foreach (Uri u in factory.AvailableExtensionAggregates)
                    {
                        g.Assert(updateNode, extensionAggregate, g.CreateUriNode(u));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Concrete implementation of a Handler Configuration for SPARQL Servers
    /// </summary>
    public class SparqlServerConfiguration 
        : BaseSparqlServerConfiguration
    {
        /// <summary>
        /// Creates a new SPARQL Server Configuration from information in a Configuration Graph
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public SparqlServerConfiguration(IHttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode)
        {

        }

        /// <summary>
        /// Creates a new SPARQL Server Configuration from information in a Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public SparqlServerConfiguration(IGraph g, INode objNode)
            : base(null, g, objNode) { }
    }
}
