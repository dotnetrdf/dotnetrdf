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

using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF
{
    /// <summary>
    /// A Graph which represents the Union of several Graphs
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Union is entirely virtual, the Graphs and their Triples are not actually physically merged together
    /// </para>
    /// <para>
    /// All Assert and Retract operations are directed only to the Default Graph while a Clear() operation will clear all Graphs in the Union
    /// </para>
    /// </remarks>
    public class UnionGraph 
        : Graph
    {
        private IGraph _default;
        private List<IGraph> _graphs;

        /// <summary>
        /// Creates a new Union Graph which is the Union of all the given Graphs with a specific Default Graph
        /// </summary>
        /// <param name="defaultGraph">Default Graph of the Union</param>
        /// <param name="graphs">Other Graphs in the Union</param>
        public UnionGraph(IGraph defaultGraph, IEnumerable<IGraph> graphs)
            : base(new UnionTripleCollection(defaultGraph.Triples, graphs.Where(g => !ReferenceEquals(defaultGraph, g)).Select(g => g.Triples)))
        {
            _default = defaultGraph;
            _graphs = graphs.Where(g => !ReferenceEquals(defaultGraph, g)).ToList();
        }

        /// <summary>
        /// Gets the Nodes of the Graph
        /// </summary>
        public override IEnumerable<INode> Nodes
        {
            get
            {
                return _default.Nodes.Concat(from g in _graphs
                                                  from n in g.Nodes
                                                  select n);
            }
        }

        /// <summary>
        /// Asserts some Triples in the Graph
        /// </summary>
        /// <param name="ts">Triples</param>
        /// <remarks>
        /// Assert and Retract operations are directed to the Default Graph of the Union.  We have to override the method to do this as although the <see cref="UnionTripleCollection">UnionTripleCollection</see> will direct asserts/retracts to Triple Collection of the default Graph we cannot guarantee that the Graph will be able to carry out any assertion/retraction logic (e.g. persistence) it might have implemented if the Assert/Retract bypasses the Assert/Retract method of the Default Graph
        /// </remarks>
        public override bool Assert(IEnumerable<Triple> ts)
        {
            return _default.Assert(ts);
        }

        /// <summary>
        /// Asserts s Triple in the Graph
        /// </summary>
        /// <param name="t">Triple</param>
        /// <remarks>
        /// Assert and Retract operations are directed to the Default Graph of the Union.  We have to override the method to do this as although the <see cref="UnionTripleCollection">UnionTripleCollection</see> will direct asserts/retracts to Triple Collection of the default Graph we cannot guarantee that the Graph will be able to carry out any assertion/retraction logic (e.g. persistence) it might have implemented if the Assert/Retract bypasses the Assert/Retract method of the Default Graph
        /// </remarks>
        public override bool Assert(Triple t)
        {
            return _default.Assert(t);
        }

        /// <summary>
        /// Retracts some Triples from the Graph
        /// </summary>
        /// <param name="ts">Triples</param>
        /// <remarks>
        /// Assert and Retract operations are directed to the Default Graph of the Union.  We have to override the method to do this as although the <see cref="UnionTripleCollection">UnionTripleCollection</see> will direct asserts/retracts to Triple Collection of the default Graph we cannot guarantee that the Graph will be able to carry out any assertion/retraction logic (e.g. persistence) it might have implemented if the Assert/Retract bypasses the Assert/Retract method of the Default Graph
        /// </remarks>
        public override bool Retract(IEnumerable<Triple> ts)
        {
            return _default.Retract(ts);
        }

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">Triple</param>
        /// <remarks>
        /// Assert and Retract operations are directed to the Default Graph of the Union.  We have to override the method to do this as although the <see cref="UnionTripleCollection">UnionTripleCollection</see> will direct asserts/retracts to Triple Collection of the default Graph we cannot guarantee that the Graph will be able to carry out any assertion/retraction logic (e.g. persistence) it might have implemented if the Assert/Retract bypasses the Assert/Retract method of the Default Graph
        /// </remarks>
        public override bool Retract(Triple t)
        {
            return _default.Retract(t);
        }

        /// <summary>
        /// Clears all the Graphs in the Union
        /// </summary>
        public override void Clear()
        {
            if (!RaiseClearRequested()) return;

            _default.Clear();
            foreach (IGraph g in _graphs)
            {
                g.Clear();
            }

            RaiseCleared();
        }
    }
}
