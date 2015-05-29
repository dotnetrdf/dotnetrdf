/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using VDS.RDF.Collections;
using VDS.RDF.Query;

namespace VDS.RDF.Graphs
{
    /// <summary>
    /// Class for representing RDF Graphs
    /// </summary>
    /// <threadsafety instance="false">Safe for multi-threaded read-only access but unsafe if one/more threads may modify the Graph by using the <see cref="Graph.Assert(Triple)">Assert</see>, <see cref="Graph.Retract(Triple)">Retract</see> or <see cref="BaseGraph.Merge(IGraph)">Merge</see> methods</threadsafety>
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="graph")]
#endif
    public class Graph 
        : BaseGraph
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of a Graph
        /// </summary>
        public Graph() 
            : base() { }

        /// <summary>
        /// Creates a new instance of a Graph with an optionally empty Namespace Map
        /// </summary>
        /// <param name="emptyNamespaceMap">Whether the Namespace Map should be empty</param>
        public Graph(bool emptyNamespaceMap)
            : this()
        {
            if (emptyNamespaceMap) this._nsmapper.Clear();
        }

        /// <summary>
        /// Creates a new instance of a Graph using the given Triple Collection
        /// </summary>
        /// <param name="tripleCollection">Triple Collection</param>
        public Graph(ITripleCollection tripleCollection)
            : base(tripleCollection) { }

        /// <summary>
        /// Creates a new instance of a Graph using the given Triple Collection and an optionally empty Namespace Map
        /// </summary>
        /// <param name="tripleCollection">Triple Collection</param>
        /// <param name="emptyNamespaceMap">Whether the Namespace Map should be empty</param>
        public Graph(ITripleCollection tripleCollection, bool emptyNamespaceMap)
            : base(tripleCollection)
        {
            if (emptyNamespaceMap) this._nsmapper.Clear();
        }

#if !SILVERLIGHT
        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected Graph(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
#endif

        #endregion

        #region Triple Assertion & Retraction

        /// <summary>
        /// Asserts a Triple in the Graph
        /// </summary>
        /// <param name="t">The Triple to add to the Graph</param>
        public override void Assert(Triple t)
        {
            //Add to Triples Collection
            this._triples.Add(t);
        }

        /// <summary>
        /// Asserts a List of Triples in the graph
        /// </summary>
        /// <param name="ts">List of Triples in the form of an IEnumerable</param>
        public override void Assert(IEnumerable<Triple> ts)
        {
            this._triples.AddRange(ts);
        }

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">Triple to Retract</param>
        /// <remarks>Current implementation may have some defunct Nodes left in the Graph as only the Triple is retracted</remarks>
        public override void Retract(Triple t)
        {
            this._triples.Remove(t);
        }

        /// <summary>
        /// Retracts a enumeration of Triples from the graph
        /// </summary>
        /// <param name="ts">Enumeration of Triples to retract</param>
        public override void Retract(IEnumerable<Triple> ts)
        {
            this._triples.RemoveRange(ts);
        }

        #endregion
    }

    /// <summary>
    /// Class for representing RDF Graphs when you don't want Indexing
    /// </summary>
    /// <remarks>
    /// Gives better load performance but poorer lookup performance
    /// </remarks>
    public class NonIndexedGraph
        : Graph
    {
        /// <summary>
        /// Creates a new Graph which is not indexed
        /// </summary>
        public NonIndexedGraph()
            : base(new TripleCollection()) { }
    }
}
