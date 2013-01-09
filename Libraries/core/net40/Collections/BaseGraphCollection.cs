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
using System.Text;

namespace VDS.RDF.Collections
{
    /// <summary>
    /// Abstract Base Class for Graph Collections
    /// </summary>
    public abstract class BaseGraphCollection 
        : IGraphCollection
    {
        /// <summary>
        /// Checks whether the Graph with the given Uri exists in this Graph Collection
        /// </summary>
        /// <param name="graphUri">Graph Uri to test</param>
        /// <returns>True if a graph with the given URI exists in the collection</returns>
        /// <remarks>
        /// The null URI is used to reference the Default Graph
        /// </remarks>
        public abstract bool ContainsKey(Uri graphUri);

        /// <summary>
        /// Checks whether the graph given is stored in the collection under the given URI
        /// </summary>
        /// <param name="kvp">URI and Graph pair</param>
        /// <returns>True if the graph given exists in the collection under the given URI</returns>
        public virtual bool Contains(KeyValuePair<Uri, IGraph> kvp)
        {
            IGraph g;
            if (this.TryGetValue(kvp.Key, out g))
            {
                return kvp.Value.Equals(g);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a graph to the collection
        /// </summary>
        /// <param name="g">Graph to add</param>
        /// <remarks>
        /// The null URI is used to reference the Default Graph
        /// </remarks>
        public abstract void Add(Uri graphUri, IGraph g);

        /// <summary>
        /// Adds a graph to the collection
        /// </summary>
        /// <param name="kvp">URI and Graph pair</param>
        /// <remarks>
        /// The null URI is used to reference the Default Graph
        /// </remarks>
        public virtual void Add(KeyValuePair<Uri, IGraph> kvp)
        {
            this.Add(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Clears the contents of the collection
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Removes a Graph from the collection
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to remove</param>
        /// <returns>True if a Graph is removed, false otherwise</returns>
        /// <remarks>
        /// The null URI is used to reference the Default Graph
        /// </remarks>
        public abstract bool Remove(Uri graphUri);

        /// <summary>
        /// Removes a Graph from the collection only if the contents of the graph exactly match the graph stored against that URI in the collection
        /// </summary>
        /// <param name="kvp">URI and Graph pair</param>
        /// <returns>True if a Graph is removed, false otherwise</returns>
        /// <remarks>
        /// The null URI is used to reference the Default Graph
        /// </remarks>
        public virtual bool Remove(KeyValuePair<Uri, IGraph> kvp)
        {
            IGraph g;
            if (this.TryGetValue(kvp.Key, out g))
            {
                if (kvp.Value.Equals(g))
                {
                    return this.Remove(kvp.Key);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the number of Graphs in the Collection
        /// </summary>
        public abstract int Count
        {
            get;
        }

        /// <summary>
        /// Gets whether the collection is read only
        /// </summary>
        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Provides access to the URIs of the graphs in the collection
        /// </summary>
        public abstract ICollection<Uri> Keys
        {
            get;
        }

        /// <summary>
        /// Provides access to the graphs in the collection
        /// </summary>
        public abstract ICollection<IGraph> Values
        {
            get;
        }

        /// <summary>
        /// Gets/Sets a graph in the collection
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns>The graph if it exists in the collection, otherwise an error is thrown</returns>
        /// <remarks>
        /// The null URI is used to reference the Default Graph
        /// </remarks>
        public abstract IGraph this[Uri graphUri]
        {
            get;
            set;
        }

        /// <summary>
        /// Tries to get the graph associated with a given URI from the collection
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="g">Graph</param>
        /// <returns>True if a graph with the given URI exists, false otherwise</returns>
        /// <remarks>
        /// The null URI is used to reference the Default Graph
        /// </remarks>
        public virtual bool TryGetValue(Uri graphUri, out IGraph g)
        {
            if (this.ContainsKey(graphUri))
            {
                g = this[graphUri];
                return true;
            }
            else
            {
                g = null;
                return false;
            }
        }

        /// <summary>
        /// Copies the contents of the collection to an array
        /// </summary>
        /// <param name="dest">Array to copy to</param>
        /// <param name="index">Index to start copying into the array at</param>
        public virtual void CopyTo(KeyValuePair<Uri, IGraph>[] dest, int index)
        {
            if (dest == null) throw new ArgumentNullException("dest", "Null destination array");
            if (index < 0) throw new ArgumentOutOfRangeException("Index < 0");
            if ((dest.Length - index) < this.Count) throw new ArgumentException("Insufficient space to copy");

            int i = index;
            foreach (KeyValuePair<Uri, IGraph> kvp in this)
            {
                dest[i] = kvp;
                i++;
            }
        }

        /// <summary>
        /// Disposes of the Graph Collection
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<KeyValuePair<Uri, IGraph>> GetEnumerator();

        /// <summary>
        /// Gets the Enumerator for this Collection
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Event which is raised when a Graph is added to the Collection
        /// </summary>
        public event GraphEventHandler GraphAdded;

        /// <summary>
        /// Event which is raised when a Graph is removed from the Collection
        /// </summary>
        public event GraphEventHandler GraphRemoved;

        /// <summary>
        /// Helper method which raises the <see cref="GraphAdded">Graph Added</see> event manually
        /// </summary>
        /// <param name="g">Graph</param>
        protected virtual void RaiseGraphAdded(IGraph g)
        {
            GraphEventHandler d = this.GraphAdded;
            if (d != null)
            {
                d(this, new GraphEventArgs(g));
            }
        }

        /// <summary>
        /// Helper method which raises the <see cref="GraphRemoved">Graph Removed</see> event manually
        /// </summary>
        /// <param name="g">Graph</param>
        protected virtual void RaiseGraphRemoved(IGraph g)
        {
            GraphEventHandler d = this.GraphRemoved;
            if (d != null)
            {
                d(this, new GraphEventArgs(g));
            }
        }
    }
}
