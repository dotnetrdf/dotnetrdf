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
using VDS.RDF.Configuration;
using VDS.RDF.Update.Protocol;

namespace VDS.RDF.Web.Configuration.Protocol
{
    /// <summary>
    /// Abstract Base Class for representing SPARQL Graph Store HTTP Protocol for Graph Management Handler configurations
    /// </summary>
    public abstract class BaseProtocolHandlerConfiguration
        : BaseHandlerConfiguration
    {
        /// <summary>
        /// Protocol processor
        /// </summary>
        protected ISparqlHttpProtocolProcessor _processor;
        /// <summary>
        /// Service Description Graph
        /// </summary>
        protected IGraph _serviceDescription = null;

        /// <summary>
        /// Creates a new Protocol Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public BaseProtocolHandlerConfiguration(IHttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode)
        {
            // Then get the Protocol Processor to be used
            ISparqlHttpProtocolProcessor processor;
            INode procNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyProtocolProcessor)));
            if (procNode == null) throw new DotNetRdfConfigurationException("Unable to load Protocol Handler Configuration as the RDF configuration file does not specify a dnr:protocolProcessor property for the Handler");
            Object temp = ConfigurationLoader.LoadObject(g, procNode);
            if (temp is ISparqlHttpProtocolProcessor)
            {
                processor = (ISparqlHttpProtocolProcessor)temp;
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to load Protocol Handler Configuration as the RDF configuration file specifies a value for the Handlers dnr:protocolProcessor property which cannot be loaded as an object which implements the ISparqlHttpProtocolProcessor interface");
            }
            this._processor = processor;

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
        /// Gets the SPARQL Graph Store HTTP Protocol for Graph Management processor which is to be used
        /// </summary>
        public ISparqlHttpProtocolProcessor Processor
        {
            get
            {
                return this._processor;
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

        }
    }

    /// <summary>
    /// A basic Protocol Handler Configuration implentation
    /// </summary>
    public class ProtocolHandlerConfiguration 
        : BaseProtocolHandlerConfiguration
    {
        /// <summary>
        /// Creates a new Protocol Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public ProtocolHandlerConfiguration(IHttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode) { }
    }
}
