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
    /// Wrapper class for Graph Collections
    /// </summary>
    public class GraphCollection 
        : BaseGraphCollection, IEnumerable<IGraph>
    {
        /// <summary>
        /// Constant used as the URI for Default Graphs (which is the Graph with the Null Base URI) for the purposes of getting a Hash Code for the Graph URI
        /// </summary>
        public const String DefaultGraphUri = "dotnetrdf:default-graph";

        /// <summary>
        /// Dictionary of Graph Uri Enhanced Hash Codes to Graphs
        /// </summary>
        /// <remarks>See <see cref="Extensions.GetEnhancedHashCode">GetEnhancedHashCode()</see></remarks>
        protected Dictionary<int, IGraph> _graphs = new Dictionary<int, IGraph>();
        /// <summary>
        /// List of Graphs which handles Graphs which have Hash Code collisions
        /// </summary>
        protected List<IGraph> _collisionGraphs = new List<IGraph>();

        /// <summary>
        /// Checks whether the Graph with the given Uri exists in this Graph Collection
        /// </summary>
        /// <param name="graphUri">Graph Uri to test</param>
        /// <returns></returns>
        public override bool Contains(Uri graphUri)
        {
            int id;
            if (graphUri == null)
            {
                id = new Uri(DefaultGraphUri).GetEnhancedHashCode();
            }
            else
            {
                id = graphUri.GetEnhancedHashCode();
            }
            if (this._graphs.ContainsKey(id)) 
            {
                //Check Ordinal Equality of String form of URIs to detect Hash Code collision
                if (this._graphs[id].BaseUri != null && this._graphs[id].BaseUri.ToString().Equals(graphUri.ToSafeString(), StringComparison.Ordinal))
                {
                    return true;
                }
                else if (this._graphs[id].BaseUri == null && graphUri == null)
                {
                    return true;
                }
                else
                {
                    //Hash Code Collision
                    //See if is in list of Collision Graphs
                    return this._collisionGraphs.Any(g => (g.BaseUri == null && graphUri == null) || g.BaseUri.ToString().Equals(graphUri.ToSafeString(), StringComparison.Ordinal));
                  }
            } 
            else 
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a Graph to the Collection
        /// </summary>
        /// <param name="g">Graph to add</param>
        /// <param name="mergeIfExists">Sets whether the Graph should be merged with an existing Graph of the same Uri if present</param>
        /// <exception cref="RdfException">Throws an RDF Exception if the Graph has no Base Uri or if the Graph already exists in the Collection and the <paramref name="mergeIfExists"/> parameter was not set to true</exception>
        protected internal override void Add(IGraph g, bool mergeIfExists)
        {
            //Graphs added to a Graph Collection must have a Base Uri
            int id;
            if (g.BaseUri == null)
            {
                id = new Uri(DefaultGraphUri).GetEnhancedHashCode();
            }
            else
            {
                id = g.BaseUri.GetEnhancedHashCode();
            }
            if (this._graphs.ContainsKey(id))
            {
                //Check for Hash Code collisions
                if (this._graphs[id].BaseUri != null && this._graphs[id].BaseUri.ToString().Equals(g.BaseUri.ToString(), StringComparison.Ordinal))
                {
                    //Already exists in the Graph Collection
                    if (mergeIfExists)
                    {
                        //Merge into the existing Graph
                        this._graphs[id].Merge(g);
                    }
                    else
                    {
                        //Not allowed
                        throw new RdfException("The Graph you tried to add already exists in the Graph Collection and the mergeIfExists parameter was set to false");
                    }
                }
                else if (this._graphs[id].BaseUri == null && g.BaseUri == null)
                {
                    //Already exists in the Graph Collection
                    if (mergeIfExists)
                    {
                        //Merge into the existing Graph
                        this._graphs[id].Merge(g);
                    }
                    else
                    {
                        //Not allowed
                        throw new RdfException("The Graph you tried to add already exists in the Graph Collection and the mergeIfExists parameter was set to false");
                    }
                }
                else
                {
                    //Hash Code collision
                    IGraph temp = this._collisionGraphs.FirstOrDefault(graph => (graph.BaseUri == null && g.BaseUri == null) || graph.BaseUri.ToString().Equals(g.BaseUri.ToString(), StringComparison.Ordinal));
                    if (temp != null)
                    {
                        //Aready exists in Collision Graphs
                        if (mergeIfExists)
                        {
                            temp.Merge(g);
                        }
                        else
                        {
                            //Not allowed
                            throw new RdfException("The Graph you tried to add already exists in the Graph Collection and the mergeIfExists parameter was set to false");
                        }
                    }
                    else
                    {
                        //Add to collision Graphs
                        this._collisionGraphs.Add(g);
                        this.RaiseGraphAdded(g);
                    }
                }
            }
            else
            {
                //Safe to add a new Graph
                this._graphs.Add(id, g);
                this.RaiseGraphAdded(g);
            }
        }

        /// <summary>
        /// Removes a Graph from the Collection
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to remove</param>
        protected internal override void Remove(Uri graphUri)
        {
            int id;
            if (graphUri == null)
            {
                id = new Uri(DefaultGraphUri).GetEnhancedHashCode();
            }
            else
            {
                id = graphUri.GetEnhancedHashCode();
            }
            if (this._graphs.ContainsKey(id))
            {
                if (this._graphs[id].BaseUri != null && this._graphs[id].BaseUri.ToString().Equals(graphUri.ToString(), StringComparison.Ordinal))
                {
                    IGraph temp = this._graphs[id];
                    this._graphs.Remove(id);
                    this.RaiseGraphRemoved(temp);

                    //Were there any collisions on this Hash Code?
                    //Q: Do we need a more general fix for null Base URI here?
                    if (this._collisionGraphs.Any(g => g.BaseUri != null && g.BaseUri.GetEnhancedHashCode().Equals(id)))
                    {
                        IGraph first = this._collisionGraphs.First(g => g.BaseUri != null && g.BaseUri.GetEnhancedHashCode().Equals(id));
                        this._graphs.Add(id, first);
                        this._collisionGraphs.Remove(first);
                    }
                }
                else if (this._graphs[id].BaseUri == null && graphUri == null)
                {
                    IGraph temp = this._graphs[id];
                    this._graphs.Remove(id);
                    this.RaiseGraphRemoved(temp);
                    
                    //Were there any collisions on this Hash Code?
                    //Q: Do we need a more general fix for null Base URI here?
                    if (this._collisionGraphs.Any(g => g.BaseUri != null && g.BaseUri.GetEnhancedHashCode().Equals(id)))
                    {
                        IGraph first = this._collisionGraphs.First(g => g.BaseUri != null && g.BaseUri.GetEnhancedHashCode().Equals(id));
                        this._graphs.Add(id, first);
                        this._collisionGraphs.Remove(first);
                    }
                }
                else
                {
                    //Hash Code collision
                    //Remove from Collision Graphs list
                    IGraph temp = this._collisionGraphs.First(g => (g.BaseUri == null && graphUri == null) || g.BaseUri.ToString().Equals(graphUri.ToString(), StringComparison.Ordinal));
                    this._collisionGraphs.RemoveAll(g => (g.BaseUri == null && graphUri == null) || g.BaseUri.ToString().Equals(graphUri.ToString(), StringComparison.Ordinal));
                    this.RaiseGraphRemoved(temp);
                }
            }
        }

        /// <summary>
        /// Gets the number of Graphs in the Collection
        /// </summary>
        public override int Count
        {
            get
            {
                return this._graphs.Count + this._collisionGraphs.Count;
            }
        }

        /// <summary>
        /// Provides access to the Graph URIs of Graphs in the Collection
        /// </summary>
        public override IEnumerable<Uri> GraphUris
        {
            get
            {
                return (from g in this
                            select g.BaseUri);
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
                int id;
                if (graphUri == null)
                {
                    id = new Uri(DefaultGraphUri).GetEnhancedHashCode();
                }
                else
                {
                    id = graphUri.GetEnhancedHashCode();
                }
                if (this._graphs.ContainsKey(id))
                {
                    //Check for collisions here
                    if (this._graphs[id].BaseUri != null && this._graphs[id].BaseUri.ToString().Equals(graphUri.ToString(), StringComparison.Ordinal))
                    {
                        return this._graphs[id];
                    }
                    else if (this._graphs[id].BaseUri == null && graphUri == null)
                    {
                        return this._graphs[id];
                    }
                    else
                    {
                        //Is the relevant Graph in the Collision Graphs List
                        IGraph temp = this._collisionGraphs.FirstOrDefault(g => g.BaseUri.ToString().Equals(graphUri.ToString(), StringComparison.Ordinal));
                        if (temp == null)
                        {
                            throw new RdfException("The Graph with the given URI does not exist in this Graph Collection");
                        }
                        else
                        {
                            return temp;
                        }
                    }
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
            //Concatenate in the Collision Graphs if required
            if (this._collisionGraphs.Count > 0)
            {
                return this._graphs.Values.Concat(this._collisionGraphs).GetEnumerator();
            }
            else
            {
                return this._graphs.Values.GetEnumerator();
            }
        }

        /// <summary>
        /// Gets the Enumerator for this Collection
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Disposes of the Graph Collection
        /// </summary>
        /// <remarks>Invokes the <see cref="IGraph.Dipose">Dispose()</see> method of all Graphs contained in the Collection</remarks>
        public override void Dispose()
        {
            foreach (IGraph g in this._graphs.Values)
            {
                g.Dispose();
            }
            foreach (IGraph g in this._collisionGraphs)
            {
                g.Dispose();
            }
            this._graphs.Clear();
            this._collisionGraphs.Clear();
        }
    }

#if !NO_RWLOCK

    /// <summary>
    /// Thread Safe Graph Collection
    /// </summary>
    public class ThreadSafeGraphCollection : GraphCollection, IEnumerable<IGraph>
    {
        private ReaderWriterLockSlim _lockManager = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

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
                this._lockManager.EnterReadLock();
                contains = base.Contains(graphUri);
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return contains;
        }

        /// <summary>
        /// Adds a Graph to the Collection
        /// </summary>
        /// <param name="g">Graph to add</param>
        /// <param name="mergeIfExists">Sets whether the Graph should be merged with an existing Graph of the same Uri if present</param>
        /// <exception cref="RdfException">Throws an RDF Exception if the Graph has no Base Uri or if the Graph already exists in the Collection and the <paramref name="mergeIfExists"/> parameter was not set to true</exception>
        protected internal override void Add(IGraph g, bool mergeIfExists)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                base.Add(g, mergeIfExists);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes a Graph from the Collection
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to remove</param>
        protected internal override void Remove(Uri graphUri)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                base.Remove(graphUri);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
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
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<IGraph> GetEnumerator()
        {
            List<IGraph> graphs = new List<IGraph>();
            try
            {
                this._lockManager.EnterReadLock();
                graphs = (from g in this._graphs.Values
                          select g).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
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
                    this._lockManager.EnterReadLock();
                    uris = base.GraphUris.ToList();
                }
                finally
                {
                    this._lockManager.ExitReadLock();
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
                    this._lockManager.EnterReadLock();
                    g = base[graphUri];
                }
                finally
                {
                    this._lockManager.ExitReadLock();
                }
                return g;
            }
        }

        /// <summary>
        /// Disposes of the Graph Collection
        /// </summary>
        /// <remarks>Invokes the <see cref="IGraph.Dipose">Dispose()</see> method of all Graphs contained in the Collection</remarks>
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
    }

#endif

}
