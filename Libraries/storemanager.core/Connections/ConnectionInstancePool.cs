/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using System.Text;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    /// <summary>
    /// A pool for <see cref="Connection"/> instances
    /// </summary>
    /// <remarks>
    /// The pool is used so that when connections which are serialized into multiple connection graphs are deserialized the same instance is used across all connection graphs so they all reflect the same state of the connection
    /// </remarks>
    public static class ConnectionInstancePool
    {
        private static readonly Dictionary<Uri, Connection> _instances = new Dictionary<Uri, Connection>(new UriComparer());

        /// <summary>
        /// Tries to get a connection instance from the pool
        /// </summary>
        /// <param name="rootUri">Root URI</param>
        /// <param name="connection">Connection</param>
        /// <returns>True if the connection was already in the pool</returns>
        public static bool TryGetInstance(Uri rootUri, out Connection connection)
        {
            lock (_instances)
            {
                return _instances.TryGetValue(rootUri, out connection);
            }
        }

        /// <summary>
        /// Adds a connection instance to the pool
        /// </summary>
        /// <param name="connection">Connection</param>
        public static void Add(Connection connection)
        {
            lock (_instances)
            {
                _instances[connection.RootUri] = connection;
            }
        }

        /// <summary>
        /// Gets whether a given connection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static bool Contains(Connection connection)
        {
            lock (_instances)
            {
                return _instances.ContainsKey(connection.RootUri) && ReferenceEquals(_instances[connection.RootUri], connection);
            }
        }
    }
}
