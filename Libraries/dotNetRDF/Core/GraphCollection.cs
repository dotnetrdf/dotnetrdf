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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VDS.Common.Collections;

namespace VDS.RDF
{
    /// <summary>
    /// Wrapper class for Graph Collections
    /// </summary>
    public class GraphCollection 
        : BaseGraphCollection, IEnumerable<IGraph>
    {
        /// <summary>
        /// Internal Constant used as the Hash Code for the default graph
        /// </summary>
        protected const int DefaultGraphID = 0;

        /// <summary>
        /// Dictionary of Graph Uri Enhanced Hash Codes to Graphs
        /// </summary>
        /// <remarks>See <see cref="Extensions.GetEnhancedHashCode">GetEnhancedHashCode()</see></remarks>
        protected MultiDictionary<Uri, IGraph> _graphs;

        /// <summary>
        /// Creates a new Graph Collection
        /// </summary>
        public GraphCollection()
        {
            _graphs = new MultiDictionary<Uri, IGraph>(u => (u != null ? u.GetEnhancedHashCode() : DefaultGraphID), true, new UriComparer(), MultiDictionaryMode.AVL);
        }

        /// <summary>
        /// Checks whether the Graph with the given Uri exists in this Graph Collection
        /// </summary>
        /// <param name="graphUri">Graph Uri to test</param>
        /// <returns></returns>
        public override bool Contains(Uri graphUri)
        {
            return _graphs.ContainsKey(graphUri);
        }

        /// <summary>
        /// Adds a Graph to the Collection
        /// </summary>
        /// <param name="g">Graph to add</param>
        /// <param name="mergeIfExists">Sets whether the Graph should be merged with an existing Graph of the same Uri if present</param>
        /// <exception cref="RdfException">Throws an RDF Exception if the Graph has no Base Uri or if the Graph already exists in the Collection and the <paramref name="mergeIfExists"/> parameter was not set to true</exception>
        protected internal override bool Add(IGraph g, bool mergeIfExists)
        {
            if (_graphs.ContainsKey(g.BaseUri))
            {
                // Already exists in the Graph Collection
                if (mergeIfExists)
                {
                    // Merge into the existing Graph
                    _graphs[g.BaseUri].Merge(g);
                    return true;
                }
                else
                {
                    // Not allowed
                    throw new RdfException("The Graph you tried to add already exists in the Graph Collection and the mergeIfExists parameter was set to false");
                }
            }
            else
            {
                // Safe to add a new Graph
                _graphs.Add(g.BaseUri, g);
                RaiseGraphAdded(g);
                return true;
            }
        }

        /// <summary>
        /// Removes a Graph from the Collection
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to remove</param>
        protected internal override bool Remove(Uri graphUri)
        {
            IGraph g;
            if (_graphs.TryGetValue(graphUri, out g))
            {
                if (_graphs.Remove(graphUri))
                {
                    RaiseGraphRemoved(g);
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Gets the number of Graphs in the Collection
        /// </summary>
        public override int Count
        {
            get
            {
                return _graphs.Count;
            }
        }

        /// <summary>
        /// Provides access to the Graph URIs of Graphs in the Collection
        /// </summary>
        public override IEnumerable<Uri> GraphUris
        {
            get
            {
                return _graphs.Keys;
            }
        }

        /// <summary>
        /// Gets a Graph from the Collection
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <returns></returns>
        public override IGraph this[Uri graphUri]
        {
            get 
            {
                IGraph g;
                if (_graphs.TryGetValue(graphUri, out g))
                {
                    return g;
                }
                else
                {
                    throw new RdfException("The Graph with the given URI does not exist in this Graph Collection");
                }
            }
        }

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<IGraph> GetEnumerator()
        {
            return _graphs.Values.GetEnumerator();
        }

        /// <summary>
        /// Gets the Enumerator for this Collection
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Disposes of the Graph Collection
        /// </summary>
        /// <remarks>Invokes the <strong>Dispose()</strong> method of all Graphs contained in the Collection</remarks>
        public override void Dispose()
        {
            _graphs.Clear();
        }
    }

    /// <summary>
    /// Thread Safe decorator around a Graph collection
    /// </summary>
    /// <remarks>
    /// Dependings on your platform this either provides MRSW concurrency via a <see cref="ReaderWriterLockSlim" /> or exclusive access concurrency via a <see cref="Monitor"/>
    /// </remarks>
    public class ThreadSafeGraphCollection 
        : WrapperGraphCollection
    {
        private ReaderWriterLockSlim _lockManager = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Creates a new Thread Safe decorator around the default <see cref="GraphCollection"/>
        /// </summary>
        public ThreadSafeGraphCollection()
            : base(new GraphCollection()) { }

        /// <summary>
        /// Creates a new Thread Safe decorator around the supplied graph collection
        /// </summary>
        /// <param name="graphCollection">Graph Collection</param>
        public ThreadSafeGraphCollection(BaseGraphCollection graphCollection)
            : base(graphCollection) { }

        /// <summary>
        /// Enters the write lock
        /// </summary>
        protected void EnterWriteLock()
        {
            _lockManager.EnterWriteLock();
        }

        /// <summary>
        /// Exits the write lock
        /// </summary>
        protected void ExitWriteLock()
        {
            _lockManager.ExitWriteLock();
        }

        /// <summary>
        /// Enters the read lock
        /// </summary>
        protected void EnterReadLock()
        {
            _lockManager.EnterReadLock();
        }

        /// <summary>
        /// Exits the read lock
        /// </summary>
        protected void ExitReadLock()
        {
            _lockManager.ExitReadLock();
        }

        /// <summary>
        /// Checks whether the Graph with the given Uri exists in this Graph Collection
        /// </summary>
        /// <param name="graphUri">Graph Uri to test</param>
        /// <returns></returns>
        public override bool Contains(Uri graphUri)
        {
            bool contains = false;

            try
            {
                EnterReadLock();
                contains = _graphs.Contains(graphUri);
            }
            finally
            {
                ExitReadLock();
            }
            return contains;
        }

        /// <summary>
        /// Adds a Graph to the Collection
        /// </summary>
        /// <param name="g">Graph to add</param>
        /// <param name="mergeIfExists">Sets whether the Graph should be merged with an existing Graph of the same Uri if present</param>
        /// <exception cref="RdfException">Throws an RDF Exception if the Graph has no Base Uri or if the Graph already exists in the Collection and the <paramref name="mergeIfExists"/> parameter was not set to true</exception>
        protected internal override bool Add(IGraph g, bool mergeIfExists)
        {
            try
            {
                EnterWriteLock();
                return _graphs.Add(g, mergeIfExists);
            }
            finally
            {
                ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes a Graph from the Collection
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to remove</param>
        protected internal override bool Remove(Uri graphUri)
        {
            try
            {
                EnterWriteLock();
                return _graphs.Remove(graphUri);
            }
            finally
            {
                ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the number of Graphs in the Collection
        /// </summary>
        public override int Count
        {
            get
            {
                int c = 0;
                try
                {
                    EnterReadLock();
                    c = _graphs.Count;
                }
                finally
                {
                    ExitReadLock();
                }
                return c;
            }
        }

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<IGraph> GetEnumerator()
        {
            List<IGraph> graphs = new List<IGraph>();
            try
            {
                EnterReadLock();
                graphs = _graphs.ToList();
            }
            finally
            {
                ExitReadLock();
            }
            return graphs.GetEnumerator();
        }

        /// <summary>
        /// Provides access to the Graph URIs of Graphs in the Collection
        /// </summary>
        public override IEnumerable<Uri> GraphUris
        {
            get
            {
                List<Uri> uris = new List<Uri>();
                try
                {
                    EnterReadLock();
                    uris = _graphs.GraphUris.ToList();
                }
                finally
                {
                    ExitReadLock();
                }
                return uris;
            }
        }

        /// <summary>
        /// Gets a Graph from the Collection
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <returns></returns>
        public override IGraph this[Uri graphUri]
        {
            get
            {
                IGraph g = null;
                try
                {
                    EnterReadLock();
                    g = _graphs[graphUri];
                }
                finally
                {
                    ExitReadLock();
                }
                return g;
            }
        }

        /// <summary>
        /// Disposes of the Graph Collection
        /// </summary>
        /// <remarks>Invokes the <strong>Dispose()</strong> method of all Graphs contained in the Collection</remarks>
        public override void Dispose()
        {
            try
            {
                EnterWriteLock();
                _graphs.Dispose();
            }
            finally
            {
                ExitWriteLock();
            }
        }
    }

}
