/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    /// <summary>
    /// Defines a Connection
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface is intended to be used in conjunction with <see cref="ConnectionAttribute"/> annotations on properties of your implementation in order to indicate configurable properties that can be controlled.  Then when your <see cref="IConnectionDefinition.OpenConnection()"/> method gets called you can use the information in those properties to actually create a connection
    /// </para>
    /// </remarks>
    public interface IConnectionDefinition
        : IEnumerable<KeyValuePair<PropertyInfo, ConnectionAttribute>>
    {
        /// <summary>
        /// Gets the display name for this connection
        /// </summary>
        String StoreName
        {
            get;
        }

        /// <summary>
        /// Gets the description for this connection
        /// </summary>
        String StoreDescription
        {
            get;
        }

        /// <summary>
        /// Gets the .Net type that connections created from this definition will have
        /// </summary>
        Type Type
        {
            get;
        }

        /// <summary>
        /// Opens a new connection
        /// </summary>
        /// <returns></returns>
        IStorageProvider OpenConnection();

        /// <summary>
        /// Populates settings from an existing Configuration Serialization Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="objNode">Object Node</param>
        void PopulateFrom(IGraph g, INode objNode);

        /// <summary>
        /// Make a copy of the defintion
        /// </summary>
        /// <returns>Copy of the existing definition</returns>
        IConnectionDefinition Copy();
    }
}
