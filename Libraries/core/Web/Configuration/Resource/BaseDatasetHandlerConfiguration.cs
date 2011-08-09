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
using System.Linq;
using System.Text;
using System.Web;
using VDS.RDF.Configuration;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Web.Configuration.Resource
{
    /// <summary>
    /// Abstract Base Class for Dataset Handler configurations
    /// </summary>
    public class BaseDatasetHandlerConfiguration : BaseHandlerConfiguration
    {
        private ISparqlDataset _dataset;

        /// <summary>
        /// Creates a new Dataset Handler configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="config">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public BaseDatasetHandlerConfiguration(HttpContext context, IGraph config, INode objNode)
            : base(context, config, objNode)
        {
            INode datasetNode = ConfigurationLoader.GetConfigurationNode(config, objNode, ConfigurationLoader.CreateConfigurationNode(config, ConfigurationLoader.PropertyUsingDataset));
            if (datasetNode == null) throw new DotNetRdfConfigurationException("Unable to load Dataset Handler Configuration as there is no value given for the required dnr:usingDataset property");

            //Load the Dataset
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
    public class DatasetHandlerConfiguration : BaseDatasetHandlerConfiguration
    {
        /// <summary>
        /// Creates a new Dataset Handler configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="config">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public DatasetHandlerConfiguration(HttpContext context, IGraph config, INode objNode)
            : base(context, config, objNode) { }
    }
}

#endif
