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
using System.Linq;
using System.Reflection;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Factory class for producing <see cref="IObjectFactory">IObjectFactory</see> instances from Configuration Graphs
    /// </summary>
    /// <remarks>
    /// <para>
    /// This essentially reflexive implementation allows for defining additional <see cref="IObjectFactory">IObjectFactory</see> instances that can load custom/user defined types based on user definable Configuration.  If your Configuration Graphs contain custom object factory definitions then you should call <see cref="ConfigurationLoader.AutoDetectObjectFactories">ConfigurationLoader.AutoDetectObjectFactories()</see> before attempting to load any Configuration.
    /// </para>
    /// </remarks>
    public class ObjectFactoryFactory : IObjectFactory
    {
        /// <summary>
        /// Tries to load an Object Factory based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            IObjectFactory output;
            try
            {
                output = (IObjectFactory)Activator.CreateInstance(targetType);
            }
            catch
            {
                //Any error means this loader can't load this type
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
            Type iobjloader = typeof(IObjectFactory);

            //We can load any object which implements IObjectLoader and has a public unparameterized constructor
            if (t.GetInterfaces().Any(i => i.Equals(iobjloader)))
            {
                ConstructorInfo c = t.GetConstructor(System.Type.EmptyTypes);
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
