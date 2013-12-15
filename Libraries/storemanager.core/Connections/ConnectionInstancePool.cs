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
