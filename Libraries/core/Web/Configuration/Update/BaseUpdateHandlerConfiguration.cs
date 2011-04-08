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
        public BaseUpdateHandlerConfiguration(HttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode)
        {
            //Then get the Update Processor to be used
            ISparqlUpdateProcessor processor;
            INode procNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUpdateProcessor));
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

            //Handler Settings
            this._showUpdateForm = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyShowUpdateForm), this._showUpdateForm);
            String defUpdateFile = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyDefaultUpdateFile));
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
            //Add Local Extension Function definitions
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
    public class UpdateHandlerConfiguration : BaseUpdateHandlerConfiguration
    {
        /// <summary>
        /// Creates a new Update Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public UpdateHandlerConfiguration(HttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode) { }
    }
}

#endif