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
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Web.Configuration.Resource
{
    /// <summary>
    /// Abstract Base Class for Dataset Handler configurations
    /// </summary>
    public class BaseDatasetHandlerConfiguration 
        : BaseHandlerConfiguration
    {
        private ISparqlDataset _dataset;

        /// <summary>
        /// Creates a new Dataset Handler configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="config">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public BaseDatasetHandlerConfiguration(IHttpContext context, IGraph config, INode objNode)
            : base(context, config, objNode)
        {
            INode datasetNode = ConfigurationLoader.GetConfigurationNode(config, objNode, config.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingDataset)));
            if (datasetNode == null) throw new DotNetRdfConfigurationException("Unable to load Dataset Handler Configuration as there is no value given for the required dnr:usingDataset property");

            // Load the Dataset
            Object temp = ConfigurationLoader.LoadObject(config, datasetNode);
            if (temp is ISparqlDataset)
            {
                this._dataset = (ISparqlDataset)temp;
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to load Dataset Handler Configuration as the dnr:usingDatset property points to an Object which cannot be loaded as an object which implements the ISparqlDataset interface");
            }
        }

        /// <summary>
        /// Gets the Dataset
        /// </summary>
        public ISparqlDataset Dataset
        {
            get
            {
                return this._dataset;
            }
        }
    }

    /// <summary>
    /// Basic implementation of a Dataset Handler Configuration
    /// </summary>
    public class DatasetHandlerConfiguration
        : BaseDatasetHandlerConfiguration
    {
        /// <summary>
        /// Creates a new Dataset Handler configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="config">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public DatasetHandlerConfiguration(IHttpContext context, IGraph config, INode objNode)
            : base(context, config, objNode) { }
    }
}
