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
    /// Implementation of a Graph Difference algorithm for RDF Graphs
    /// </summary>
    /// <remarks>
    /// <para>
    /// This algorithm is broadly based upon the methodology fror computing differences in RDF Graphs described in the <a href="http://www.springerlink.com/index/lq65211003774313.pdf">RDFSync</a> paper by Tummarello et al.  This is an implementation purely of a difference algorithm and not the synchronisation aspects described in their paper.  Main difference between their algorithm and mine is that mine does not make the input Graphs lean as it is concerned with showing the raw differences between the Graphs and does not concern itself with whether the differences may be semantically irrelevant.
    /// </para>
    /// <para>
    /// To understand this consider the following Graphs:
    /// </para>
    /// <h2>Graph A</h2>
    /// <code>
    /// _:autos1 rdfs:label "Rob" .
    /// </code>
    /// <h2>Graph B</h2>
    /// <code>
    /// _:autos1 rdfs:label "Rob" .
    /// _:autos2 rdfs:label "Rob" .
    /// </code>
    /// <para>
    /// Given these Graphs computing the Graph Difference between A and B would report an Added MSG (Minimal Spanning Graph) when in fact the 2nd Graph is non-lean and could be reduced to the same as the 1st Graph
    /// </para>
    /// </remarks>
    public class GraphDiff
    {
        private HashSet<Triple> _lhsUnassigned = new HashSet<Triple>();
        private HashSet<Triple> _rhsUnassigned = new HashSet<Triple>();
        private List<IGraph> _lhsMSGs = new List<IGraph>();
        private List<IGraph> _rhsMSGs = new List<IGraph>();

        /// <summary>
        /// Calculates the Difference between the two Graphs i.e. the changes required to get from the 1st Graph to the 2nd Graph
        /// </summary>
        /// <param name="a">First Graph</param>
        /// <param name="b">Second Graph</param>
        /// <returns></returns>
        public GraphDiffReport Difference(IGraph a, IGraph b)
        {
            GraphDiffReport report = new GraphDiffReport();

            //Firstly check for Graph Equality
            Dictionary<INode,INode> equalityMapping = new Dictionary<INode,INode>();
            if (a.Equals(b, out equalityMapping))
            {
                //If Graphs are equal set AreEqual to true, assign the mapping and return
                report.AreEqual = true;
                if (equalityMapping != null) report.Mapping = equalityMapping;
                return report;
            }

            //Next check for changes in Ground Triples
            //Iterate over the Ground Triples in the 1st Graph to find those that have been removed in the 2nd
            foreach (Triple t in a.Triples.Where(t => t.IsGroundTriple))
            {
                if (!b.Triples.Contains(t))
                {
                    report.AddRemovedTriple(t);
                }
            }
            //Iterate over the Ground Triples in the 2nd Graph to find those that have been added in the 2nd
            foreach (Triple t in b.Triples.Where(t => t.IsGroundTriple))
            {
                if (!a.Triples.Contains(t))
                {
                    report.AddAddedTriple(t);
                }
            }

            //Do we need to compute MSGs?
            //If all Triples are Ground Triples then this step gets skipped which saves on computation
            if (a.Triples.Any(t => !t.IsGroundTriple) || b.Triples.Any(t => !t.IsGroundTriple))
            {
                //Some non-ground Triples so start computing MSGs

                //First build 2 HashSets of the non-ground Triples from the Graphs
                foreach (Triple t in a.Triples.Where(t => !t.IsGroundTriple))
                {
                    this._lhsUnassigned.Add(t);
                }
                foreach (Triple t in b.Triples.Where(t => !t.IsGroundTriple))
                {
                    this._rhsUnassigned.Add(t);
                }

                //Then compute all the MSGs
                this.ComputeMSGs(a, this._lhsUnassigned, this._lhsMSGs);
                this.ComputeMSGs(b, this._rhsUnassigned, this._rhsMSGs);

                //Sort MSGs by size - this is just so we start checking MSG equality from smallest MSGs first for efficiency
                GraphSizeComparer comparer = new GraphSizeComparer();
                this._lhsMSGs.Sort(comparer);
                this._rhsMSGs.Sort(comparer);

                //Now start trying to match MSG
                foreach (IGraph msg in this._lhsMSGs)
                {
                    //Get Candidate MSGs from RHS i.e. those of equal size
                    List<IGraph> candidates = (from g in this._rhsMSGs
                                               where g.Triples.Count == msg.Triples.Count
                                               select g).ToList();

                    if (candidates.Count == 0)
                    {
                        //No Candidate Matches so this MSG is not present in the 2nd Graph so add to report as a Removed MSG
                        report.AddRemovedMSG(msg);
                    }
                    else
                    {
                        //Do any of the candidates match?
                        bool hasMatch = false;
                        foreach (IGraph candidate in candidates)
                        {
                            Dictionary<INode, INode> tempMapping = new Dictionary<INode, INode>();
                            if (msg.Equals(candidate, out tempMapping))
                            {
                                //This MSG has a Match in the 2nd Graph so add the Mapping information
                                hasMatch = true;
                                try
                                {
                                    this.MergeMapping(report, tempMapping);
                                }
                                catch (RdfException)
                                {
                                    //If the Mapping cannot be merged it is a bad mapping and we try other candidates
                                    hasMatch = false;
                                    continue;
                                }

                                //Remove the matched MSG from the RHS MSGs so we cannot match another LHS MSG to it later
                                //We use ReferenceEquals for this remove to avoid potentially costly Graph Equality calculations
                                this._rhsMSGs.RemoveAll(g => ReferenceEquals(g, candidate));
                            }
                        }

                        //No match was found so the MSG is removed from the 2nd Graph
                        if (!hasMatch) report.AddRemovedMSG(msg);   
                    }
                }

                //If we are left with any MSGs in the RHS then these are added MSG
                foreach (IGraph msg in this._rhsMSGs)
                {
                    report.AddAddedMSG(msg);
                }
            }
            return report;
        }

        /// <summary>
        /// Computes MSGs for a Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="unassigned">Triples that need assigning to MSGs</param>
        /// <param name="msgs">MSGs list to populate</param>
        private void ComputeMSGs(IGraph g, HashSet<Triple> unassigned, List<IGraph> msgs)
        {
            //While we have unassigned Triples build MSGs
            while (unassigned.Count > 0)
            {
                HashSet<INode> processed = new HashSet<INode>();
                Queue<INode> unprocessed = new Queue<INode>();

                //Get next Triple from unassigned
                Triple first = unassigned.First();
                unassigned.Remove(first);

                //Get the BNodes from it that need to be processed
                this.GetNodesForProcessing(first, unprocessed);

                //Start building an MSG starting from the Triple
                Graph msg = new Graph();
                msg.Assert(first);
                while (unprocessed.Count > 0)
                {
                    INode next = unprocessed.Dequeue();
                    //Can safely skip Nodes we've already processed
                    if (processed.Contains(next)) continue;

                    //Get all the Triples that use the given Node and find any additional Blank Nodes for processing
                    foreach (Triple t in g.GetTriples(next))
                    {
                        //When a Triple is added to an MSG it is removed from the unassigned list
                        unassigned.Remove(t);
                        msg.Assert(t);
                        this.GetNodesForProcessing(t, unprocessed);
                    }

                    processed.Add(next);
                }

                msgs.Add(msg);
            }
        }

        private void GetNodesForProcessing(Triple t, Queue<INode> nodes)
        {
            if (t.Subject.NodeType == NodeType.Blank) nodes.Enqueue(t.Subject);
            if (t.Predicate.NodeType == NodeType.Blank) nodes.Enqueue(t.Predicate);
            if (t.Object.NodeType == NodeType.Blank) nodes.Enqueue(t.Object);
        }

        private void MergeMapping(GraphDiffReport report, Dictionary<INode, INode> mapping)
        {
            //This first run through ensures the mappings don't conflict in which case it is an invalid mapping
            foreach (KeyValuePair<INode, INode> kvp in mapping)
            {
                if (report.Mapping.ContainsKey(kvp.Key))
                {
                    if (!report.Mapping[kvp.Key].Equals(kvp.Value)) throw new RdfException("Error in GraphDiff - " + kvp.Key.ToString() + " is already mapped to " + report.Mapping[kvp.Key].ToString() + " so cannot be remapped to " + kvp.Value.ToString());
                }
            }
            //The second run through does the actual merge
            foreach (KeyValuePair<INode, INode> kvp in mapping)
            {
                report.Mapping.Add(kvp.Key, kvp.Value);
            }
        }
    }

    /// <summary>
    /// Represents the Differences between 2 Graphs
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Diff represents the Difference between the 2 Graphs at the time the Difference was calculated - if the Graphs subsequently change then the Diff must be recalculated
    /// </para>
    /// </remarks>
    public class GraphDiffReport
    {
        private bool _areEqual = false;
        private Dictionary<INode, INode> _mapping = new Dictionary<INode,INode>();
        private List<Triple> _addedTriples = new List<Triple>();
        private List<Triple> _removedTriples = new List<Triple>();
        private List<IGraph> _addedMSGs = new List<IGraph>();
        private List<IGraph> _removedMSGs = new List<IGraph>();

        /// <summary>
        /// Gets whether the Graphs were equal at the time the Diff was calculated
        /// </summary>
        public bool AreEqual
        {
            get
            {
                return this._areEqual;
            }
            internal set
            {
                this._areEqual = value;
            }
        }

        /// <summary>
        /// Provides the mapping from Blank Nodes in 1 Graph to Blank Nodes in another
        /// </summary>
        /// <remarks>
        /// <para>
        /// In the case of Equal Graphs this will be a complete mapping, if the Graphs are different then it will be an empty/partial mapping depending on whether Blank Nodes can be mapped from one Graph to another or not
        /// </para>
        /// </remarks>
        public Dictionary<INode, INode> Mapping
        {
            get
            {
                return this._mapping;
            }
            internal set
            {
                this._mapping = value;
            }
        }

        /// <summary>
        /// Gets the Ground Triples (i.e. no Blank Nodes) that must be added to the 1st Graph to get the 2nd Graph
        /// </summary>
        public IEnumerable<Triple> AddedTriples
        {
            get
            {
                return this._addedTriples;
            }
        }

        /// <summary>
        /// Gets the Ground Triples (i.e. no Blank Nodes) that must be removed from the 1st Graph to get the 2nd Graph
        /// </summary>
        public IEnumerable<Triple> RemovedTriples
        {
            get
            {
                return this._removedTriples;
            }
        }

        /// <summary>
        /// Gets the MSGs (Minimal Spanning Graphs i.e. sets of Triples sharing common Blank Nodes) that must be added to the 1st Graph to get the 2nd Graph
        /// </summary>
        public IEnumerable<IGraph> AddedMSGs
        {
            get
            {
                return this._addedMSGs;
            }
        }

        /// <summary>
        /// Gets the MSGs (Minimal Spanning Graphs i.e. sets of Triples sharing common Blank Nodes) that must be added to the 1st Graph to get the 2nd Graph
        /// </summary>
        public IEnumerable<IGraph> RemovedMSGs
        {
            get
            {
                return this._removedMSGs;
            }
        }

        internal void AddAddedTriple(Triple t)
        {
            this._addedTriples.Add(t);
        }

        internal void AddRemovedTriple(Triple t)
        {
            this._removedTriples.Add(t);
        }

        internal void AddAddedMSG(IGraph g)
        {
            this._addedMSGs.Add(g);
        }

        internal void AddRemovedMSG(IGraph g)
        {
            this._removedMSGs.Add(g);
        }
    }

    /// <summary>
    /// A Comparer for Graphs which compares based on number of Triples
    /// </summary>
    /// <remarks>
    /// Used internally in computing Graph Differences but made a public Graph as it may occasionally come in useful
    /// </remarks>
    public class GraphSizeComparer : IComparer<IGraph>
    {
        /// <summary>
        /// Compares Graphs based on their number of Triples
        /// </summary>
        /// <param name="x">Graph</param>
        /// <param name="y">Graph</param>
        /// <returns></returns>
        public int Compare(IGraph x, IGraph y)
        {
            return x.Triples.Count.CompareTo(y.Triples.Count);
        }
    }
}
