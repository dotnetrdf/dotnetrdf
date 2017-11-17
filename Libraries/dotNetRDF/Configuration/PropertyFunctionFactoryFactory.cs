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
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Factory class for producing Custom SPARQL Expression Factories from Configuration Graphs
    /// </summary>
    public class PropertyFunctionFactoryFactory 
        : IObjectFactory
    {
        private Type _factoryType = typeof(IPropertyFunctionFactory);

        /// <summary>
        /// Tries to load a SPARQL Property Function Factory based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            IPropertyFunctionFactory output;
            try
            {
                output = (IPropertyFunctionFactory)Activator.CreateInstance(targetType);
            }
            catch
            {
                // Any error means this loader can't load this type
                return false;
            }

            obj = output;
            return true;
        }

        /// <summary>
        /// Gets whether this Factory can load objects of the given Type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            // We can load any object which implements IPropertyFunctionFactory and has a public unparameterized constructor
            return t.GetInterfaces().Contains(_factoryType) && t.GetConstructors().Any(c => c.GetParameters().Length == 0 && c.IsPublic);
        }
    }
}
