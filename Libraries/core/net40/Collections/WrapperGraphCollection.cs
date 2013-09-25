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
    /// Abstract decorator for Graph Collections to make it easier to add new functionality to existing implementations
    /// </summary>
    public abstract class WrapperGraphCollection
        : BaseGraphCollection
    {
        /// <summary>
        /// Underlying Graph Collection
        /// </summary>
        protected readonly IGraphCollection _graphs;

        /// <summary>
        /// Creates a decorator around a default <see cref="GraphCollection"/> instance
        /// </summary>
        public WrapperGraphCollection()
            : this(new GraphCollection()) { }

        /// <summary>
        /// Creates a decorator around the given graph collection
        /// </summary>
        /// <param name="graphCollection">Graph Collection</param>
        public WrapperGraphCollection(IGraphCollection graphCollection)
        {
            if (graphCollection == null) throw new ArgumentNullException("graphCollection");
            this._graphs = graphCollection;
            this._graphs.GraphAdded += this.HandleGraphAdded;
            this._graphs.GraphRemoved += this.HandleGraphRemoved;
        }

        private void HandleGraphAdded(Object sender, GraphEventArgs args)
        {
            this.RaiseGraphAdded(args.Graph, args.GraphName);
        }

        private void HandleGraphRemoved(Object sender, GraphEventArgs args)
        {
            this.RaiseGraphRemoved(args.Graph, args.GraphName);
        }

        /// <summary>
        /// Adds a Graph to the collection
        /// </summary>
        /// <param name="graphName">Graph name</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public override void Add(INode graphName, IGraph g)
        {
            this._graphs.Add(graphName, g);
        }

        public override void Add(KeyValuePair<INode, IGraph> kvp)
        {
            this._graphs.Add(kvp);
        }

        /// <summary>
        /// Gets whether the collection contains the given Graph
        /// </summary>
        /// <param name="graphName">Graph URI</param>
        /// <returns></returns>
        public override bool ContainsKey(INode graphName)
        {
            return this._graphs.ContainsKey(graphName);
        }

        public override bool Contains(KeyValuePair<INode, IGraph> kvp)
        {
            return this._graphs.Contains(kvp);
        }

        /// <summary>
        /// Removes a Graph from the collection
        /// </summary>
        /// <param name="graphName">Graph URI</param>
        /// <returns></returns>
        public override bool Remove(INode graphName)
        {
            return this._graphs.Remove(graphName);
        }

        public override bool Remove(KeyValuePair<INode, IGraph> kvp)
        {
            return this._graphs.Remove(kvp);
        }

        public override void Clear()
        {
            this._graphs.Clear();
        }

        /// <summary>
        /// Gets a Graph from the collection
        /// </summary>
        /// <param name="graphName">Graph URI</param>
        /// <returns></returns>
        public override IGraph this[INode graphName]
        {
            get
            {
                return this._graphs[graphName];
            }
            set
            {
                this._graphs[graphName] = value;
            }
        }

        /// <summary>
        /// Gets the number of Graphs in the collection
        /// </summary>
        public override int Count
        {
            get
            {
                return this._graphs.Count;
            }
        }

        /// <summary>
        /// Disposes of the collection
        /// </summary>
        public override void Dispose()
        {
            this._graphs.Dispose();
        }

        /// <summary>
        /// Gets the enumerator for the collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<KeyValuePair<INode, IGraph>> GetEnumerator()
        {
            return this._graphs.GetEnumerator();
        }

        /// <summary>
        /// Gets the names of the Graphs in the collection
        /// </summary>
        public override ICollection<INode> Keys
        {
            get 
            {
                return this._graphs.Keys;
            }
        }

        /// <summary>
        /// Gets the graphs in the collection
        /// </summary>
        public override ICollection<IGraph> Values
        {
            get 
            {
                return this._graphs.Values;
            }
        }
    }
}
