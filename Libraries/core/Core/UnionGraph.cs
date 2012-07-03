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
            this._default = defaultGraph;
            this._graphs = graphs.Where(g => !ReferenceEquals(defaultGraph, g)).ToList();
        }

        /// <summary>
        /// Gets the Nodes of the Graph
        /// </summary>
        public override IEnumerable<INode> Nodes
        {
            get
            {
                return this._default.Nodes.Concat(from g in this._graphs
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
        public override void Assert(IEnumerable<Triple> ts)
        {
            this._default.Assert(ts);
        }

        /// <summary>
        /// Asserts some Triples in the Graph
        /// </summary>
        /// <param name="ts">Triples</param>
        /// <remarks>
        /// Assert and Retract operations are directed to the Default Graph of the Union.  We have to override the method to do this as although the <see cref="UnionTripleCollection">UnionTripleCollection</see> will direct asserts/retracts to Triple Collection of the default Graph we cannot guarantee that the Graph will be able to carry out any assertion/retraction logic (e.g. persistence) it might have implemented if the Assert/Retract bypasses the Assert/Retract method of the Default Graph
        /// </remarks>
        public override void Assert(List<Triple> ts)
        {
            this._default.Assert(ts);
        }

        /// <summary>
        /// Asserts s Triple in the Graph
        /// </summary>
        /// <param name="t">Triple</param>
        /// <remarks>
        /// Assert and Retract operations are directed to the Default Graph of the Union.  We have to override the method to do this as although the <see cref="UnionTripleCollection">UnionTripleCollection</see> will direct asserts/retracts to Triple Collection of the default Graph we cannot guarantee that the Graph will be able to carry out any assertion/retraction logic (e.g. persistence) it might have implemented if the Assert/Retract bypasses the Assert/Retract method of the Default Graph
        /// </remarks>
        public override void Assert(Triple t)
        {
            this._default.Assert(t);
        }

        /// <summary>
        /// Asserts some Triples in the Graph
        /// </summary>
        /// <param name="ts">Triples</param>
        /// <remarks>
        /// Assert and Retract operations are directed to the Default Graph of the Union.  We have to override the method to do this as although the <see cref="UnionTripleCollection">UnionTripleCollection</see> will direct asserts/retracts to Triple Collection of the default Graph we cannot guarantee that the Graph will be able to carry out any assertion/retraction logic (e.g. persistence) it might have implemented if the Assert/Retract bypasses the Assert/Retract method of the Default Graph
        /// </remarks>
        public override void Assert(Triple[] ts)
        {
            this._default.Assert(ts);
        }

        /// <summary>
        /// Retracts some Triples from the Graph
        /// </summary>
        /// <param name="ts">Triples</param>
        /// <remarks>
        /// Assert and Retract operations are directed to the Default Graph of the Union.  We have to override the method to do this as although the <see cref="UnionTripleCollection">UnionTripleCollection</see> will direct asserts/retracts to Triple Collection of the default Graph we cannot guarantee that the Graph will be able to carry out any assertion/retraction logic (e.g. persistence) it might have implemented if the Assert/Retract bypasses the Assert/Retract method of the Default Graph
        /// </remarks>
        public override void Retract(IEnumerable<Triple> ts)
        {
            this._default.Retract(ts);
        }

        /// <summary>
        /// Retracts some Triples from the Graph
        /// </summary>
        /// <param name="ts">Triples</param>
        /// <remarks>
        /// Assert and Retract operations are directed to the Default Graph of the Union.  We have to override the method to do this as although the <see cref="UnionTripleCollection">UnionTripleCollection</see> will direct asserts/retracts to Triple Collection of the default Graph we cannot guarantee that the Graph will be able to carry out any assertion/retraction logic (e.g. persistence) it might have implemented if the Assert/Retract bypasses the Assert/Retract method of the Default Graph
        /// </remarks>
        public override void Retract(List<Triple> ts)
        {
            this._default.Retract(ts);
        }

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">Triple</param>
        /// <remarks>
        /// Assert and Retract operations are directed to the Default Graph of the Union.  We have to override the method to do this as although the <see cref="UnionTripleCollection">UnionTripleCollection</see> will direct asserts/retracts to Triple Collection of the default Graph we cannot guarantee that the Graph will be able to carry out any assertion/retraction logic (e.g. persistence) it might have implemented if the Assert/Retract bypasses the Assert/Retract method of the Default Graph
        /// </remarks>
        public override void Retract(Triple t)
        {
            this._default.Retract(t);
        }

        /// <summary>
        /// Retracts some Triples from the Graph
        /// </summary>
        /// <param name="ts">Triples</param>
        /// <remarks>
        /// Assert and Retract operations are directed to the Default Graph of the Union.  We have to override the method to do this as although the <see cref="UnionTripleCollection">UnionTripleCollection</see> will direct asserts/retracts to Triple Collection of the default Graph we cannot guarantee that the Graph will be able to carry out any assertion/retraction logic (e.g. persistence) it might have implemented if the Assert/Retract bypasses the Assert/Retract method of the Default Graph
        /// </remarks>
        public override void Retract(Triple[] ts)
        {
            this._default.Retract(ts);
        }

        /// <summary>
        /// Clears all the Graphs in the Union
        /// </summary>
        public override void Clear()
        {
            if (!this.RaiseClearRequested()) return;

            this._default.Clear();
            foreach (IGraph g in this._graphs)
            {
                g.Clear();
            }

            this.RaiseCleared();
        }
    }
}
