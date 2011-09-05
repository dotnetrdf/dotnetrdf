/*

Copyright Robert Vesse 2009-11
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
            this._t = t;
            this._delete = toDelete;
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
                return this._t;
            }
        }

        /// <summary>
        /// Gets whether the action is a Delete Action
        /// </summary>
        public bool IsDelete
        {
            get
            {
                return this._delete;
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
            this._g = g;
            this._action = action;
        }

        /// <summary>
        /// Gets the Graph to be persisted
        /// </summary>
        public ITransactionalGraph Graph
        {
            get
            {
                return this._g;
            }
        }

        /// <summary>
        /// Gets the Action Type
        /// </summary>
        public GraphPersistenceActionType Action
        {
            get
            {
                return this._action;
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
            this._graphAction = graphAction;
        }

        /// <summary>
        /// Creates a new persistence action that pertains to a Triple
        /// </summary>
        /// <param name="tripleAction">Triple Action</param>
        public TripleStorePersistenceAction(TriplePersistenceAction tripleAction)
        {
            this._tripleAction = tripleAction;
        }

        /// <summary>
        /// Gets whether this action pertains to a Graph
        /// </summary>
        public bool IsGraphAction
        {
            get
            {
                return this._graphAction != null;
            }
        }

        /// <summary>
        /// Gets whether this action peratins to a Triple
        /// </summary>
        public bool IsTripleAction
        {
            get
            {
                return this._tripleAction != null;
            }
        }

        /// <summary>
        /// Gets the Graph Action (if any)
        /// </summary>
        public GraphPersistenceAction GraphAction
        {
            get
            {
                return this._graphAction;
            }
        }

        /// <summary>
        /// Gets the Triple Action (if any)
        /// </summary>
        public TriplePersistenceAction TripleAction
        {
            get
            {
                return this._tripleAction;
            }
        }
    }
}
