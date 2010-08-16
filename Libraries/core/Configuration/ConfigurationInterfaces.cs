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

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Interface for Object Factories which are factory classes that can create Objects based on configuration information in a Graph
    /// </summary>
    public interface IObjectFactory
    {
        /// <summary>
        /// Attempts to load an Object of the given type identified by the given Node and returned as the Type that this loader generates
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Created Object</param>
        /// <returns>True if the loader succeeded in creating an Object</returns>
        /// <remarks>
        /// <para>
        /// The Factory should not throw an error if some required configuration is missing as another factory further down the processing chain may still be able to create the object.  If the factory encounters errors and all the required configuration information is present then that error should be thrown i.e. class instantiation throws an error or a call to load an object that this object requires fails.
        /// </para>
        /// </remarks>
        bool TryLoadObject(IGraph g, INode objNode, Type targetType, out Object obj);

        /// <summary>
        /// Returns whether this Factory is capable of creating objects of the given type
        /// </summary>
        /// <param name="t">Target Type</param>
        /// <returns></returns>
        bool CanLoadObject(Type t);
    }

    /// <summary>
    /// Interface for Objects which can have their configuration serialized to RDF
    /// </summary>
    public interface IConfigurationSerializable
    {
        /// <summary>
        /// Serializes the Configuration in the given context
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        void SerializeConfiguration(ConfigurationSerializationContext context);
    }

    /// <summary>
    /// Inteface for Objects which can resolve paths specified for Configuration properties
    /// </summary>
    public interface IPathResolver
    {
        /// <summary>
        /// Resolves a Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns></returns>
        String ResolvePath(String path);
    }
}
