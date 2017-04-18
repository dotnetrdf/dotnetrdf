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
        public BaseGraphHandlerConfiguration(IHttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode)
        {
            INode graphNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingGraph)));
            if (graphNode == null) throw new DotNetRdfConfigurationException("Unable to load Graph Handler Configuration as the required dnr:usingGraph property does not exist");

            // Load the Graph
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
    public class GraphHandlerConfiguration
        : BaseGraphHandlerConfiguration
    {
        /// <summary>
        /// Creates a new Graph Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public GraphHandlerConfiguration(IHttpContext context, IGraph g, INode objNode)
            : base(context, g, objNode) { }
    }
}
