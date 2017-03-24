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
