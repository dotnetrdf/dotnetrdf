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
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.PropertyFunctions
{
    /// <summary>
    /// Factory for creating property functions
    /// </summary>
    public static class PropertyFunctionFactory
    {
        private static List<IPropertyFunctionFactory> _factories = new List<IPropertyFunctionFactory>();

        /// <summary>
        /// Gets the number of globally registered factories
        /// </summary>
        public static int FactoryCount
        {
            get
            {
                return _factories.Count;
            }
        }

        /// <summary>
        /// Adds a globally registered factory
        /// </summary>
        /// <param name="factory">Factory</param>
        public static void AddFactory(IPropertyFunctionFactory factory)
        {
            lock (_factories)
            {
                _factories.Add(factory);
            }
        }

        /// <summary>
        /// Removes a globally registered factory
        /// </summary>
        /// <param name="factory">Factory</param>
        public static void RemoveFactory(IPropertyFunctionFactory factory)
        {
            lock (_factories)
            {
                _factories.Remove(factory);
            }
        }

        /// <summary>
        /// Gets whether a factory is registered
        /// </summary>
        /// <param name="factoryType">Factory Type</param>
        /// <returns></returns>
        public static bool IsRegisteredFactory(Type factoryType)
        {
            lock (_factories)
            {
                return _factories.Any(f => f.GetType().Equals(factoryType));
            }
        }

        /// <summary>
        /// Gets whether a factory is registered
        /// </summary>
        /// <param name="factory">Factory</param>
        /// <returns></returns>
        public static bool IsRegisteredFactory(IPropertyFunctionFactory factory)
        {
            lock (_factories)
            {
                return _factories.Contains(factory);
            }
        }

        /// <summary>
        /// Gets whether a URI is considered a property function by the global factories
        /// </summary>
        /// <param name="u">Function URI</param>
        /// <returns></returns>
        public static bool IsPropertyFunction(Uri u)
        {
            return IsPropertyFunction(u, Enumerable.Empty<IPropertyFunctionFactory>());
        }

        /// <summary>
        /// Gets whether a URI is considered a property function by any global/local factory
        /// </summary>
        /// <param name="u">Function URI</param>
        /// <param name="localFactories">Locally scoped factories</param>
        /// <returns></returns>
        public static bool IsPropertyFunction(Uri u, IEnumerable<IPropertyFunctionFactory> localFactories)
        {
            return localFactories.Any(f => f.IsPropertyFunction(u)) || _factories.Any(f => f.IsPropertyFunction(u));
        }

        /// <summary>
        /// Tries to create a property function
        /// </summary>
        /// <param name="info">Property Function information</param>
        /// <param name="function">Property Function</param>
        /// <returns></returns>
        public static bool TryCreatePropertyFunction(PropertyFunctionInfo info, out IPropertyFunctionPattern function)
        {
            return TryCreatePropertyFunction(info, Enumerable.Empty<IPropertyFunctionFactory>(), out function);
        }

        /// <summary>
        /// Tries to create a property function
        /// </summary>
        /// <param name="info">Property Function information</param>
        /// <param name="localFactories">Locally Scoped factories</param>
        /// <param name="function">Property Function</param>
        /// <returns></returns>
        public static bool TryCreatePropertyFunction(PropertyFunctionInfo info, IEnumerable<IPropertyFunctionFactory> localFactories, out IPropertyFunctionPattern function)
        {
            function = null;
            // First try locally scoped factories
            foreach (IPropertyFunctionFactory factory in localFactories)
            {
                if (factory.TryCreatePropertyFunction(info, out function)) return true;
            }
            // Then try global factories
            foreach (IPropertyFunctionFactory factory in _factories)
            {
                if (factory.TryCreatePropertyFunction(info, out function)) return true;
            }
            return false;
        }
    }
}
