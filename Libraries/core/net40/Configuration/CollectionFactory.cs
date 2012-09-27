/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Object Factory for loading triple and graph collections
    /// </summary>
    public class CollectionFactory
        : IObjectFactory
    {
        private Type _tripleCollectionType = typeof(BaseTripleCollection),
                     _graphCollectionType = typeof(BaseGraphCollection);

        /// <summary>
        /// Tries to load a triple/graph collection which is specified in the given configuration graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target type</param>
        /// <param name="obj">Returned Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            INode wrapsNode;
            Object wrappedCollection;
            if (this._tripleCollectionType.IsAssignableFrom(targetType))
            {
                wrapsNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingTripleCollection)));
                if (wrapsNode == null)
                {
                    //Simple Triple Collection creation
                    try
                    {
                        obj = (BaseTripleCollection)Activator.CreateInstance(targetType);
                    }
                    catch
                    {
                        return false;
                    }
                }
                else 
                {
                    //Wrapped Triple Collection creation
                    wrappedCollection = ConfigurationLoader.LoadObject(g, wrapsNode) as BaseTripleCollection;
                    if (wrappedCollection == null) throw new DotNetRdfConfigurationException("Unable to load the Triple Collection identified by the Node '" + objNode.ToString() + "' as the dnr:usingTripleCollection points to an object which cannot be loaded as an instance of the required type BaseTripleCollection for this collection to wrap");
                    try
                    {
                        obj = (BaseTripleCollection)Activator.CreateInstance(targetType, new Object[] { wrappedCollection });
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            else if (this._graphCollectionType.IsAssignableFrom(targetType))
            {
                wrapsNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingGraphCollection)));
                if (wrapsNode == null)
                {
                    //Simple Graph Collection creation
                    try
                    {
                        obj = (BaseGraphCollection)Activator.CreateInstance(targetType);
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                {
                    //Wrapped Graph Collection creation
                    wrappedCollection = ConfigurationLoader.LoadObject(g, wrapsNode) as BaseGraphCollection;
                    if (wrappedCollection == null) throw new DotNetRdfConfigurationException("Unable to load the Graph Collection identified by the Node '" + objNode.ToString() + "' as the dnr:usingGraphCollection points to an object which cannot be loaded as an instance of the required type BaseGraphCollection for this collection to wrap");
                    try
                    {
                        obj = (BaseGraphCollection)Activator.CreateInstance(targetType, new Object[] { wrappedCollection });
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return (obj != null);
        }

        /// <summary>
        /// Gets whether this factory can load objects of the given type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            return (this._tripleCollectionType.IsAssignableFrom(t) || this._graphCollectionType.IsAssignableFrom(t))
                   && t.GetConstructors().Any(c => c.IsPublic);
        }
    }
}
