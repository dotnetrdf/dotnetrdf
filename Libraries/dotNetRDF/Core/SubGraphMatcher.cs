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
    /// Implements a Sub-Graph Isomorphism Algorithm
    /// </summary>
    class SubGraphMatcher 
    {
        // The Unbound and Bound lists refers to the Nodes of the Target Graph
        private List<INode> _unbound;
        private List<INode> _bound;
        private Dictionary<INode, INode> _mapping;
        private List<Triple> _subTriples;
        private List<Triple> _parentTriples;

        /// <summary>
        /// Checks to see whether a given Graph is a sub-graph of the other Graph
        /// </summary>
        /// <param name="subgraph">Sub-Graph</param>
        /// <param name="parent">Graph</param>
        /// <returns></returns>
        public bool IsSubGraph(IGraph subgraph, IGraph parent)
        {
            // Graphs can't be equal to null
            if (subgraph == null) return false;
            if (parent == null) return false;

            // If we're the same Graph (by reference) then we're trivially sub-graphs since we're equal
            if (ReferenceEquals(subgraph, parent)) return true;

            // If sub-graph has more Triples can't be a sub-graph
            if (subgraph.Triples.Count > parent.Triples.Count) return false;

            int gtCount = 0;
            Dictionary<INode, int> subNodes = new Dictionary<INode, int>();
            Dictionary<INode, int> parentNodes = new Dictionary<INode, int>();
            _parentTriples = parent.Triples.ToList();
            foreach (Triple t in subgraph.Triples)
            {
                if (t.IsGroundTriple)
                {
                    // If the other Graph doesn't contain the Ground Triple can't be a sub-graph
                    // We make the contains call first as that is typically O(1) while the Remove call may be O(n)
                    if (!parent.Triples.Contains(t)) return false;
                    if (!_parentTriples.Remove(t)) return false;
                    gtCount++;
                }
                else
                {
                    // If not a Ground Triple remember which Blank Nodes we need to map
                    if (t.Subject.NodeType == NodeType.Blank)
                    {
                        if (!subNodes.ContainsKey(t.Subject))
                        {
                            subNodes.Add(t.Subject, 1);
                        }
                        else
                        {
                            subNodes[t.Subject]++;
                        }
                    }
                    if (t.Predicate.NodeType == NodeType.Blank)
                    {
                        if (!subNodes.ContainsKey(t.Predicate))
                        {
                            subNodes.Add(t.Predicate, 1);
                        }
                        else
                        {
                            subNodes[t.Predicate]++;
                        }
                    }
                    if (t.Object.NodeType == NodeType.Blank)
                    {
                        if (!subNodes.ContainsKey(t.Object))
                        {
                            subNodes.Add(t.Object, 1);
                        }
                        else
                        {
                            subNodes[t.Object]++;
                        }
                    }
                }
            }

            // Eliminate any remaining Ground Triples from the parent Graph since these aren't relavant to whether we're a sub-graph
            _parentTriples.RemoveAll(t => t.IsGroundTriple);

            // If there are no Triples left in the parent Graph, all our Triples were Ground Triples and there are no Blank Nodes to map then we're a sub-graph
            if (_parentTriples.Count == 0 && gtCount == subgraph.Triples.Count && subNodes.Count == 0) return true;

            // Now classify the remaining Triples from the parent Graph
            foreach (Triple t in _parentTriples)
            {
                if (t.Subject.NodeType == NodeType.Blank)
                {
                    if (!parentNodes.ContainsKey(t.Subject))
                    {
                        parentNodes.Add(t.Subject, 1);
                    }
                    else
                    {
                        parentNodes[t.Subject]++;
                    }
                }
                if (t.Predicate.NodeType == NodeType.Blank)
                {
                    if (!parentNodes.ContainsKey(t.Predicate))
                    {
                        parentNodes.Add(t.Predicate, 1);
                    }
                    else
                    {
                        parentNodes[t.Predicate]++;
                    }
                }
                if (t.Object.NodeType == NodeType.Blank)
                {
                    if (!parentNodes.ContainsKey(t.Object))
                    {
                        parentNodes.Add(t.Object, 1);
                    }
                    else
                    {
                        parentNodes[t.Object]++;
                    }
                }
            }

            // First off we must have the no more Blank Nodes in the sub-graph than in the parent graph
            if (subNodes.Count > parentNodes.Count) return false;

            // Then we sub-classify by the number of Blank Nodes with each degree classification
            Dictionary<int, int> subDegrees = new Dictionary<int, int>();
            Dictionary<int, int> parentDegrees = new Dictionary<int, int>();
            foreach (int degree in subNodes.Values)
            {
                if (subDegrees.ContainsKey(degree))
                {
                    subDegrees[degree]++;
                }
                else
                {
                    subDegrees.Add(degree, 1);
                }
            }
            foreach (int degree in parentNodes.Values)
            {
                if (parentDegrees.ContainsKey(degree))
                {
                    parentDegrees[degree]++;
                }
                else
                {
                    parentDegrees.Add(degree, 1);
                }
            }

            // Then we must have no more Degrees in the sub-graph than in the parent graph
            if (subDegrees.Count > parentDegrees.Count) return false;

            // While we'll use the degree classifications in the next step we can't do an explicit check of the
            // two things like we do for the GraphMatcher as the degrees of BNodes may be very different in the
            // sub-graph than the parent graph depending on the section of the graph it represents (assuming it
            // is a sub-graph at all!)

            // Try to do a Rules Based Mapping
            return TryRulesBasedMapping(subgraph, parent, subNodes, parentNodes, subDegrees, parentDegrees);
        }

        /// <summary>
        /// Uses a series of Rules to attempt to generate a mapping without the need for brute force guessing
        /// </summary>
        /// <param name="subgraph">1st Graph</param>
        /// <param name="parent">2nd Graph</param>
        /// <param name="subNodes">1st Graph Node classification</param>
        /// <param name="parentNodes">2nd Graph Node classification</param>
        /// <param name="subDegrees">1st Graph Degree classification</param>
        /// <param name="parentDegrees">2nd Graph Degree classification</param>
        /// <returns></returns>
        private bool TryRulesBasedMapping(IGraph subgraph, IGraph parent, Dictionary<INode, int> subNodes, Dictionary<INode, int> parentNodes, Dictionary<int, int> subDegrees, Dictionary<int, int> parentDegrees)
        {
            // Start with new lists and dictionaries each time in case we get reused
            _unbound = new List<INode>();
            _bound = new List<INode>();
            _mapping = new Dictionary<INode, INode>();
            _subTriples = new List<Triple>();

            // Initialise the Source Triples list
            _subTriples.AddRange(from t in subgraph.Triples
                                         where !t.IsGroundTriple
                                         select t);

            // First thing consider the trivial mapping
            Dictionary<INode, INode> trivialMapping = new Dictionary<INode, INode>();
            foreach (INode n in subNodes.Keys)
            {
                trivialMapping.Add(n, n);
            }
            List<Triple> targets = new List<Triple>(_parentTriples);
            if (_subTriples.All(t => targets.Remove(t.MapTriple(parent, trivialMapping))))
            {
                _mapping = trivialMapping;
                return true;
            }

            // Initialise the Unbound list
            _unbound = (from n in parentNodes.Keys
                             select n).ToList();

            // Map single use Nodes first to reduce the size of the overall mapping
            foreach (KeyValuePair<INode, int> pair in subNodes.Where(p => p.Value == 1))
            {
                // Find the Triple we need to map
                Triple toMap = (from t in _subTriples
                                where t.Involves(pair.Key)
                                select t).First();

                foreach (INode n in _unbound.Where(n => parentNodes[n] == pair.Value))
                {
                    // See if this Mapping works
                    _mapping.Add(pair.Key, n);
                    if (_parentTriples.Remove(toMap.MapTriple(parent, _mapping)))
                    {
                        _subTriples.Remove(toMap);
                        _bound.Add(n);
                        break;
                    }
                    _mapping.Remove(pair.Key);
                }

                // If we couldn't map a single use Node then it's not a sub-graph
                if (!_mapping.ContainsKey(pair.Key))
                {
                    return false;
                }
                else
                {
                    // Otherwise we can mark that Node as Bound
                    _unbound.Remove(_mapping[pair.Key]);
                }
            }

            // If all the Nodes were used only once and we mapped them all then it's a sub-graph
            if (_parentTriples.Count == 0) return true;

            // Map any Nodes of unique degree next
            foreach (KeyValuePair<int, int> degreeClass in subDegrees)
            {
                if (degreeClass.Key > 1 && degreeClass.Value == 1)
                {
                    // There is a Node of degree greater than 1 than has a unique degree
                    // i.e. there is only one Node with this degree so there can only ever be one
                    // possible mapping for this Node
                    INode x = subNodes.FirstOrDefault(p => p.Value == degreeClass.Key).Key;
                    INode y = parentNodes.FirstOrDefault(p => p.Value == degreeClass.Key).Key;

                    // If either of these return null then the Graphs can't be equal
                    if (x == null || y == null) return false;

                    // Add the Mapping
                    _mapping.Add(x, y);
                    _bound.Add(y);
                    _unbound.Remove(y);
                }
            }

            // Work out which Nodes are paired up 
            // By this we mean any Nodes which appear with other Nodes in a Triple
            // If multiple nodes appear together we can use this information to restrict
            // the possible mappings we generate
            List<MappingPair> subDependencies = new List<MappingPair>();
            foreach (Triple t in _subTriples)
            {
                if (t.Subject.NodeType == NodeType.Blank && t.Predicate.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank)
                {
                    throw new RdfException("GraphMatcher cannot compute mappings where a Triple is entirely composed of Blank Nodes");
                }
                if (t.Subject.NodeType == NodeType.Blank && t.Predicate.NodeType == NodeType.Blank)
                {
                    if (!t.Subject.Equals(t.Predicate)) subDependencies.Add(new MappingPair(t.Subject, t.Predicate, TripleIndexType.SubjectPredicate));
                }
                else if (t.Subject.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank)
                {
                    if (!t.Subject.Equals(t.Object)) subDependencies.Add(new MappingPair(t.Subject, t.Object, TripleIndexType.SubjectObject));
                }
                else if (t.Predicate.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank)
                {
                    if (!t.Predicate.Equals(t.Object)) subDependencies.Add(new MappingPair(t.Predicate, t.Object, TripleIndexType.PredicateObject));
                }
            }
            subDependencies = subDependencies.Distinct().ToList();
            List<MappingPair> parentDependencies = new List<MappingPair>();
            foreach (Triple t in _parentTriples)
            {
                if (t.Subject.NodeType == NodeType.Blank && t.Predicate.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank)
                {
                    throw new RdfException("GraphMatcher cannot compute mappings where a Triple is entirely composed of Blank Nodes");
                }
                if (t.Subject.NodeType == NodeType.Blank && t.Predicate.NodeType == NodeType.Blank)
                {
                    if (!t.Subject.Equals(t.Predicate)) parentDependencies.Add(new MappingPair(t.Subject, t.Predicate, TripleIndexType.SubjectPredicate));
                }
                else if (t.Subject.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank)
                {
                    if (!t.Subject.Equals(t.Object)) parentDependencies.Add(new MappingPair(t.Subject, t.Object, TripleIndexType.SubjectObject));
                }
                else if (t.Predicate.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank)
                {
                    if (!t.Predicate.Equals(t.Object)) parentDependencies.Add(new MappingPair(t.Predicate, t.Object, TripleIndexType.PredicateObject));
                }
            }
            parentDependencies = parentDependencies.Distinct().ToList();

            // Once we know of dependencies we can then map independent nodes
            List<INode> subIndependents = (from n in subNodes.Keys
                                              where !subDependencies.Any(p => p.Contains(n))
                                              select n).ToList();
            List<INode> parentIndependents = (from n in parentNodes.Keys
                                              where !parentDependencies.Any(p => p.Contains(n))
                                              select n).ToList();

            // If the number of independent nodes in the sub-graph is greater than it cannot be a sub-graph
            if (subIndependents.Count > parentIndependents.Count) return false;

            // Try to map the independent nodes
            foreach (INode x in subIndependents)
            {
                // They may already be mapped as they may be single use Triples
                if (_mapping.ContainsKey(x)) continue;

                List<Triple> xs = _subTriples.Where(t => t.Involves(x)).ToList();
                foreach (INode y in parentIndependents)
                {
                    if (subNodes[x] != parentNodes[y]) continue;

                    _mapping.Add(x, y);

                    // Test the mapping
                    List<Triple> ys = _parentTriples.Where(t => t.Involves(y)).ToList();
                    if (xs.All(t => ys.Remove(t.MapTriple(parent, _mapping))))
                    {
                        // This is a valid mapping
                        xs.ForEach(t => _parentTriples.Remove(t.MapTriple(parent, _mapping)));
                        xs.ForEach(t => _subTriples.Remove(t));
                        break;
                    }
                    _mapping.Remove(x);
                }

                // If we couldn't map an independent Node then we fail
                if (!_mapping.ContainsKey(x)) return false;
            }

            // Want to save our mapping so far here as if the mapping we produce using the dependency information
            // is flawed then we'll have to attempt brute force
            Dictionary<INode, INode> baseMapping = new Dictionary<INode, INode>(_mapping);

            // Now we use the dependency information to try and find mappings
            foreach (MappingPair dependency in subDependencies)
            {
                // If both dependent Nodes are already mapped we don't need to try mapping them again
                if (_mapping.ContainsKey(dependency.X) && _mapping.ContainsKey(dependency.Y)) continue;

                // Get all the Triples with this Pair in them
                List<Triple> xs;
                bool canonical = false;
                switch (dependency.Type)
                {
                    case TripleIndexType.SubjectPredicate:
                        xs = (from t in _subTriples
                              where t.Subject.Equals(dependency.X) && t.Predicate.Equals(dependency.Y)
                              select t).ToList();
                        if (xs.Count == 1)
                        {
                            canonical = ((from t in _subTriples
                                          where t.Subject.NodeType == NodeType.Blank && t.Predicate.NodeType == NodeType.Blank && t.Object.Equals(xs[0].Object)
                                          select t).Count() == 1);
                        }
                        break;
                    case TripleIndexType.SubjectObject:
                        xs = (from t in _subTriples
                              where t.Subject.Equals(dependency.X) && t.Object.Equals(dependency.Y)
                              select t).ToList();
                        if (xs.Count == 1)
                        {
                            canonical = ((from t in _subTriples
                                          where t.Subject.NodeType == NodeType.Blank && t.Predicate.Equals(xs[0].Predicate) && t.Object.NodeType == NodeType.Blank
                                          select t).Count() == 1);
                        }
                        break;
                    case TripleIndexType.PredicateObject:
                        xs = (from t in _subTriples
                              where t.Predicate.Equals(dependency.X) && t.Object.Equals(dependency.Y)
                              select t).ToList();
                        if (xs.Count == 1)
                        {
                            canonical = ((from t in _subTriples
                                          where t.Subject.Equals(xs[0].Subject) && t.Predicate.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank
                                          select t).Count() == 1);
                        }
                        break;
                    default:
                        // This means we've gone wrong somehow
                        throw new RdfException("Unknown exception occurred while trying to generate a Mapping between the two Graphs");
                }

                bool xbound = _mapping.ContainsKey(dependency.X);
                bool ybound = _mapping.ContainsKey(dependency.Y);

                // Look at all the possible Target Dependencies we could map to
                foreach (MappingPair target in parentDependencies)
                {
                    if (target.Type != dependency.Type) continue;

                    // If one of the Nodes we're trying to map was already mapped then we can further restrict
                    // candidate mappings
                    if (xbound)
                    {
                        if (!target.X.Equals(_mapping[dependency.X])) continue;
                    }
                    if (ybound)
                    {
                        if (!target.Y.Equals(_mapping[dependency.Y])) continue;
                    }

                    // If the Nodes in the Target have already been used then we can discard this possible mapping
                    if (!xbound && _mapping.ContainsValue(target.X)) continue;
                    if (!ybound && _mapping.ContainsValue(target.Y)) continue;

                    // Get the Triples with the Target Pair in them
                    List<Triple> ys;
                    switch (target.Type)
                    {
                        case TripleIndexType.SubjectPredicate:
                            ys = (from t in _parentTriples
                                  where t.Subject.Equals(target.X) && t.Predicate.Equals(target.Y)
                                  select t).ToList();
                            break;
                        case TripleIndexType.SubjectObject:
                            ys = (from t in _parentTriples
                                  where t.Subject.Equals(target.X) && t.Object.Equals(target.Y)
                                  select t).ToList();
                            break;
                        case TripleIndexType.PredicateObject:
                            ys = (from t in _parentTriples
                                  where t.Predicate.Equals(target.X) && t.Object.Equals(target.Y)
                                  select t).ToList();
                            break;
                        default:
                            // This means we've gone wrong somehow
                            throw new RdfException("Unknown exception occurred while trying to generate a Mapping between the two Graphs");
                    }

                    // If the pairs are involved in a greater number of Triples in the sub-graph it cannot be a valid mapping
                    if (xs.Count > ys.Count) continue;

                    // If all the Triples in xs can be removed from ys then this is a valid mapping
                    if (!xbound) _mapping.Add(dependency.X, target.X);
                    if (!ybound) _mapping.Add(dependency.Y, target.Y);
                    if (xs.All(t => ys.Remove(t.MapTriple(parent, _mapping))))
                    {
                        if (canonical)
                        {
                            // If this was the only possible mapping for this Pair then this is a canonical mapping
                            // and can go in the base mapping so if we have to brute force we have fewer possibles
                            // to try
                            if (!baseMapping.ContainsKey(dependency.X)) baseMapping.Add(dependency.X, target.X);
                            if (!baseMapping.ContainsKey(dependency.Y)) baseMapping.Add(dependency.Y, target.Y);
                        }
                        _bound.Add(target.X);
                        _bound.Add(target.Y);
                        _unbound.Remove(target.X);
                        _unbound.Remove(target.Y);
                        break;
                    }
                    else
                    {
                        if (!xbound) _mapping.Remove(dependency.X);
                        if (!ybound) _mapping.Remove(dependency.Y);
                    }
                }
            }

            // If we've filled in the Mapping fully then the Graph is hopefully a sub-graph of the parent graph
            if (_mapping.Count == subNodes.Count)
            {
                // Need to check we found a valid mapping
                List<Triple> ys = new List<Triple>(_parentTriples);
                if (_subTriples.All(t => ys.Remove(t.MapTriple(parent, _mapping))))
                {
                    return true;
                }
                else
                {
                    _mapping = baseMapping;
                    return TryBruteForceMapping(subgraph, parent, subNodes, parentNodes, subDependencies, parentDependencies);
                }
            }
            else
            {
                return TryBruteForceMapping(subgraph, parent, subNodes, parentNodes, subDependencies, parentDependencies);
            }
        }

        /// <summary>
        /// Generates and Tests all possibilities in a brute force manner
        /// </summary>
        /// <param name="subgraph">1st Graph</param>
        /// <param name="parent">2nd Graph</param>
        /// <param name="subNodes">1st Graph Node classification</param>
        /// <param name="parentNodes">2nd Graph Node classification</param>
        /// <param name="subDependencies">Dependencies in the 1st Graph</param>
        /// <param name="parentDependencies">Dependencies in the 2nd Graph</param>
        /// <returns></returns>
        private bool TryBruteForceMapping(IGraph subgraph, IGraph parent, Dictionary<INode, int> subNodes, Dictionary<INode, int> parentNodes, List<MappingPair> subDependencies, List<MappingPair> parentDependencies)
        {
            Dictionary<INode, List<INode>> possibleMappings = new Dictionary<INode, List<INode>>();

            // Populate existing Mappings
            foreach (KeyValuePair<INode,INode> fixedMapping in _mapping) 
            {
                possibleMappings.Add(fixedMapping.Key, new List<INode>(fixedMapping.Value.AsEnumerable<INode>()));
            }

            // Populate possibilities for each Node
            foreach (KeyValuePair<INode, int> gPair in subNodes)
            {
                if (!_mapping.ContainsKey(gPair.Key))
                {
                    possibleMappings.Add(gPair.Key, new List<INode>());
                    foreach (KeyValuePair<INode, int> hPair in parentNodes.Where(p => p.Value == gPair.Value && !_bound.Contains(p.Key)))
                    {
                        possibleMappings[gPair.Key].Add(hPair.Key);
                    }

                    // If there's no possible matches for the Node we fail
                    if (possibleMappings[gPair.Key].Count == 0) return false;
                }
            }

            // Now start testing the possiblities
            List<Dictionary<INode, INode>> possibles = GenerateMappings(possibleMappings, subDependencies, parentDependencies, parent);

            foreach (Dictionary<INode, INode> mapping in possibles)
            {
                List<Triple> targets = new List<Triple>(_parentTriples);
                if (_subTriples.All(t => targets.Remove(t.MapTriple(parent, mapping))))
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
        /// <param name="subDependencies">Dependencies in the 1st Graph</param>
        /// <param name="parentDependencies">Dependencies in the 2nd Graph</param>
        /// <param name="target">Target Graph (2nd Graph)</param>
        /// <returns></returns>
        private List<Dictionary<INode, INode>> GenerateMappings(Dictionary<INode, List<INode>> possibleMappings, List<MappingPair> subDependencies, List<MappingPair> parentDependencies, IGraph target)
        {
            List<Dictionary<INode, INode>> mappings = new List<Dictionary<INode, INode>>();

            mappings.Add(new Dictionary<INode, INode>());
            foreach (INode x in possibleMappings.Keys)
            {
                if (possibleMappings[x].Count == 1)
                {
                    // Only one possible for this Node
                    // This means we can just add this to the dictionaries and continue
                    mappings.ForEach(m => m.Add(x, possibleMappings[x].First()));
                }
                else
                {
                    // Multiple possibilities each of which generates a potential mapping
                    List<Dictionary<INode, INode>> temp = new List<Dictionary<INode, INode>>();

                    // Need to know whether there are any dependencies we can use to limit possible mappings
                    bool dependent = subDependencies.Any(p => p.Contains(x));

                    foreach (INode y in possibleMappings[x])
                    {
                        foreach (Dictionary<INode, INode> m in mappings)
                        {
                            if (m.ContainsValue(y)) continue;
                            Dictionary<INode, INode> n = new Dictionary<INode, INode>(m);
                            n.Add(x, y);
                            if (dependent)
                            {
                                foreach (MappingPair dependency in subDependencies)
                                {
                                    if (n.ContainsKey(dependency.X) && n.ContainsKey(dependency.Y))
                                    {
                                        MappingPair targetDependency = new MappingPair(n[dependency.X], n[dependency.Y], dependency.Type);
                                        if (!parentDependencies.Contains(targetDependency))
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

                // List of Triples for doing partial mapping Tests
                foreach (INode test in possibleMappings.Keys)
                {
                    List<Triple> xs = (from t in _subTriples
                                       where t.Involves(test)
                                       select t).ToList();

                    foreach (Dictionary<INode, INode> m in mappings)
                    {
                        // Are all the Blank Nodes involved in these Triples mapped at this stage?
                        if (xs.All(t => t.Nodes.All(node => node.NodeType != NodeType.Blank || m.ContainsKey(node))))
                        {
                            // Then we can do a partial mapping test
                            IEnumerable<Triple> ys = (from t in xs
                                                      where _parentTriples.Contains(t.MapTriple(target, m))
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
        /// Gets the Blank Node mapping if one could be found
        /// </summary>
        public Dictionary<INode, INode> Mapping
        {
            get
            {
                return _mapping;
            }
        }
    }
}
