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
            : base(new UnionTripleCollection(defaultGraph.Triples, graphs.Where(g => !ReferenceEquals(defaultGraph, g)).Select(g => g.Triples)), new UnionNodeCollection(defaultGraph.Nodes, graphs.Where(g => !ReferenceEquals(defaultGraph, g)).Select(g => g.Nodes)))
        {
            this._default = defaultGraph;
            this._graphs = graphs.Where(g => !ReferenceEquals(defaultGraph, g)).ToList();
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
