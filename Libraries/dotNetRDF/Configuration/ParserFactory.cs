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
using System.Reflection;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Object Factory used by the Configuration API to load parsers from configuration graphs
    /// </summary>
    public class ParserFactory 
        : IObjectFactory
    {
        private readonly Type[] _parserTypes = {
            typeof(IRdfReader),
            typeof(IStoreReader),
            typeof(ISparqlResultsReader),
        };

        /// <summary>
        /// Tries to load a Parser based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            try
            {
                obj = Activator.CreateInstance(targetType);
                return true;
            }
            catch
            {
                // Any error means this loader can't load this type
                return false;
            }
        }

        /// <summary>
        /// Gets whether this Factory can load objects of the given Type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            // We can load any object which implements any parser interface and has a public unparameterized constructor
            if (t.GetInterfaces().Any(i => _parserTypes.Contains(i)))
            {
                ConstructorInfo c = t.GetConstructor(new Type[0]);
                if (c != null)
                {
                    return c.IsPublic;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Object Factory used by the Configuration API to load writers from configuration graphs
    /// </summary>
    public class WriterFactory : IObjectFactory
    {
        private readonly Type[] _writerTypes = {
            typeof(IRdfWriter),
            typeof(IStoreWriter),
            typeof(ISparqlResultsWriter),
        };

        /// <summary>
        /// Tries to load a Writer based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            try
            {
                obj = Activator.CreateInstance(targetType);
                return true;
            }
            catch
            {
                // Any error means this loader can't load this type
                return false;
            }
        }

        /// <summary>
        /// Gets whether this Factory can load objects of the given Type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            // We can load any object which implements any writer interface and has a public unparameterized constructor
            if (t.GetInterfaces().Any(i => _writerTypes.Contains(i)))
            {
                ConstructorInfo c = t.GetConstructor(new Type[0]);
                if (c != null)
                {
                    return c.IsPublic;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }
}
