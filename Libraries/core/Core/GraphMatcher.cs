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
    /// Implements a Graph Isomorphism Algorithm
    /// </summary>
    class GraphMatcher 
    {
        //The Unbound and Bound lists refers to the Nodes of the Target Graph
        private List<INode> _unbound;
        private List<INode> _bound;
        private Dictionary<INode, INode> _mapping;
        private List<Triple> _sourceTriples;
        private List<Triple> _targetTriples;

        /// <summary>
        /// Compares two Graphs for equality
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="h">Graph</param>
        /// <returns></returns>
        public bool Equals(IGraph g, IGraph h)
        {
            //Graphs can't be equal to null
            if (g == null) return false;
            if (h == null) return false;

            //If we're the same Graph (by reference) then we're trivially equal
            if (ReferenceEquals(g, h)) return true;

            //If different number of Triples then the Graphs can't be equal
            if (g.Triples.Count != h.Triples.Count) return false;

            int gtCount = 0;
            Dictionary<INode, int> gNodes = new Dictionary<INode, int>();
            Dictionary<INode, int> hNodes = new Dictionary<INode, int>();
            this._targetTriples = h.Triples.ToList();
            foreach (Triple t in g.Triples)
            {
                if (t.IsGroundTriple)
                {
                    //If the other Graph doesn't contain the Ground Triple so can't be equal
                    //We make the contains call first as that is typically O(1) while the Remove call may be O(n)
                    if (!h.Triples.Contains(t)) return false;
                    if (!this._targetTriples.Remove(t)) return false;
                    gtCount++;
                }
                else
                {
                    //If not a Ground Triple remember which Blank Nodes we need to map
                    if (t.Subject.NodeType == NodeType.Blank)
                    {
                        if (!gNodes.ContainsKey(t.Subject))
                        {
                            gNodes.Add(t.Subject, 1);
                        }
                        else
                        {
                            gNodes[t.Subject]++;
                        }
                    }
                    if (t.Predicate.NodeType == NodeType.Blank)
                    {
                        if (!gNodes.ContainsKey(t.Predicate))
                        {
                            gNodes.Add(t.Predicate, 1);
                        }
                        else
                        {
                            gNodes[t.Predicate]++;
                        }
                    }
                    if (t.Object.NodeType == NodeType.Blank)
                    {
                        if (!gNodes.ContainsKey(t.Object))
                        {
                            gNodes.Add(t.Object, 1);
                        }
                        else
                        {
                            gNodes[t.Object]++;
                        }
                    }
                }
            }

            //If the other Graph still contains Ground Triples then the Graphs aren't equal
            if (this._targetTriples.Any(t => t.IsGroundTriple)) return false;

            //If there are no Triples left in the other Graph, all our Triples were Ground Triples and there are no Blank Nodes to map the Graphs are equal
            if (this._targetTriples.Count == 0 && gtCount == g.Triples.Count && gNodes.Count == 0) return true;

            //Now classify the remaining Triples from the other Graph
            foreach (Triple t in this._targetTriples)
            {
                if (t.Subject.NodeType == NodeType.Blank)
                {
                    if (!hNodes.ContainsKey(t.Subject))
                    {
                        hNodes.Add(t.Subject, 1);
                    }
                    else
                    {
                        hNodes[t.Subject]++;
                    }
                }
                if (t.Predicate.NodeType == NodeType.Blank)
                {
                    if (!hNodes.ContainsKey(t.Predicate))
                    {
                        hNodes.Add(t.Predicate, 1);
                    }
                    else
                    {
                        hNodes[t.Predicate]++;
                    }
                }
                if (t.Object.NodeType == NodeType.Blank)
                {
                    if (!hNodes.ContainsKey(t.Object))
                    {
                        hNodes.Add(t.Object, 1);
                    }
                    else
                    {
                        hNodes[t.Object]++;
                    }
                }
            }

            //First off we must have the same number of Blank Nodes in each Graph
            if (gNodes.Count != hNodes.Count) return false;

            //Then we sub-classify by the number of Blank Nodes with each degree classification
            Dictionary<int, int> gDegrees = new Dictionary<int, int>();
            Dictionary<int, int> hDegrees = new Dictionary<int, int>();
            foreach (int degree in gNodes.Values)
            {
                if (gDegrees.ContainsKey(degree))
                {
                    gDegrees[degree]++;
                }
                else
                {
                    gDegrees.Add(degree, 1);
                }
            }
            foreach (int degree in hNodes.Values)
            {
                if (hDegrees.ContainsKey(degree))
                {
                    hDegrees[degree]++;
                }
                else
                {
                    hDegrees.Add(degree, 1);
                }
            }

            //Then we must have the same number of degree classifications
            if (gDegrees.Count != hDegrees.Count) return false;

            //Then for each degree classification there must be the same number of BNodes with that degree in both Graph
            if (gDegrees.All(pair => hDegrees.ContainsKey(pair.Key) && gDegrees[pair.Key] == hDegrees[pair.Key]))
            {
                //Try to do a Rules Based Mapping
                return this.TryRulesBasedMapping(g, h, gNodes, hNodes, gDegrees, hDegrees);
            }
            else
            {
                //There are degree classifications that don't have the same number of BNodes with that degree 
                //or the degree classification is not in the other Graph
                return false;
            }

        }

        /// <summary>
        /// Uses a series of Rules to attempt to generate a mapping without the need for brute force guessing
        /// </summary>
        /// <param name="g">1st Graph</param>
        /// <param name="h">2nd Graph</param>
        /// <param name="gNodes">1st Graph Node classification</param>
        /// <param name="hNodes">2nd Graph Node classification</param>
        /// <param name="gDegrees">1st Graph Degree classification</param>
        /// <param name="hDegrees">2nd Graph Degree classification</param>
        /// <returns></returns>
        private bool TryRulesBasedMapping(IGraph g, IGraph h, Dictionary<INode, int> gNodes, Dictionary<INode, int> hNodes, Dictionary<int, int> gDegrees, Dictionary<int, int> hDegrees)
        {
            //Start with new lists and dictionaries each time in case we get reused
            this._unbound = new List<INode>();
            this._bound = new List<INode>();
            this._mapping = new Dictionary<INode, INode>();
            this._sourceTriples = new List<Triple>();

            //Initialise the Source Triples list
            this._sourceTriples.AddRange(from t in g.Triples
                                         where !t.IsGroundTriple
                                         select t);

            //First thing consider the trivial mapping
            Dictionary<INode, INode> trivialMapping = new Dictionary<INode, INode>();
            foreach (INode n in gNodes.Keys)
            {
                trivialMapping.Add(n, n);
            }
            List<Triple> targets = new List<Triple>(this._targetTriples);
            if (this._sourceTriples.All(t => targets.Remove(t.MapTriple(h, trivialMapping))))
            {
                this._mapping = trivialMapping;
                return true;
            }

            //Initialise the Unbound list
            this._unbound = (from n in hNodes.Keys
                             select n).ToList();

            //Map single use Nodes first to reduce the size of the overall mapping
            foreach (KeyValuePair<INode, int> pair in gNodes.Where(p => p.Value == 1))
            {
                //Find the Triple we need to map
                Triple toMap = (from t in this._sourceTriples
                                where t.Involves(pair.Key)
                                select t).First();

                foreach (INode n in this._unbound.Where(n => hNodes[n] == pair.Value))
                {
                    //See if this Mapping works
                    this._mapping.Add(pair.Key, n);
                    if (this._targetTriples.Remove(toMap.MapTriple(h, this._mapping)))
                    {
                        this._sourceTriples.Remove(toMap);
                        this._bound.Add(n);
                        break;
                    }
                    this._mapping.Remove(pair.Key);
                }

                //If we couldn't map a single use Node then the Graphs are not equal
                if (!this._mapping.ContainsKey(pair.Key))
                {
                    return false;
                }
                else
                {
                    //Otherwise we can mark that Node as Bound
                    this._unbound.Remove(this._mapping[pair.Key]);
                }
            }

            //If all the Nodes were used only once and we mapped them all then the Graphs are equal
            if (this._targetTriples.Count == 0) return true;

            //Map any Nodes of unique degree next
            foreach (KeyValuePair<int, int> degreeClass in gDegrees)
            {
                if (degreeClass.Key > 1 && degreeClass.Value == 1)
                {
                    //There is a Node of degree greater than 1 than has a unique degree
                    //i.e. there is only one Node with this degree so there can only ever be one
                    //possible mapping for this Node
                    INode x = gNodes.FirstOrDefault(p => p.Value == degreeClass.Key).Key;
                    INode y = hNodes.FirstOrDefault(p => p.Value == degreeClass.Key).Key;

                    //If either of these return null then the Graphs can't be equal
                    if (x == null || y == null) return false;

                    //Add the Mapping
                    this._mapping.Add(x, y);
                    this._bound.Add(y);
                    this._unbound.Remove(y);
                }
            }

            //Work out which Nodes are paired up 
            //By this we mean any Nodes which appear with other Nodes in a Triple
            //If multiple nodes appear together we can use this information to restrict
            //the possible mappings we generate
            List<MappingPair> sourceDependencies = new List<MappingPair>();
            foreach (Triple t in this._sourceTriples)
            {
                if (t.Subject.NodeType == NodeType.Blank && t.Predicate.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank)
                {
                    throw new RdfException("GraphMatcher cannot compute mappings where a Triple is entirely composed of Blank Nodes");
                }
                if (t.Subject.NodeType == NodeType.Blank && t.Predicate.NodeType == NodeType.Blank)
                {
                    if (!t.Subject.Equals(t.Predicate)) sourceDependencies.Add(new MappingPair(t.Subject, t.Predicate, TripleIndexType.SubjectPredicate));
                }
                else if (t.Subject.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank)
                {
                    if (!t.Subject.Equals(t.Object)) sourceDependencies.Add(new MappingPair(t.Subject, t.Object, TripleIndexType.SubjectObject));
                }
                else if (t.Predicate.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank)
                {
                    if (!t.Predicate.Equals(t.Object)) sourceDependencies.Add(new MappingPair(t.Predicate, t.Object, TripleIndexType.PredicateObject));
                }
            }
            sourceDependencies = sourceDependencies.Distinct().ToList();
            List<MappingPair> targetDependencies = new List<MappingPair>();
            foreach (Triple t in this._targetTriples)
            {
                if (t.Subject.NodeType == NodeType.Blank && t.Predicate.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank)
                {
                    throw new RdfException("GraphMatcher cannot compute mappings where a Triple is entirely composed of Blank Nodes");
                }
                if (t.Subject.NodeType == NodeType.Blank && t.Predicate.NodeType == NodeType.Blank)
                {
                    if (!t.Subject.Equals(t.Predicate)) targetDependencies.Add(new MappingPair(t.Subject, t.Predicate, TripleIndexType.SubjectPredicate));
                }
                else if (t.Subject.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank)
                {
                    if (!t.Subject.Equals(t.Object)) targetDependencies.Add(new MappingPair(t.Subject, t.Object, TripleIndexType.SubjectObject));
                }
                else if (t.Predicate.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank)
                {
                    if (!t.Predicate.Equals(t.Object)) targetDependencies.Add(new MappingPair(t.Predicate, t.Object, TripleIndexType.PredicateObject));
                }
            }
            targetDependencies = targetDependencies.Distinct().ToList();

            //Once we know of dependencies we can then map independent nodes
            List<INode> sourceIndependents = (from n in gNodes.Keys
                                              where !sourceDependencies.Any(p => p.Contains(n))
                                              select n).ToList();
            List<INode> targetIndependents = (from n in hNodes.Keys
                                              where !targetDependencies.Any(p => p.Contains(n))
                                              select n).ToList();

            //If the number of independent nodes in the two Graphs is different we return false
            if (sourceIndependents.Count != targetIndependents.Count) return false;

            //Try to map the independent nodes
            foreach (INode x in sourceIndependents)
            {
                //They may already be mapped as they may be single use Triples
                if (this._mapping.ContainsKey(x)) continue;

                List<Triple> xs = this._sourceTriples.Where(t => t.Involves(x)).ToList();
                foreach (INode y in targetIndependents)
                {
                    if (gNodes[x] != hNodes[y]) continue;

                    this._mapping.Add(x, y);

                    //Test the mapping
                    List<Triple> ys = this._targetTriples.Where(t => t.Involves(y)).ToList();
                    if (xs.All(t => ys.Remove(t.MapTriple(h, this._mapping))))
                    {
                        //This is a valid mapping
                        xs.ForEach(t => this._targetTriples.Remove(t.MapTriple(h, this._mapping)));
                        xs.ForEach(t => this._sourceTriples.Remove(t));
                        break;
                    }
                    this._mapping.Remove(x);
                }

                //If we couldn't map an independent Node then we fail
                if (!this._mapping.ContainsKey(x)) return false;
            }

            //Want to save our mapping so far here as if the mapping we produce using the dependency information
            //is flawed then we'll have to attempt brute force
            Dictionary<INode, INode> baseMapping = new Dictionary<INode, INode>(this._mapping);

            //Now we use the dependency information to try and find mappings
            foreach (MappingPair dependency in sourceDependencies)
            {
                //If both dependent Nodes are already mapped we don't need to try mapping them again
                if (this._mapping.ContainsKey(dependency.X) && this._mapping.ContainsKey(dependency.Y)) continue;

                //Get all the Triples with this Pair in them
                List<Triple> xs;
                bool canonical = false;
                switch (dependency.Type)
                {
                    case TripleIndexType.SubjectPredicate:
                        xs = (from t in this._sourceTriples
                              where t.Subject.Equals(dependency.X) && t.Predicate.Equals(dependency.Y)
                              select t).ToList();
                        if (xs.Count == 1)
                        {
                            canonical = ((from t in this._sourceTriples
                                          where t.Subject.NodeType == NodeType.Blank && t.Predicate.NodeType == NodeType.Blank && t.Object.Equals(xs[0].Object)
                                          select t).Count() == 1);
                        }
                        break;
                    case TripleIndexType.SubjectObject:
                        xs = (from t in this._sourceTriples
                              where t.Subject.Equals(dependency.X) && t.Object.Equals(dependency.Y)
                              select t).ToList();
                        if (xs.Count == 1)
                        {
                            canonical = ((from t in this._sourceTriples
                                          where t.Subject.NodeType == NodeType.Blank && t.Predicate.Equals(xs[0].Predicate) && t.Object.NodeType == NodeType.Blank
                                          select t).Count() == 1);
                        }
                        break;
                    case TripleIndexType.PredicateObject:
                        xs = (from t in this._sourceTriples
                              where t.Predicate.Equals(dependency.X) && t.Object.Equals(dependency.Y)
                              select t).ToList();
                        if (xs.Count == 1)
                        {
                            canonical = ((from t in this._sourceTriples
                                          where t.Subject.Equals(xs[0].Subject) && t.Predicate.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank
                                          select t).Count() == 1);
                        }
                        break;
                    default:
                        //This means we've gone wrong somehow
                        throw new RdfException("Unknown exception occurred while trying to generate a Mapping between the two Graphs");
                }

                bool xbound = this._mapping.ContainsKey(dependency.X);
                bool ybound = this._mapping.ContainsKey(dependency.Y);

                //Look at all the possible Target Dependencies we could map to
                foreach (MappingPair target in targetDependencies)
                {
                    if (target.Type != dependency.Type) continue;

                    //If one of the Nodes we're trying to map was already mapped then we can further restrict
                    //candidate mappings
                    if (xbound)
                    {
                        if (!target.X.Equals(this._mapping[dependency.X])) continue;
                    }
                    if (ybound)
                    {
                        if (!target.Y.Equals(this._mapping[dependency.Y])) continue;
                    }

                    //If the Nodes in the Target have already been used then we can discard this possible mapping
                    if (!xbound && this._mapping.ContainsValue(target.X)) continue;
                    if (!ybound && this._mapping.ContainsValue(target.Y)) continue;

                    //Get the Triples with the Target Pair in them
                    List<Triple> ys;
                    switch (target.Type)
                    {
                        case TripleIndexType.SubjectPredicate:
                            ys = (from t in this._targetTriples
                                  where t.Subject.Equals(target.X) && t.Predicate.Equals(target.Y)
                                  select t).ToList();
                            break;
                        case TripleIndexType.SubjectObject:
                            ys = (from t in this._targetTriples
                                  where t.Subject.Equals(target.X) && t.Object.Equals(target.Y)
                                  select t).ToList();
                            break;
                        case TripleIndexType.PredicateObject:
                            ys = (from t in this._targetTriples
                                  where t.Predicate.Equals(target.X) && t.Object.Equals(target.Y)
                                  select t).ToList();
                            break;
                        default:
                            //This means we've gone wrong somehow
                            throw new RdfException("Unknown exception occurred while trying to generate a Mapping between the two Graphs");
                    }

                    //If the pairs are involved in different numbers of Triples then they aren't possible mappings
                    if (xs.Count != ys.Count) continue;

                    //If all the Triples in xs can be removed from ys then this is a valid mapping
                    if (!xbound) this._mapping.Add(dependency.X, target.X);
                    if (!ybound) this._mapping.Add(dependency.Y, target.Y);
                    if (xs.All(t => ys.Remove(t.MapTriple(h, this._mapping))))
                    {
                        if (canonical)
                        {
                            //If this was the only possible mapping for this Pair then this is a canonical mapping
                            //and can go in the base mapping so if we have to brute force we have fewer possibles
                            //to try
                            if (!baseMapping.ContainsKey(dependency.X)) baseMapping.Add(dependency.X, target.X);
                            if (!baseMapping.ContainsKey(dependency.Y)) baseMapping.Add(dependency.Y, target.Y);
                        }
                        this._bound.Add(target.X);
                        this._bound.Add(target.Y);
                        this._unbound.Remove(target.X);
                        this._unbound.Remove(target.Y);
                        break;
                    }
                    else
                    {
                        if (!xbound) this._mapping.Remove(dependency.X);
                        if (!ybound) this._mapping.Remove(dependency.Y);
                    }
                }
            }

            //If we've filled in the Mapping fully then the Graphs are hopefully equal
            if (this._mapping.Count == gNodes.Count)
            {
                //Need to check we found a valid mapping
                List<Triple> ys = new List<Triple>(this._targetTriples);
                if (this._sourceTriples.All(t => ys.Remove(t.MapTriple(h, this._mapping))))
                {
                    return true;
                }
                else
                {
                    this._mapping = baseMapping;
                    return this.TryBruteForceMapping(g, h, gNodes, hNodes, sourceDependencies, targetDependencies);
                }
            }
            else
            {
                return this.TryBruteForceMapping(g, h, gNodes, hNodes, sourceDependencies, targetDependencies);
            }
        }

        /// <summary>
        /// Generates and Tests all possibilities in a brute force manner
        /// </summary>
        /// <param name="g">1st Graph</param>
        /// <param name="h">2nd Graph</param>
        /// <param name="gNodes">1st Graph Node classification</param>
        /// <param name="hNodes">2nd Graph Node classification</param>
        /// <param name="sourceDependencies">Dependencies in the 1st Graph</param>
        /// <param name="targetDependencies">Dependencies in the 2nd Graph</param>
        /// <returns></returns>
        private bool TryBruteForceMapping(IGraph g, IGraph h, Dictionary<INode, int> gNodes, Dictionary<INode, int> hNodes, List<MappingPair> sourceDependencies, List<MappingPair> targetDependencies)
        {
            Dictionary<INode, List<INode>> possibleMappings = new Dictionary<INode, List<INode>>();

            //Populate existing Mappings
            foreach (KeyValuePair<INode,INode> fixedMapping in this._mapping) 
            {
                possibleMappings.Add(fixedMapping.Key, new List<INode>(fixedMapping.Value.AsEnumerable<INode>()));
            }

            //Populate possibilities for each Node
            foreach (KeyValuePair<INode, int> gPair in gNodes)
            {
                if (!this._mapping.ContainsKey(gPair.Key))
                {
                    possibleMappings.Add(gPair.Key, new List<INode>());
                    foreach (KeyValuePair<INode, int> hPair in hNodes.Where(p => p.Value == gPair.Value && !this._bound.Contains(p.Key)))
                    {
                        possibleMappings[gPair.Key].Add(hPair.Key);
                    }

                    //If there's no possible matches for the Node we fail
                    if (possibleMappings[gPair.Key].Count == 0) return false;
                }
            }

            //Now start testing the possiblities
            List<Dictionary<INode, INode>> possibles = this.GenerateMappings(possibleMappings, sourceDependencies, targetDependencies, h);

            foreach (Dictionary<INode, INode> mapping in possibles)
            {
                List<Triple> targets = new List<Triple>(this._targetTriples);
                if (this._sourceTriples.All(t => targets.Remove(t.MapTriple(h, mapping))))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Helper method for brute forcing the possible mappings
        /// </summary>
        /// <param name="possibleMappings">Possible Mappings</param>
        /// <param name="sourceDependencies">Dependencies in the 1st Graph</param>
        /// <param name="targetDependencies">Dependencies in the 2nd Graph</param>
        /// <param name="target">Target Graph (2nd Graph)</param>
        /// <returns></returns>
        private List<Dictionary<INode, INode>> GenerateMappings(Dictionary<INode, List<INode>> possibleMappings, List<MappingPair> sourceDependencies, List<MappingPair> targetDependencies, IGraph target)
        {
            List<Dictionary<INode, INode>> mappings = new List<Dictionary<INode, INode>>();

            mappings.Add(new Dictionary<INode, INode>());
            foreach (INode x in possibleMappings.Keys)
            {
                if (possibleMappings[x].Count == 1)
                {
                    //Only one possible for this Node
                    //This means we can just add this to the dictionaries and continue
                    mappings.ForEach(m => m.Add(x, possibleMappings[x].First()));
                }
                else
                {
                    //Multiple possibilities each of which generates a potential mapping
                    List<Dictionary<INode, INode>> temp = new List<Dictionary<INode, INode>>();

                    //Need to know whether there are any dependencies we can use to limit possible mappings
                    bool dependent = sourceDependencies.Any(p => p.Contains(x));

                    foreach (INode y in possibleMappings[x])
                    {
                        foreach (Dictionary<INode, INode> m in mappings)
                        {
                            if (m.ContainsValue(y)) continue;
                            Dictionary<INode, INode> n = new Dictionary<INode, INode>(m);
                            n.Add(x, y);
                            if (dependent)
                            {
                                foreach (MappingPair dependency in sourceDependencies)
                                {
                                    if (n.ContainsKey(dependency.X) && n.ContainsKey(dependency.Y))
                                    {
                                        MappingPair targetDependency = new MappingPair(n[dependency.X], n[dependency.Y], dependency.Type);
                                        if (!targetDependencies.Contains(targetDependency))
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }
                            
                            temp.Add(n);
                        }


                    }
                    mappings.Clear();
                    mappings = temp;
                }

                //List of Triples for doing partial mapping Tests
                foreach (INode test in possibleMappings.Keys)
                {
                    List<Triple> xs = (from t in this._sourceTriples
                                       where t.Involves(test)
                                       select t).ToList();

                    foreach (Dictionary<INode, INode> m in mappings)
                    {
                        //Are all the Blank Nodes involved in these Triples mapped at this stage?
                        if (xs.All(t => t.Nodes.All(node => node.NodeType != NodeType.Blank || m.ContainsKey(node))))
                        {
                            //Then we can do a partial mapping test
                            IEnumerable<Triple> ys = (from t in xs
                                                      where this._targetTriples.Contains(t.MapTriple(target, m))
                                                      select t);

                            if (xs.Count != ys.Count())
                            {
                                continue;
                            }
                        }
                    }
                }
            }

            return mappings;
        }

        /// <summary>
        /// Gets the Blank Node Mapping found between the Graphs
        /// </summary>
        public Dictionary<INode, INode> Mapping
        {
            get
            {
                return this._mapping;
            }
        }
    }

    /// <summary>
    /// Represents a Pair of Nodes that occur in the same Triple
    /// </summary>
    class MappingPair
    {
        private INode _x, _y;
        private int _hash;
        private TripleIndexType _type;

        public MappingPair(INode x, INode y, TripleIndexType type)
        {
            this._x = x;
            this._y = y;
            this._type = type;
            this._hash = Tools.CombineHashCodes(x, y);
        }

        public INode X
        {
            get
            {
                return this._x;
            }
        }

        public INode Y
        {
            get
            {
                return this._y;
            }
        }

        public TripleIndexType Type
        {
            get
            {
                return this._type;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is MappingPair)
            {
                MappingPair p = (MappingPair)obj;
                return (this._x.Equals(p.X) && this._y.Equals(p.Y) && this._type == p.Type);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this._hash;
        }

        public bool Contains(INode n)
        {
            return (this._x.Equals(n) || this._y.Equals(n));
        }
    }
}
