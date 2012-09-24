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
            //First try locally scoped factories
            foreach (IPropertyFunctionFactory factory in localFactories)
            {
                if (factory.TryCreatePropertyFunction(info, out function)) return true;
            }
            //Then try global factories
            foreach (IPropertyFunctionFactory factory in _factories)
            {
                if (factory.TryCreatePropertyFunction(info, out function)) return true;
            }
            return false;
        }
    }
}
