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
using System.IO;
using VDS.RDF.Configuration;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Update;

namespace VDS.RDF.Web.Configuration.Update
{
    /// <summary>
    /// Abstract Base class for SPARQL Update Handler configurations
    /// </summary>
    public abstract class BaseUpdateHandlerConfiguration : BaseHandlerConfiguration
    {
        /// <summary>
        /// Update Processor to be used
        /// </summary>
        protected ISparqlUpdateProcessor _processor;
        /// <summary>
        /// Whether Update Form should be shown
        /// </summary>
        protected bool _showUpdateForm = true;
         /// <summary>
        /// Default Update Text for the Update Form
        /// </summary>
        protected String _defaultUpdate = String.Empty;

        /// <summary>
        /// Service Description Graph
        /// </summary>
        protected IGraph _serviceDescription = null;

        /// <summary>
        /// Creates a new Update Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public BaseUpdateHandlerConfiguration(IHttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode)
        {
            // Then get the Update Processor to be used
            ISparqlUpdateProcessor processor;
            INode procNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUpdateProcessor)));
            if (procNode == null) throw new DotNetRdfConfigurationException("Unable to load Update Handler Configuration as the RDF configuration file does not specify a dnr:updateProcessor property for the Handler");
            Object temp = ConfigurationLoader.LoadObject(g, procNode);
            if (temp is ISparqlUpdateProcessor)
            {
                processor = (ISparqlUpdateProcessor)temp;
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to load Update Handler Configuration as the RDF configuration file specifies a value for the Handlers dnr:updateProcessor property which cannot be loaded as an object which implements the ISparqlUpdateProcessor interface");
            }
            this._processor = processor;

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
        /// Gets the SPARQL Update processor which is to be used
        /// </summary>
        public ISparqlUpdateProcessor Processor
        {
            get
            {
                return this._processor;
            }
        }

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
        /// Adds Description of Features for the given Handler Configuration
        /// </summary>
        /// <param name="g">Service Description Graph</param>
        /// <param name="descripNode">Description Node for the Service</param>
        public virtual void AddFeatureDescription(IGraph g, INode descripNode)
        {
            // Add Local Extension Function definitions
            IUriNode extensionFunction = g.CreateUriNode("sd:" + SparqlServiceDescriber.PropertyExtensionFunction);
            IUriNode extensionAggregate = g.CreateUriNode("sd:" + SparqlServiceDescriber.PropertyExtensionAggregate);
            foreach (ISparqlCustomExpressionFactory factory in this._expressionFactories)
            {
                foreach (Uri u in factory.AvailableExtensionFunctions)
                {
                    g.Assert(descripNode, extensionFunction, g.CreateUriNode(u));
                }
                foreach (Uri u in factory.AvailableExtensionAggregates)
                {
                    g.Assert(descripNode, extensionAggregate, g.CreateUriNode(u));
                }
            }
        }
    }

    /// <summary>
    /// Basic implementation of a Update Handler Configuration
    /// </summary>
    public class UpdateHandlerConfiguration
        : BaseUpdateHandlerConfiguration
    {
        /// <summary>
        /// Creates a new Update Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public UpdateHandlerConfiguration(IHttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode) { }
    }
}
