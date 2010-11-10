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
using System.Reflection;
using System.Web;
using VDS.RDF.Configuration;
using VDS.RDF.Configuration.Permissions;
using VDS.RDF.Query;
using VDS.RDF.Query.Describe;

namespace VDS.RDF.Web.Configuration.Query
{
    /// <summary>
    /// Abstract Base class for SPARQL Query Handlers
    /// </summary>
    public class BaseQueryHandlerConfiguration : BaseHandlerConfiguration
    {
        /// <summary>
        /// Query Processor to be used
        /// </summary>
        protected ISparqlQueryProcessor _processor;

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
        /// Creates a new Query Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public BaseQueryHandlerConfiguration(HttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode)
        {
            //Get the Query Processor to be used
            ISparqlQueryProcessor processor;
            INode procNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyQueryProcessor));
            if (procNode == null) throw new DotNetRdfConfigurationException("Unable to load Query Handler Configuration as the RDF configuration file does not specify a dnr:queryProcessor property for the Handler");
            Object temp = ConfigurationLoader.LoadObject(g, procNode);
            if (temp is ISparqlQueryProcessor)
            {
                processor = (ISparqlQueryProcessor)temp;
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to load Query Handler Configuration as the RDF configuration file specifies a value for the Handlers dnr:updateProcessor property which cannot be loaded as an object which implements the ISparqlQueryProcessor interface");
            }
            this._processor = processor;

            //SPARQL Query Default Config
            this._defaultGraph = ConfigurationLoader.GetConfigurationValue(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyDefaultGraphUri)).ToSafeString();
            this._defaultTimeout = ConfigurationLoader.GetConfigurationInt64(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyTimeout), this._defaultTimeout);
            this._defaultPartialResults = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyPartialResults), this._defaultPartialResults);

            //Handler Configuration
            this._showQueryForm = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyShowQueryForm), this._showQueryForm);
            String defQueryFile = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyDefaultQueryFile));
            if (defQueryFile != null)
            {
                defQueryFile = context.Server.MapPath(defQueryFile);
                if (File.Exists(defQueryFile))
                {
                    using (StreamReader reader = new StreamReader(defQueryFile))
                    {
                        this._defaultQuery = reader.ReadToEnd();
                        reader.Close();
                    }
                }
            }

            //Get the SPARQL Describe Algorithm
            INode describeNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyDescribeAlgorithm));
            if (describeNode != null)
            {
                if (describeNode.NodeType == NodeType.Literal)
                {
                    String algoClass = ((LiteralNode)describeNode).Value;
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
        }

        /// <summary>
        /// Gets the Processor used to evaluate queries
        /// </summary>
        public ISparqlQueryProcessor Processor
        {
            get
            {
                return this._processor;
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
    }

    /// <summary>
    /// Basic implementation of a Query Handler Configuration
    /// </summary>
    public class QueryHandlerConfiguration : BaseQueryHandlerConfiguration
    {
        /// <summary>
        /// Creates a new Protocol Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public QueryHandlerConfiguration(HttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode) { }
    }
}

#endif