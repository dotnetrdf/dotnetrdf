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
    /// Object Factory used by the Configuration API to load parsers from configuration graphs
    /// </summary>
    public class ParserFactory 
        : IObjectFactory
    {
        private Type[] _parserTypes = new Type[]
        {
            typeof(IRdfReader),
            typeof(IStoreReader),
            typeof(ISparqlResultsReader)
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
                //Any error means this loader can't load this type
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
            //We can load any object which implements any parser interface and has a public unparameterized constructor
            if (t.GetInterfaces().Any(i => this._parserTypes.Contains(i)))
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

    /// <summary>
    /// Object Factory used by the Configuration API to load writers from configuration graphs
    /// </summary>
    public class WriterFactory : IObjectFactory
    {
        private Type[] _writerTypes = new Type[]
        {
            typeof(IRdfWriter),
            typeof(IStoreWriter),
            typeof(ISparqlResultsWriter)
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
                //Any error means this loader can't load this type
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
            //We can load any object which implements any writer interface and has a public unparameterized constructor
            if (t.GetInterfaces().Any(i => this._writerTypes.Contains(i)))
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
