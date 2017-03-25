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
    /// The Configuration Loader is responsible for the loading of Configuration information 
    /// and objects based upon information encoded in a Graph but more generally may be used 
    /// for the loading of any type of object whose configuration has been loaded in a Graph 
    /// and for which a relevant <see cref="IObjectFactory">IObjectFactory</see> is available.
    /// </summary>
    public interface IConfigurationLoader
    {
        /// <summary>
        /// Loads the Object identified by the given URI as an object of the given type based on information from the Configuration Graph
        /// </summary>
        /// <remarks>
        /// See remarks under <see cref="ConfigurationLoader.LoadObject(VDS.RDF.IGraph,VDS.RDF.INode)"/> 
        /// </remarks>
        T LoadObject<T>(Uri objectIdentifier);

        /// <summary>
        /// Loads the Object identified by the given blank node identifier as an object of the given type based on information from the Configuration Graph
        /// </summary>
        /// <remarks>
        /// See remarks under <see cref="ConfigurationLoader.LoadObject(VDS.RDF.IGraph,VDS.RDF.INode)"/> 
        /// </remarks>
        T LoadObject<T>(string blankNodeIdentifier);

        /// <summary>
        /// Loads the Object identified by the given blank node identifier as an <see cref="Object"/>
        /// </summary>
        /// <remarks>
        /// See remarks under <see cref="ConfigurationLoader.LoadObject(VDS.RDF.IGraph,VDS.RDF.INode)"/> 
        /// </remarks>
        object LoadObject(string blankNodeIdentifier);

        /// <summary>
        /// Loads the Object identified by the given URI as an <see cref="Object"/>
        /// </summary>
        /// <remarks>
        /// See remarks under <see cref="ConfigurationLoader.LoadObject(VDS.RDF.IGraph,VDS.RDF.INode)"/> 
        /// </remarks>
        object LoadObject(Uri objectIdentifier);
    }
}