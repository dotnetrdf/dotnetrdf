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
using System.Diagnostics;
using System.Linq;

namespace VDS.RDF
{
    /// <summary>
    /// Implements a Graph Isomorphism Algorithm
    /// </summary>
    /// <remarks>
    /// <para>
    /// The algorithm used to determine Graph equality is based in part on a Iterative Vertex Classification Algorithm described in a Technical Report from HP by Jeremy J Carroll - <a href="http://www.hpl.hp.com/techreports/2001/HPL-2001-293.html">Matching RDF Graphs</a> but has been expanded upon significantly to use a variety of techniques.
    /// </para>
    /// <para>
    /// Graph Equality is determined according to the following algorithm, we refer to the first graph as the <em>Source Graph</em> and the second graph as the <em>Target Graph</em>:
    /// </para>
    /// <ol>
    /// <li>If both graphs are null they are considered equal</li>
    /// <li>If only one of the given graph is null then they are not equal</li>
    /// <li>If the given graphs are reference equal then they are equal</li>
    /// <li>If the given graphs have a different number of Triples they are not equal</li>
    /// <li>Declare a list of triples which are the triples of the second graph called <em>TargetTriples</em></li>
    /// <li>Declare two dictionaries of Nodes to Integers which are called <em>SourceClassification</em> and <em>TargetClassification</em></li>
    /// <li>For Each Triple in the Source Graph
    ///     <ol>
    ///     <li>If it is a ground triple and cannot be found and removed from <em>TargetTriples</em> then graphs are not equal since the triple does not exist in both graphs</li>
    ///     <li>If it contains blank nodes track the number of usages of this blank node in <em>SourceClassification</em></li>
    ///     </ol>
    /// </li> 
    /// <li>If there are any triples remaining in <em>TargetTriples</em> which are ground triples then graphs are not equal since the Source Graph does not contain them</li>
    /// <li>If all the triples from both graphs were ground triples (i.e. there were no blank nodes) then the graphs are equal</li>
    /// <li>Iterate over the remaining triples in <em>TargetTriples</em> and populate the <em>TargetClassification</em></li>
    /// <li>If the count of the two classifications is different the graphs are not equal since there are differing numbers of blank nodes in the Graph</li>
    /// <li>Now build two additional dictionaries of Integers to Integers which are called <em>SourceDegreeClassification</em> and <em>TargetDegreeClassification</em>.  Iterate over <em>SourceClassification</em> and <em>TargetClassification</em> such that the corresponding degree classifications contain a mapping of the number of blank nodes with a given degree</li>
    /// <li>If the count of the two degree classifications is different the graphs are not equal since there are not the same range of blank node degrees in both graphs</li>
    /// <li>For All classifications in <em>SourceDegreeClassification</em> there must be a matching classification in <em>TargetDegreeClassification</em> else the graphs are not equal</li>
    /// <li>Then build a possible mapping using the following rules:
    ///     <ol>
    ///     <li>Any blank bode used only once (single-use) in the Source Graph should be mapped to an equivalent blank bode in the Target Graph.  If this is not possible then the graphs are not equal</li>
    ///     <li>Any blank node with a unique degree in the Source Graph should be mapped to an equivalent blank node in the Target Graph.  If this is not possible then the graphs are not equal</li>
    ///     <li>Any blank node used with unique constants (two other ground terms in a triple) in the Source Graph should be mapped to an equivalent blank bode in the Target Graph.  If this is not possible then the graphs are not equal.</li>
    ///     <li>Build up lists of dependent pairs of blank Nodes for both graphs</li>
    ///     <li>Use these lists to determine if there are any independent nodes not yet mapped in the Source Graph.  These should be mapped to equivalent blank nodes in the Target Graph, if this is not possible the graphs are not equal</li>
    ///     <li><strong>Important:</strong> Keep a copy of the mapping up to this point as a <em>Base Mapping</em> for use as a fallback in later steps</li>
    ///     <li>Use the dependency information and existing mappings to generate a possible mapping</li>
    ///     <li>If a complete possible mapping (there is a mapping for each blank node from the Source Graph to the Target Graph) then test this mapping.  If it succeeds then the graphs are equal</li>
    ///     </ol>
    /// </li>
    /// <li>If we don't yet have a mapping take a divide and conquer approach:
    ///     <ol>
    ///     <li>Take the not yet mapped blank nodes for each graph and sub-divide them into their isolated sub-graphs</li>
    ///     <li>If there are at least 2 isolated sub-graphs proceed to divide and conquer</li>
    ///     <li>For Each Isolated Sub-Graph from the Source Graph
    ///         <ol>
    ///         <li>Consider each possible isolated sub-graph of the same size from the target graph, if there are none then graphs are not equal.  If there is a single possible equal isolated sub-graph add the mappings for all involved blank nodes.</li>
    ///         </ol>
    ///     </li>
    ///     <li>If we now have a complete possible mapping (there is a mapping for each blank node from the Source Graph to the Target Graph) then test the mapping.  Return success/failure depending on whether the mapping is valid.</li>
    ///     <li><strong>Important:</strong> Keep a copy of the mapping up to this point as a <em>Base Mapping</em> for use as a base for the brute force step</li>
    ///     </ol> 
    /// </li>
    /// <li>If we still don't have a complete mapping we now fallback to the <em>Base Mapping</em> and use it as a basis for brute forcing the possible solution space and testing every possibility until either a mapping works or we find the graphs to be non-equal</li>
    /// </ol>
    /// </remarks>
    public class GraphMatcher
    {
        // The Unbound and Bound lists refers to the Nodes of the Target Graph
        private List<INode> _unbound;
        private List<INode> _bound;
        private Dictionary<INode, INode> _mapping;
        private HashSet<Triple> _sourceTriples;
        private HashSet<Triple> _targetTriples;

        /// <summary>
        /// Compares two Graphs for equality
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="h">Graph</param>
        /// <returns></returns>
        
        public bool Equals(IGraph g, IGraph h)
        {
            Debug.WriteLine("Making simple equality checks");

            // If both are null then consider equal
            if (g == null && h == null)
            {
                Debug.WriteLine("[EQUAL] Both Graphs null");
                return true;
            }
            // Graphs can't be equal to null
            if (g == null)
            {
                Debug.WriteLine("[NOT EQUAL] First Graph is null");
                return false;
            }
            if (h == null)
            {
                Debug.WriteLine("[NOT EQUAL] Second Graph is null");
                return false;
            }

            // If we're the same Graph (by reference) then we're trivially equal
            if (ReferenceEquals(g, h))
            {
                Debug.WriteLine("[EQUAL] Graphs equal be reference");
                return true;
            }

            // If different number of Triples then the Graphs can't be equal
            if (g.Triples.Count != h.Triples.Count)
            {
                Debug.WriteLine("[NOT EQUAL] Differing number of triples between graphs");
                return false;
            }

            int gtCount = 0;
            Dictionary<INode, int> gNodes = new Dictionary<INode, int>();
            Dictionary<INode, int> hNodes = new Dictionary<INode, int>();
            _targetTriples = new HashSet<Triple>(h.Triples);
            foreach (Triple t in g.Triples)
            {
                if (t.IsGroundTriple)
                {
                    // If the other Graph doesn't contain the Ground Triple so can't be equal
                    // We make the contains call first as that is typically O(1) while the Remove call may be O(n)
                    if (!h.Triples.Contains(t))
                    {
                        Debug.WriteLine("[NOT EQUAL] First graph contains a ground triple which is not in the second graph");
                        return false;
                    }
                    if (!_targetTriples.Remove(t))
                    {
                        Debug.WriteLine("[NOT EQUAL] First graph contains a ground triple which is not in the second graph");
                        return false;
                    }
                    gtCount++;
                }
                else
                {
                    // If not a Ground Triple remember which Blank Nodes we need to map
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

            // If the other Graph still contains Ground Triples then the Graphs aren't equal
            if (_targetTriples.Any(t => t.IsGroundTriple))
            {
                Debug.WriteLine("[NOT EQUAL] Second Graph contains ground triples not present in first graph");
                return false;
            }
            Debug.WriteLine("Validated that there are " + gtCount + " ground triples present in both graphs");

            // If there are no Triples left in the other Graph, all our Triples were Ground Triples and there are no Blank Nodes to map the Graphs are equal
            if (_targetTriples.Count == 0 && gtCount == g.Triples.Count && gNodes.Count == 0)
            {
                Debug.WriteLine("[EQUAL] Graphs contain only ground triples and all triples are present in both graphs");
                return true;
            }

            // Now classify the remaining Triples from the other Graph
            foreach (Triple t in _targetTriples)
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

            // First off we must have the same number of Blank Nodes in each Graph
            if (gNodes.Count != hNodes.Count)
            {
                Debug.WriteLine("[NOT EQUAL] Differing number of unique blank nodes between graphs");
                return false;
            }
            Debug.WriteLine("Both graphs contain " + gNodes.Count + " unique blank nodes");

            // Then we sub-classify by the number of Blank Nodes with each degree classification
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

            // Then we must have the same number of degree classifications
            if (gDegrees.Count != hDegrees.Count)
            {
                Debug.WriteLine("[NOT EQUAL] Degree classification of Blank Nodes indicates no possible equality mapping");
                return false;
            }
            Debug.WriteLine("Degree classification of blank nodes indicates there is a possible equality mapping");

            // Then for each degree classification there must be the same number of BNodes with that degree in both Graph
            if (gDegrees.All(pair => hDegrees.ContainsKey(pair.Key) && gDegrees[pair.Key] == hDegrees[pair.Key]))
            {
                Debug.WriteLine("Validated that degree classification does provide a possible equality mapping");

                // Try to do a Rules Based Mapping
                return TryRulesBasedMapping(g, h, gNodes, hNodes, gDegrees, hDegrees);
            }
            else
            {
                // There are degree classifications that don't have the same number of BNodes with that degree 
                // or the degree classification is not in the other Graph
                Debug.WriteLine("[NOT EQUAL] Degree classification of Blank Nodes indicates no possible equality mapping");
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
            Debug.WriteLine("Attempting rules based equality mapping");

            // Start with new lists and dictionaries each time in case we get reused
            _unbound = new List<INode>();
            _bound = new List<INode>();
            _mapping = new Dictionary<INode, INode>();

            // Initialise the Source Triples list
            _sourceTriples = new HashSet<Triple>(from t in g.Triples
                                                      where !t.IsGroundTriple
                                                      select t);

            // First thing consider the trivial mapping
            Dictionary<INode, INode> trivialMapping = new Dictionary<INode, INode>();
            foreach (INode n in gNodes.Keys)
            {
                trivialMapping.Add(n, n);
            }
            HashSet<Triple> targets = new HashSet<Triple>(_targetTriples);
            if (_sourceTriples.All(t => targets.Remove(t.MapTriple(h, trivialMapping))))
            {
                _mapping = trivialMapping;
                Debug.WriteLine("[EQUAL] Trivial Mapping (all Blank Nodes have identical IDs) is a valid equality mapping");
                return true;
            }
            Debug.WriteLine("Trivial Mapping (all Blank Nodes have identical IDs) did not hold");

            // Initialise the Unbound list
            _unbound = (from n in hNodes.Keys
                             select n).ToList();

            // Map single use Nodes first to reduce the size of the overall mapping
            Debug.WriteLine("Mapping single use blank nodes");
            foreach (KeyValuePair<INode, int> pair in gNodes.Where(p => p.Value == 1))
            {
                // Find the Triple we need to map
                Triple toMap = (from t in _sourceTriples
                                where t.Involves(pair.Key)
                                select t).First();

                foreach (INode n in _unbound.Where(n => hNodes[n] == pair.Value))
                {
                    // See if this Mapping works
                    _mapping.Add(pair.Key, n);
                    if (_targetTriples.Remove(toMap.MapTriple(h, _mapping)))
                    {
                        _sourceTriples.Remove(toMap);
                        _bound.Add(n);
                        break;
                    }
                    _mapping.Remove(pair.Key);
                }

                // There is a pathological case where we can't map anything this way because
                // there are always dependencies between the blank nodes in which case we
                // still continue because we may figure out a mapping with the dependency based
                // rules or brute force approach :-(

                // Otherwise we can mark that Node as Bound
                if (_mapping.ContainsKey(pair.Key)) _unbound.Remove(_mapping[pair.Key]);
            }
            Debug.WriteLine("Single use nodes allowed mapping " + _mapping.Count + " nodes, " + _mapping.Count + " mapped out of a total " + gNodes.Count);

            // If all the Nodes were used only once and we mapped them all then the Graphs are equal
            if (_targetTriples.Count == 0)
            {
                Debug.WriteLine("[EQUAL] All Blank Nodes were single use and were successfully mapped");
                return true;
            }

            // Map any Nodes of unique degree next
            Debug.WriteLine("Trying to map unique blank nodes");
            int mappedSoFar = _mapping.Count;
            foreach (KeyValuePair<int, int> degreeClass in gDegrees)
            {
                if (degreeClass.Key > 1 && degreeClass.Value == 1)
                {
                    // There is a Node of degree greater than 1 than has a unique degree
                    // i.e. there is only one Node with this degree so there can only ever be one
                    // possible mapping for this Node
                    INode x = gNodes.FirstOrDefault(p => p.Value == degreeClass.Key).Key;
                    INode y = hNodes.FirstOrDefault(p => p.Value == degreeClass.Key).Key;

                    // If either of these return null then the Graphs can't be equal
                    if (x == null || y == null)
                    {
                        Debug.WriteLine("[NOT EQUAL] Node with unique degree could not be mapped");
                        _mapping = null;
                        return false;
                    }

                    // Add the Mapping
                    _mapping.Add(x, y);
                    _bound.Add(y);
                    _unbound.Remove(y);
                }
            }
            Debug.WriteLine("Unique blank nodes allowing mapping of " + (_mapping.Count - mappedSoFar) + " nodes, " + _mapping.Count + " mapped out of a total " + gNodes.Count);
            mappedSoFar = _mapping.Count;

            // Then look for nodes which are associated with unique constants
            // By this we mean nodes that occur as the subject/object of a triple where the other two parts are non-blank
            Debug.WriteLine("Trying to map blank nodes based on unique constants associated with the nodes");
            foreach (Triple t in _sourceTriples)
            {
                if (t.Subject.NodeType == NodeType.Blank && t.Predicate.NodeType != NodeType.Blank && t.Object.NodeType != NodeType.Blank)
                {
                    // Ignore if already mapped
                    if (_mapping.ContainsKey(t.Subject)) continue;

                    // Are there any possible matches?
                    // We only need to know about at most 2 possibilities since zero possiblities means non-equal graphs, one is a valid mapping and two or more is not mappable this way
                    List<Triple> possibles = new List<Triple>(_targetTriples.Where(x => x.Subject.NodeType == NodeType.Blank && x.Predicate.Equals(t.Predicate) && x.Object.Equals(t.Object)).Take(2));
                    if (possibles.Count == 1)
                    {
                        // Precisely one possible match so map
                        INode x = t.Subject;
                        INode y = possibles.First().Subject;
                        _mapping.Add(x, y);
                        _bound.Add(y);
                        _unbound.Remove(y);
                    }
                    else if (possibles.Count == 0)
                    {
                        // No possible matches so not equal graphs
                        Debug.WriteLine("[NOT EQUAL] Node used in a triple with two constants where no candidate mapping for that triple could be found");
                        _mapping = null;
                        return false;
                    }
                }
                else if (t.Subject.NodeType != NodeType.Blank && t.Predicate.NodeType != NodeType.Blank && t.Object.NodeType == NodeType.Blank)
                {
                    // Ignore if already mapped
                    if (_mapping.ContainsKey(t.Object)) continue;

                    // Are there any possible matches?
                    // We only need to know about at most 2 possibilities since zero possiblities means non-equal graphs, one is a valid mapping and two or more is not mappable this way
                    List<Triple> possibles = new List<Triple>(_targetTriples.Where(x => x.Subject.Equals(t.Subject) && x.Predicate.Equals(t.Predicate) && x.Object.NodeType == NodeType.Blank).Take(2));
                    if (possibles.Count == 1)
                    {
                        // Precisely one possible match so map
                        INode x = t.Object;
                        INode y = possibles.First().Object;
                        _mapping.Add(x, y);
                        _bound.Add(y);
                        _unbound.Remove(y);
                    }
                }
            }
            Debug.WriteLine("Using unique constants associated with blank nodes allowed mapping of " + (_mapping.Count - mappedSoFar) + " blank nodes, " + _mapping.Count + " mapped out of a total " + gNodes.Count);
            mappedSoFar = _mapping.Count;

            // Work out which Nodes are paired up 
            // By this we mean any Nodes which appear with other Nodes in a Triple
            // If multiple nodes appear together we can use this information to restrict
            // the possible mappings we generate
            List<MappingPair> sourceDependencies = new List<MappingPair>();
            foreach (Triple t in _sourceTriples)
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
            foreach (Triple t in _targetTriples)
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

            // Once we know of dependencies we can then map independent nodes
            List<INode> sourceIndependents = (from n in gNodes.Keys
                                              where !sourceDependencies.Any(p => p.Contains(n))
                                              select n).ToList();
            List<INode> targetIndependents = (from n in hNodes.Keys
                                              where !targetDependencies.Any(p => p.Contains(n))
                                              select n).ToList();

            // If the number of independent nodes in the two Graphs is different we return false
            if (sourceIndependents.Count != targetIndependents.Count)
            {
                Debug.WriteLine("[NOT EQUAL] Graphs contain different number of independent blank nodes");
                return false;
            }
            Debug.WriteLine("Graphs contain " + sourceIndependents.Count + " independent blank nodes, attempting mapping");

            // Try to map the independent nodes
            foreach (INode x in sourceIndependents)
            {
                // They may already be mapped as they may be single use Triples
                if (_mapping.ContainsKey(x)) continue;

                List<Triple> xs = _sourceTriples.Where(t => t.Involves(x)).ToList();
                foreach (INode y in targetIndependents)
                {
                    if (gNodes[x] != hNodes[y]) continue;

                    _mapping.Add(x, y);

                    // Test the mapping
                    HashSet<Triple> ys = new HashSet<Triple>(_targetTriples.Where(t => t.Involves(y)));
                    if (xs.All(t => ys.Remove(t.MapTriple(h, _mapping))))
                    {
                        // This is a valid mapping
                        xs.ForEach(t => _targetTriples.Remove(t.MapTriple(h, _mapping)));
                        xs.ForEach(t => _sourceTriples.Remove(t));
                        break;
                    }
                    _mapping.Remove(x);
                }

                // If we couldn't map an independent Node then we fail
                if (!_mapping.ContainsKey(x))
                {
                    Debug.WriteLine("[NOT EQUAL] Independent blank node could not be mapped");
                    return false;
                }
            }
            Debug.WriteLine("Independent blank node mapping was able to map " + (_mapping.Count - mappedSoFar) + " blank nodes, " + _mapping.Count + " blank nodes out of a total " + gNodes.Count);
            mappedSoFar = _mapping.Count;

            // Want to save our mapping so far here as if the mapping we produce using the dependency information
            // is flawed then we'll have to attempt brute force
            Dictionary<INode, INode> baseMapping = new Dictionary<INode, INode>(_mapping);

            Debug.WriteLine("Using dependency information to try and map more blank nodes");

            // Now we use the dependency information to try and find mappings
            foreach (MappingPair dependency in sourceDependencies)
            {
                // If both dependent Nodes are already mapped we don't need to try mapping them again
                if (_mapping.ContainsKey(dependency.X) && _mapping.ContainsKey(dependency.Y)) continue;

                // Get all the Triples with this Pair in them
                List<Triple> xs;
                bool canonical = false;
                switch (dependency.Type)
                {
                    case TripleIndexType.SubjectPredicate:
                        xs = (from t in _sourceTriples
                              where t.Subject.Equals(dependency.X) && t.Predicate.Equals(dependency.Y)
                              select t).ToList();
                        if (xs.Count == 1)
                        {
                            canonical = ((from t in _sourceTriples
                                          where t.Subject.NodeType == NodeType.Blank && t.Predicate.NodeType == NodeType.Blank && t.Object.Equals(xs[0].Object)
                                          select t).Count() == 1);
                        }
                        break;
                    case TripleIndexType.SubjectObject:
                        xs = (from t in _sourceTriples
                              where t.Subject.Equals(dependency.X) && t.Object.Equals(dependency.Y)
                              select t).ToList();
                        if (xs.Count == 1)
                        {
                            canonical = ((from t in _sourceTriples
                                          where t.Subject.NodeType == NodeType.Blank && t.Predicate.Equals(xs[0].Predicate) && t.Object.NodeType == NodeType.Blank
                                          select t).Count() == 1);
                        }
                        break;
                    case TripleIndexType.PredicateObject:
                        xs = (from t in _sourceTriples
                              where t.Predicate.Equals(dependency.X) && t.Object.Equals(dependency.Y)
                              select t).ToList();
                        if (xs.Count == 1)
                        {
                            canonical = ((from t in _sourceTriples
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
                foreach (MappingPair target in targetDependencies)
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
                            ys = (from t in _targetTriples
                                  where t.Subject.Equals(target.X) && t.Predicate.Equals(target.Y)
                                  select t).ToList();
                            break;
                        case TripleIndexType.SubjectObject:
                            ys = (from t in _targetTriples
                                  where t.Subject.Equals(target.X) && t.Object.Equals(target.Y)
                                  select t).ToList();
                            break;
                        case TripleIndexType.PredicateObject:
                            ys = (from t in _targetTriples
                                  where t.Predicate.Equals(target.X) && t.Object.Equals(target.Y)
                                  select t).ToList();
                            break;
                        default:
                            // This means we've gone wrong somehow
                            throw new RdfException("Unknown exception occurred while trying to generate a Mapping between the two Graphs");
                    }

                    // If the pairs are involved in different numbers of Triples then they aren't possible mappings
                    if (xs.Count != ys.Count) continue;

                    // If all the Triples in xs can be removed from ys then this is a valid mapping
                    if (!xbound) _mapping.Add(dependency.X, target.X);
                    if (!ybound) _mapping.Add(dependency.Y, target.Y);
                    if (xs.All(t => ys.Remove(t.MapTriple(h, _mapping))))
                    {
                        if (canonical)
                        {
                            // If this was the only possible mapping for this Pair then this is a canonical mapping
                            // and can go in the base mapping so if we have to brute force we have fewer possibles
                            // to try
                            if (!baseMapping.ContainsKey(dependency.X)) baseMapping.Add(dependency.X, target.X);
                            if (!baseMapping.ContainsKey(dependency.Y)) baseMapping.Add(dependency.Y, target.Y);

                            _bound.Add(target.X);
                            _bound.Add(target.Y);
                            _unbound.Remove(target.X);
                            _unbound.Remove(target.Y);
                        }
                        break;
                    }
                    else
                    {
                        if (!xbound) _mapping.Remove(dependency.X);
                        if (!ybound) _mapping.Remove(dependency.Y);
                    }
                }
            }
            Debug.WriteLine("Dependency information allowed us to map a further " + (_mapping.Count - mappedSoFar) + " nodes, we now have a possible mapping with " + _mapping.Count + " blank nodes mapped (" + baseMapping.Count + " confirmed mappings) out of a total " + gNodes.Count + " blank nodes");

            // If we've filled in the Mapping fully then the Graphs are hopefully equal
            if (_mapping.Count == gNodes.Count)
            {
                // Need to check we found a valid mapping
                List<Triple> ys = new List<Triple>(_targetTriples);
                if (_sourceTriples.All(t => ys.Remove(t.MapTriple(h, _mapping))))
                {
                    Debug.WriteLine("[EQUAL] Generated a rules based mapping successfully");
                    return true;
                }
                else
                {
                    // Fall back should to a divide and conquer mapping
                    Debug.WriteLine("Had a potential rules based mapping but it was invalid, falling back to divide and conquer mapping with base mapping of " + baseMapping.Count + " nodes");
                    _mapping = baseMapping;
                    return TryDivideAndConquerMapping(g, h, gNodes, hNodes, sourceDependencies, targetDependencies);
                }
            }
            else
            {
                Debug.WriteLine("Rules based mapping did not generate a complete mapping, falling back to divide and conquer mapping");
                return TryDivideAndConquerMapping(g, h, gNodes, hNodes, sourceDependencies, targetDependencies);
            }
        }

        /// <summary>
        /// Uses a divide and conquer based approach to generate a mapping without the need for brute force guessing
        /// </summary>
        /// <param name="g">1st Graph</param>
        /// <param name="h">2nd Graph</param>
        /// <param name="gNodes">1st Graph Node classification</param>
        /// <param name="hNodes">2nd Graph Node classification</param>
        /// <param name="sourceDependencies">Dependencies in the 1st Graph</param>
        /// <param name="targetDependencies">Dependencies in the 2nd Graph</param>
        /// <returns></returns>
        private bool TryDivideAndConquerMapping(IGraph g, IGraph h, Dictionary<INode, int> gNodes, Dictionary<INode, int> hNodes, List<MappingPair> sourceDependencies, List<MappingPair> targetDependencies)
        {
            Debug.WriteLine("Attempting divide and conquer based mapping");

            // Need to try and split the BNodes into MSGs
            // Firstly we need a copy of the unassigned triples
            HashSet<Triple> gUnassigned = new HashSet<Triple>(_sourceTriples);
            HashSet<Triple> hUnassigned = new HashSet<Triple>(_targetTriples);

            // Remove already mapped nodes
            gUnassigned.RemoveWhere(t => _mapping != null && _mapping.Keys.Any(n => t.Involves(n)));
            hUnassigned.RemoveWhere(t => _mapping != null && _mapping.Any(kvp => t.Involves(kvp.Value)));

            // Compute MSGs
            List<IGraph> gMsgs = new List<IGraph>();
            GraphDiff.ComputeMSGs(g, gUnassigned, gMsgs);
            List<IGraph> hMsgs = new List<IGraph>();
            GraphDiff.ComputeMSGs(h, hUnassigned, hMsgs);

            if (gMsgs.Count != hMsgs.Count)
            {
                Debug.WriteLine("[NOT EQUAL] Blank Node portions of graphs decomposed into differing numbers of isolated sub-graphs");
                _mapping = null;
                return false;
            }
            else if (gMsgs.Count == 1)
            {
                Debug.WriteLine("Blank Node potions of graphs did not decompose into isolated sub-graphs, falling back to brute force mapping");
                return TryBruteForceMapping(g, h, gNodes, hNodes, sourceDependencies, targetDependencies);
            }
            Debug.WriteLine("Blank Node portions of graphs decomposed into " + gMsgs.Count + " isolated sub-graphs");

            // Sort sub-graphs by triple count
            gMsgs.Sort(new GraphSizeComparer());
            hMsgs.Sort(new GraphSizeComparer());
            bool retry = true;
            int retryCount = 1;

            while (retry)
            {
                int found = 0;
                List<IGraph> lhsMsgs = new List<IGraph>(gMsgs);
                if (lhsMsgs.Count == 0) break;

                Debug.WriteLine("Divide and conquer attempt #" + retryCount);

                while (lhsMsgs.Count > 0)
                {
                    IGraph lhs = lhsMsgs[0];
                    lhsMsgs.RemoveAt(0);

                    // Find possible matches
                    List<IGraph> possibles = new List<IGraph>(hMsgs.Where(graph => graph.Triples.Count == lhs.Triples.Count));

                    if (possibles.Count == 0)
                    {
                        Debug.WriteLine("[NOT EQUAL] Isolated sub-graphs are of differing sizes");
                        _mapping = null;
                        return false;
                    }

                    // Check each possible match
                    List<Dictionary<INode, INode>> partialMappings = new List<Dictionary<INode, INode>>();
                    List<IGraph> partialMappingSources = new List<IGraph>();
                    Debug.WriteLine("Dividing and conquering on isolated sub-graph with " + lhs.Triples.Count + " triples...");
#if !NETCORE
                    Debug.Indent();
#endif
                    int i = 1;
                    foreach (IGraph rhs in possibles)
                    {
                        Debug.WriteLine("Testing possiblity " + i + " of " + possibles.Count);
#if !NETCORE
                        Debug.Indent();
#endif
                        Dictionary<INode, INode> partialMapping;
                        if (lhs.Equals(rhs, out partialMapping))
                        {
                            partialMappings.Add(partialMapping);
                            partialMappingSources.Add(rhs);
                        }
#if !NETCORE
                        Debug.Unindent();
#endif
                        i++;
                    }
#if !NETCORE
                    Debug.Unindent();
#endif
                    Debug.WriteLine("Dividing and conquering done");

                    // Did we find a possible mapping for the sub-graph?
                    if (partialMappings.Count == 0)
                    {
                        // No possible mappings
                        Debug.WriteLine("[NOT EQUAL] Divide and conquer found an isolated sub-graph that has no possible matches");
                        _mapping = null;
                        return false;
                    }
                    else if (partialMappings.Count == 1)
                    {
                        // Only one possible match
                        foreach (KeyValuePair<INode, INode> kvp in partialMappings[0])
                        {
                            if (_mapping.ContainsKey(kvp.Key))
                            {
                                Debug.WriteLine("[NOT EQUAL] Divide and conque found a sub-graph with a single possible mapping that conflicts with the existing confirmed mappings");
                                _mapping = null;
                                return false;
                            }
                            else
                            {
                                _mapping.Add(kvp.Key, kvp.Value);
                            }
                        }
                        Debug.WriteLine("Divide and conquer found a unique mapping for an isolated sub-graph and confirmed an additional " + partialMappings[0].Count + " blank node mappings");

                        // Can eliminate the matched sub-graph from further consideration
                        gMsgs.RemoveAll(x => ReferenceEquals(x, lhs));
                        hMsgs.RemoveAll(x => ReferenceEquals(x, partialMappingSources[0]));
                        found++;
                    }
                    else
                    {
                        // Multiple possible mappings
                        Debug.WriteLine("Divide and conquer found " + partialMappings.Count + " possible mappings for an isolated sub-graph, unable to confirm any mappings");
                    }
                }

                retry = found > 0;

                if (retry)
                {
                    Debug.WriteLine("Divide and conquer Attempt #" + retryCount + " found " + found + " isolated sub-graph matches, will retry to see if confirmed matches permit further matches to be made");
                }
                else
                {
                    Debug.WriteLine("Divide and conquer Attempt #" + retryCount + " found no further isolated sub-graph matches");
                }
                retryCount++;
            }

            // If we now have a complete mapping test it
            if (_mapping.Count == gNodes.Count)
            {
                // Need to check we found a valid mapping
                List<Triple> ys = new List<Triple>(_targetTriples);
                if (_sourceTriples.All(t => ys.Remove(t.MapTriple(h, _mapping))))
                {
                    Debug.WriteLine("[EQUAL] Generated a divide and conquer based mapping successfully");
                    return true;
                }
                else
                {
                    // Invalid mapping
                    Debug.WriteLine("[NOT EQUAL] Divide and conquer led to an invalid mapping");
                    _mapping = null;
                    return false;
                }
            }
            else
            {
                // If dvide and conquer fails fall back to brute force
                Debug.WriteLine("Divide and conquer based mapping did not succeed in mapping everything, have " + _mapping.Count + " confirmed mappings out of " + gNodes.Count + " total blank nodes, falling back to brute force mapping");
                return TryBruteForceMapping(g, h, gNodes, hNodes, sourceDependencies, targetDependencies);
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

            // Populate possibilities for each Node
            foreach (KeyValuePair<INode, int> gPair in gNodes)
            {
                if (!_mapping.ContainsKey(gPair.Key))
                {
                    possibleMappings.Add(gPair.Key, new List<INode>());
                    foreach (var hPair in hNodes)
                    {
                        if (hPair.Value == gPair.Value && !_bound.Contains(hPair.Key))
                        {
                            possibleMappings[gPair.Key].Add(hPair.Key);
                        }
                    }
                    //foreach (KeyValuePair<INode, int> hPair in hNodes.Where(p => p.Value == gPair.Value && !_bound.Contains(p.Key)))
                    //{
                    //    possibleMappings[gPair.Key].Add(hPair.Key);
                    //}

                    // If there's no possible matches for the Node we fail
                    if (possibleMappings[gPair.Key].Count == 0)
                    {
                        Debug.WriteLine("[NOT EQUAL] Unable to find any possible mappings for a blank node");
                        _mapping = null;
                        return false;
                    }
                }
            }

            // This should never happen but handle it just in case
            if (possibleMappings.Count == 0)
            {
                throw new RdfException("Unexpected error trying to brute force graph equality");
            }

            // Now start testing the possiblities
            IEnumerable<Dictionary<INode, INode>> possibles = GenerateMappings(new Dictionary<INode, INode>(_mapping), possibleMappings);
            int count = 0;
            foreach (Dictionary<INode, INode> mapping in possibles)
            {
                count++;
                if (mapping.Count != gNodes.Count) continue;

                HashSet<Triple> targets = new HashSet<Triple>(_targetTriples);
                if (_sourceTriples.Count != targets.Count) continue;

                // Validate the mapping
                if (_sourceTriples.All(t => targets.Remove(t.MapTriple(h, mapping))))
                {
                    _mapping = mapping;
                    Debug.WriteLine("[EQUAL] Succesfully brute forced a mapping on Attempt #" + count);
                    return true;
                }
            }

            Debug.WriteLine("[NOT EQUAL] No valid brute forced mappings (" + count + " were considered)");
            _mapping = null;
            return false;
        }

        /// <summary>
        /// Helper method for brute forcing the possible mappings
        /// </summary>
        /// <param name="baseMapping">Base Mapping</param>
        /// <param name="possibleMappings">Possible Mappings</param>
        /// <returns></returns>
        /// <remarks>
        /// The base mapping at the time of the initial call should contain known good mappings
        /// </remarks>
        public static IEnumerable<Dictionary<INode, INode>> GenerateMappings(Dictionary<INode, INode> baseMapping, Dictionary<INode, List<INode>> possibleMappings)
        {
            // Remove any explicit base mappings from possible mappings
            foreach (var p in baseMapping)
            {
                if (possibleMappings.TryGetValue(p.Key, out var mappings))
                {
                    mappings.Remove(p.Value);
                    if (mappings.Count == 0) possibleMappings.Remove(p.Key);
                }
            }
            INode x = possibleMappings.Keys.First();
            foreach (Dictionary<INode, INode> mapping in GenerateMappingsInternal(baseMapping, possibleMappings, x, baseMapping.Count + possibleMappings.Count))
            {
                yield return mapping;
            }
        }

        /// <summary>
        /// Helper method for brute forcing the possible mappings
        /// </summary>
        /// <param name="baseMapping">Base Mapping</param>
        /// <param name="possibleMappings">Possible Mappings</param>
        /// <param name="x">Node to consider for mapping</param>
        /// <returns></returns>
        /// <remarks>
        /// The base mapping contains known good mappings
        /// </remarks>
        private static IEnumerable<Dictionary<INode, INode>> GenerateMappingsInternal(Dictionary<INode, INode> baseMapping, Dictionary<INode, List<INode>> possibleMappings, INode x, int targetCount)
        {
            List<INode> possibles = possibleMappings[x];

            // For each possiblity build a possible mapping
            foreach (INode y in possibles)
            {
                Dictionary<INode, INode> test = new Dictionary<INode, INode>(baseMapping);
                if (!test.ContainsKey(x)) test.Add(x, y);

                if (test.Count == targetCount)
                {
                    yield return test;
                }
                else
                {
                    // Go ahead and recurse
                    foreach (INode x2 in possibleMappings.Keys)
                    {
                        if (test.ContainsKey(x2)) continue;
                        foreach (Dictionary<INode, INode> mapping in GenerateMappingsInternal(test, possibleMappings, x2, targetCount))
                        {
                            yield return mapping;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Blank Node Mapping found between the Graphs (if one was found)
        /// </summary>
        public Dictionary<INode, INode> Mapping
        {
            get
            {
                return _mapping;
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
            _x = x;
            _y = y;
            _type = type;
            _hash = Tools.CombineHashCodes(x, y);
        }

        public INode X
        {
            get
            {
                return _x;
            }
        }

        public INode Y
        {
            get
            {
                return _y;
            }
        }

        public TripleIndexType Type
        {
            get
            {
                return _type;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is MappingPair)
            {
                MappingPair p = (MappingPair)obj;
                return (_x.Equals(p.X) && _y.Equals(p.Y) && _type == p.Type);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        public bool Contains(INode n)
        {
            return (_x.Equals(n) || _y.Equals(n));
        }
    }
}
