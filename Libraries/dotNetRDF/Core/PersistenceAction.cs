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

namespace VDS.RDF
{
    /// <summary>
    /// Represents a Triple that is queued for persistence (either insertion/deletion)
    /// </summary>
    public class TriplePersistenceAction
    {
        private bool _delete = false;
        private Triple _t;

        /// <summary>
        /// Creates a new Triple Persistence Action (an insertion/deletion)
        /// </summary>
        /// <param name="t">Triple to persist</param>
        /// <param name="toDelete">Whether the Triple is to be deleted</param>
        public TriplePersistenceAction(Triple t, bool toDelete)
        {
            if (t == null) throw new ArgumentNullException("t");
            _t = t;
            _delete = toDelete;
        }

        /// <summary>
        /// Creates a new Triple Persistence Action (an insertion)
        /// </summary>
        /// <param name="t">Triple to persist</param>
        public TriplePersistenceAction(Triple t)
            : this(t, false) { }

        /// <summary>
        /// Gets the Triple to persist
        /// </summary>
        public Triple Triple
        {
            get
            {
                return _t;
            }
        }

        /// <summary>
        /// Gets whether the action is a Delete Action
        /// </summary>
        public bool IsDelete
        {
            get
            {
                return _delete;
            }
        }
    }

    /// <summary>
    /// Possible Types of Graph Persistence Actions
    /// </summary>
    public enum GraphPersistenceActionType
    {
        /// <summary>
        /// Graph was Added
        /// </summary>
        Added,
        /// <summary>
        /// Graph was Deleted
        /// </summary>
        Deleted,
        /// <summary>
        /// Graph was Modified
        /// </summary>
        Modified
    }

    /// <summary>
    /// Represents a Graph that is queued for persistence (added/modified/removed)
    /// </summary>
    public class GraphPersistenceAction
    {
        private ITransactionalGraph _g;
        private GraphPersistenceActionType _action;

        /// <summary>
        /// Creates a new Graph Persistence action
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="action">Action Type</param>
        public GraphPersistenceAction(IGraph g, GraphPersistenceActionType action)
            : this(new GraphPersistenceWrapper(g), action) { }

        /// <summary>
        /// Creates a new Graph Persistence action
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="action">Action Type</param>
        public GraphPersistenceAction(ITransactionalGraph g, GraphPersistenceActionType action)
        {
            _g = g;
            _action = action;
        }

        /// <summary>
        /// Gets the Graph to be persisted
        /// </summary>
        public ITransactionalGraph Graph
        {
            get
            {
                return _g;
            }
        }

        /// <summary>
        /// Gets the Action Type
        /// </summary>
        public GraphPersistenceActionType Action
        {
            get
            {
                return _action;
            }
        }
    }

    /// <summary>
    /// Represents an action on a Triple Store that is queued for persistence
    /// </summary>
    public class TripleStorePersistenceAction
    {
        private GraphPersistenceAction _graphAction;
        private TriplePersistenceAction _tripleAction;

        /// <summary>
        /// Creates a new persistence action that pertains to a Graph
        /// </summary>
        /// <param name="graphAction">Graph Action</param>
        public TripleStorePersistenceAction(GraphPersistenceAction graphAction)
        {
            _graphAction = graphAction;
        }

        /// <summary>
        /// Creates a new persistence action that pertains to a Triple
        /// </summary>
        /// <param name="tripleAction">Triple Action</param>
        public TripleStorePersistenceAction(TriplePersistenceAction tripleAction)
        {
            _tripleAction = tripleAction;
        }

        /// <summary>
        /// Gets whether this action pertains to a Graph
        /// </summary>
        public bool IsGraphAction
        {
            get
            {
                return _graphAction != null;
            }
        }

        /// <summary>
        /// Gets whether this action peratins to a Triple
        /// </summary>
        public bool IsTripleAction
        {
            get
            {
                return _tripleAction != null;
            }
        }

        /// <summary>
        /// Gets the Graph Action (if any)
        /// </summary>
        public GraphPersistenceAction GraphAction
        {
            get
            {
                return _graphAction;
            }
        }

        /// <summary>
        /// Gets the Triple Action (if any)
        /// </summary>
        public TriplePersistenceAction TripleAction
        {
            get
            {
                return _tripleAction;
            }
        }
    }
}
