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
using System.Linq;
#if NETCORE
using System.Reflection;
#endif

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
            if (_tripleCollectionType.IsAssignableFrom(targetType))
            {
                wrapsNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingTripleCollection)));
                if (wrapsNode == null)
                {
                    // Simple Triple Collection creation
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
                    // Wrapped Triple Collection creation
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
            else if (_graphCollectionType.IsAssignableFrom(targetType))
            {
                wrapsNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingGraphCollection)));
                if (wrapsNode == null)
                {
                    // Simple Graph Collection creation
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
                    // Wrapped Graph Collection creation
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
            return (_tripleCollectionType.IsAssignableFrom(t) || _graphCollectionType.IsAssignableFrom(t))
                   && t.GetConstructors().Any(c => c.IsPublic);
        }
    }
}
