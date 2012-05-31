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

using System;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// An Object Factory for creating SPARQL Datasets
    /// </summary>
    public class DatasetFactory 
        : IObjectFactory
    {
        private const String InMemoryDataset = "VDS.RDF.Query.Datasets.InMemoryDataset",
                             InMemoryQuadDataset = "VDS.RDF.Query.Datasets.InMemoryQuadDataset",
                             WebDemandDataset = "VDS.RDF.Query.Datasets.WebDemandDataset";

        /// <summary>
        /// Tries to load a SPARQL Dataset based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;

            INode storeNode;
            switch (targetType.FullName)
            {
                case InMemoryDataset:
                    storeNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingStore));
                    if (storeNode == null)
                    {
                        obj = new InMemoryDataset();
                    }
                    else
                    {
                        Object temp = ConfigurationLoader.LoadObject(g, storeNode);
                        if (temp is IInMemoryQueryableStore)
                        {
                            obj = new InMemoryDataset((IInMemoryQueryableStore)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the In-Memory Dataset identified by the Node '" + objNode.ToString() + "' since the Object pointed to by the dnr:usingStore property could not be loaded as an object which implements the IInMemoryQueryableStore interface");
                        }
                    }
                    break;

                case InMemoryQuadDataset:
                    storeNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingStore));
                    if (storeNode == null)
                    {
                        obj = new InMemoryQuadDataset();
                    }
                    else
                    {
                        Object temp = ConfigurationLoader.LoadObject(g, storeNode);
                        if (temp is IInMemoryQueryableStore)
                        {
                            obj = new InMemoryQuadDataset((IInMemoryQueryableStore)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the In-Memory Dataset identified by the Node '" + objNode.ToString() + "' since the Object pointed to by the dnr:usingStore property could not be loaded as an object which implements the IInMemoryQueryableStore interface");
                        }
                    }
                    break;

#if !SILVERLIGHT

                case WebDemandDataset:
                    storeNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingDataset));
                    if (storeNode == null)
                    {
                        obj = new WebDemandDataset(new InMemoryQuadDataset());
                    }
                    else
                    {
                        Object temp = ConfigurationLoader.LoadObject(g, storeNode);
                        if (temp is ISparqlDataset)
                        {
                            obj = new WebDemandDataset((ISparqlDataset)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the Web Demand Dataset identified by the Node '" + objNode.ToString() + "' since the Object pointed to by the dnr:usingDataset property could not be loaded as an object which implements the ISparqlDataset interface");
                        }
                    }
                    break;

#endif
            } 

            return (obj != null);
        }

        /// <summary>
        /// Gets whether this Factory can load objects of the given Type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            switch (t.FullName)
            {
                case InMemoryDataset:
                case InMemoryQuadDataset:
                case WebDemandDataset:
                    return true;
                default:
                    return false;
            }
        }
    }
}
