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
using System.Web;
using VDS.RDF.Configuration;
using VDS.RDF.Configuration.Permissions;
using VDS.RDF.Update.Protocol;

namespace VDS.RDF.Web.Configuration.Protocol
{
    /// <summary>
    /// Abstract Base Class for representing SPARQL Graph Store HTTP Protocol for Graph Management Handler configurations
    /// </summary>
    public abstract class BaseProtocolHandlerConfiguration : BaseHandlerConfiguration
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
        public BaseProtocolHandlerConfiguration(HttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode)
        {
            //Then get the Protocol Processor to be used
            ISparqlHttpProtocolProcessor processor;
            INode procNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyProtocolProcessor));
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

            //Get the Service Description Graph
            INode descripNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyServiceDescription));
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
    public class ProtocolHandlerConfiguration : BaseProtocolHandlerConfiguration
    {
        /// <summary>
        /// Creates a new Protocol Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public ProtocolHandlerConfiguration(HttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode) { }
    }
}

#endif