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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VDS.RDF
{
    /// <summary>
    /// Basic Node Collection
    /// </summary>
    public class NodeCollection : BaseNodeCollection, IEnumerable<INode>
    {
        /// <summary>
        /// Underlying storage of the Node Collection
        /// </summary>
        protected Dictionary<int, INode> _nodes;
        /// <summary>
        /// Underlying storage of the Node Collection which Handles the extra Nodes that result from Hash Code Collision
        /// </summary>
        protected List<INode> _collisionNodes;

        /// <summary>
        /// Creates a new NodeCollection
        /// </summary>
        public NodeCollection()
        {
            this._nodes = new Dictionary<int, INode>();
            this._collisionNodes = new List<INode>();
        }

        /// <summary>
        /// Checks whether the given Node is in the Node Collection
        /// </summary>
        /// <param name="n">The Node to test</param>
        /// <returns>Returns True if the Node is already in the collection</returns>
        public override bool Contains(INode n)
        {
            //Us the Hash Code in the Dictionary?
            if (this._nodes.ContainsKey(n.GetHashCode()))
            {
                //Are the two Nodes equal?
                if (this._nodes[n.GetHashCode()].Equals(n))
                {
                    return true;
                }
                else
                {
                    //There's a Hash Code Collision
                    //Is the Node in the Collision Nodes list?
                    return this._collisionNodes.Contains(n);
                }
            } 
            else 
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a new Node to the Collection
        /// </summary>
        /// <param name="n">Node to add</param>
        protected internal override void Add(INode n)
        {
            if (!this._nodes.ContainsKey(n.GetHashCode()))
            {
                this._nodes.Add(n.GetHashCode(), n);
            }
            else if (!this._nodes[n.GetHashCode()].Equals(n))
            {
                //Hash Code Collision
                this._nodes[n.GetHashCode()].Collides = true;
                n.Collides = true;

                //Add to Collision Nodes List
                this._collisionNodes.Add(n);
            }
        }

        /// <summary>
        /// Gets the Number of Nodes in the Collection
        /// </summary>
        public override int Count
        {
            get
            {
                return (this._nodes.Count + this._collisionNodes.Count);
            }
        }

        /// <summary>
        /// Gets all the Blank Nodes in the Collection
        /// </summary>
        public override IEnumerable<IBlankNode> BlankNodes
        {
            get
            {
                IEnumerable<IBlankNode> bs = from n in this
                                            where n.NodeType == NodeType.Blank
                                            select (IBlankNode)n;

                return bs;
            }
        }

        /// <summary>
        /// Gets all the Literal Nodes in the Collection
        /// </summary>
        public override IEnumerable<ILiteralNode> LiteralNodes
        {
            get
            {
                IEnumerable<ILiteralNode> ls = from n in this
                                              where n.NodeType == NodeType.Literal
                                              select (ILiteralNode)n;

                return ls;
            }
        }

        /// <summary>
        /// Gets all the Uri Nodes in the Collection
        /// </summary>
        public override IEnumerable<IUriNode> UriNodes
        {
            get
            {
                IEnumerable<IUriNode> us = from n in this
                                          where n.NodeType == NodeType.Uri
                                          select (IUriNode)n;
                return us;
            }
        }

        /// <summary>
        /// Gets all the Graph Literal Nodes in the Collection
        /// </summary>
        public override IEnumerable<IGraphLiteralNode> GraphLiteralNodes
        {
            get
            {
                IEnumerable<IGraphLiteralNode> gs = from n in this
                                                   where n.NodeType == NodeType.GraphLiteral
                                                   select (IGraphLiteralNode)n;

                return gs;
            }
        }

        #region IEnumerable<INode> Members

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<INode> GetEnumerator()
        {
            if (this._collisionNodes.Count > 0)
            {
                //Some Collision Nodes exist
                //Need to concatenate the main Node Collection with the Collision Nodes list
                return (from n in this._nodes.Values
                        select n).Concat(this._collisionNodes).GetEnumerator();
            }
            else
            {
                //No Collision Nodes exist
                //Just give back the Enumerator for the main Node Collection
                return this._nodes.Values.GetEnumerator();
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of a Node Collection
        /// </summary>
        public override void Dispose()
        {
            this._nodes.Clear();
            this._collisionNodes.Clear();
        }

        #endregion
    }


#if !NO_RWLOCK

    /// <summary>
    /// Thread Safe Node Collection
    /// </summary>
    public class ThreadSafeNodeCollection : NodeCollection, IEnumerable<INode>
    {
        private ReaderWriterLockSlim _lockManager = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Adds a new Node to the Collection
        /// </summary>
        /// <param name="n">Node to add</param>
        protected internal override void Add(INode n)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                base.Add(n);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets all the Blank Nodes in the Collection
        /// </summary>
        public override IEnumerable<IBlankNode> BlankNodes
        {
            get
            {
                List<IBlankNode> nodes = new List<IBlankNode>();
                try
                {
                    this._lockManager.EnterReadLock();
                    nodes = base.BlankNodes.ToList();
                }
                finally
                {
                    this._lockManager.ExitReadLock();
                }
                return nodes;
            }
        }

        /// <summary>
        /// Checks whether the given Node is in the Node Collection
        /// </summary>
        /// <param name="n">The Node to test</param>
        /// <returns>Returns True if the Node is already in the collection</returns>
        public override bool Contains(INode n)
        {
            bool contains = false;
            try
            {
                this._lockManager.EnterReadLock();
                contains = base.Contains(n);
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return contains;
        }

        /// <summary>
        /// Gets the Number of Nodes in the Collection
        /// </summary>
        public override int Count
        {
            get
            {
                int c = 0;
                try
                {
                    this._lockManager.EnterReadLock();
                    c = base.Count;
                }
                finally
                {
                    this._lockManager.ExitReadLock();
                }
                return c;
            }
        }

        /// <summary>
        /// Disposes of a Node Collection
        /// </summary>
        public override void Dispose()
        {
            try
            {
                this._lockManager.EnterWriteLock();
                base.Dispose();
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<INode> GetEnumerator()
        {
            List<INode> nodes = new List<INode>();
            try
            {
                this._lockManager.EnterReadLock();

                if (this._collisionNodes.Count > 0)
                {
                    nodes = (from n in this._nodes.Values
                             select n).Concat(this._collisionNodes).ToList();
                }
                else
                {
                    nodes = (from n in this._nodes.Values
                             select n).ToList();
                }
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return nodes.GetEnumerator();
        }

        /// <summary>
        /// Gets all the Graph Literal Nodes in the Collection
        /// </summary>
        public override IEnumerable<IGraphLiteralNode> GraphLiteralNodes
        {
            get
            {
                List<IGraphLiteralNode> nodes = new List<IGraphLiteralNode>();
                try
                {
                    this._lockManager.EnterReadLock();
                    nodes = base.GraphLiteralNodes.ToList();
                }
                finally
                {
                    this._lockManager.ExitReadLock();
                }
                return nodes;
            }
        }

        /// <summary>
        /// Gets all the Literal Nodes in the Collection
        /// </summary>
        public override IEnumerable<ILiteralNode> LiteralNodes
        {
            get
            {
                List<ILiteralNode> nodes = new List<ILiteralNode>();
                try
                {
                    this._lockManager.EnterReadLock();
                    nodes = base.LiteralNodes.ToList();
                }
                finally
                {
                    this._lockManager.ExitReadLock();
                }
                return nodes;
            }
        }

        /// <summary>
        /// Gets all the Uri Nodes in the Collection
        /// </summary>
        public override IEnumerable<IUriNode> UriNodes
        {
            get
            {
                List<IUriNode> nodes = new List<IUriNode>();
                try
                {
                    this._lockManager.EnterReadLock();
                    nodes = base.UriNodes.ToList();
                }
                finally
                {
                    this._lockManager.ExitReadLock();
                }
                return nodes;
            }
        }
    }

#endif
}
