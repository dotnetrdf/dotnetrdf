/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Collections.Specialized;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    /// <summary>
    /// Interface for connections graphs which manage a set of connections which are serialized to a graph and a file on disk
    /// </summary>
    public interface IConnectionsGraph
        : INotifyCollectionChanged
    {
        /// <summary>
        /// Gets the file on disk where this graph is saved
        /// </summary>
        String File { get; }

        /// <summary>
        /// Gets the underlying graph
        /// </summary>
        IGraph Graph { get; }

        /// <summary>
        /// Gets the managed connections
        /// </summary>
        IEnumerable<Connection> Connections { get; }

        /// <summary>
        /// Adds a connection
        /// </summary>
        /// <param name="connection">Connection</param>
        void Add(Connection connection);

        /// <summary>
        /// Removes a connection
        /// </summary>
        /// <param name="connection">Connection</param>
        void Remove(Connection connection);

        /// <summary>
        /// Clears all connections
        /// </summary>
        void Clear();
    }
}