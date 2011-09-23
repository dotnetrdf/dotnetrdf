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
using System.Web;
using VDS.RDF.Configuration;

namespace VDS.RDF.Web.Configuration.Resource
{
    /// <summary>
    /// Abstract Base class for Graph Handler configurations
    /// </summary>
    public abstract class BaseGraphHandlerConfiguration : BaseHandlerConfiguration
    {
        private IGraph _g;
        private String _etag;

        /// <summary>
        /// Creates a new Graph Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public BaseGraphHandlerConfiguration(HttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode)
        {
            INode graphNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingGraph));
            if (graphNode == null) throw new DotNetRdfConfigurationException("Unable to load Graph Handler Configuration as the required dnr:usingGraph property does not exist");

            //Load the Graph
            Object temp = ConfigurationLoader.LoadObject(g, graphNode);
            if (temp is IGraph)
            {
                this._g = (IGraph)temp;
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to load Graph Handler Configuration as the dnr:usingGraph property points to an Object which cannot be loaded as an object which implements the IGraph interface");
            }
        }

        /// <summary>
        /// Gets the Graph being served
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return this._g;
            }
        }

        /// <summary>
        /// Gets/Sets the cached ETag for the Graph
        /// </summary>
        public String ETag
        {
            get
            {
                return this._etag;
            }
            set
            {
                this._etag = value;
            }
        }
    }

    /// <summary>
    /// Basic implementation of a Graph Handler configuration
    /// </summary>
    public class GraphHandlerConfiguration : BaseGraphHandlerConfiguration
    {
        /// <summary>
        /// Creates a new Graph Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public GraphHandlerConfiguration(HttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode) { }
    }
}

#endif