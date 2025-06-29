/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using System;
using System.Collections.Generic;
using VDS.Common.Collections;

namespace VDS.RDF.Query.Inference;

/// <summary>
/// An Inference Engine which uses RDFS reasoning.
/// </summary>
/// <remarks>
/// <para>
/// Does basic RDFS inferencing using the schema taken from the Graph(s) which are provided in calls to the reasoners <see cref="StaticRdfsReasoner.Initialise">Initialise()</see> method.
/// </para>
/// <para>
/// Types of inference performed are as follows:.
/// </para>
/// <ul>
///     <li>Class hierarchy reasoning - asserts additional types triples for anything that is typed as the subclass of a class.</li>
///     <li>Property hierarchy reasoning - asserts additional property triples for anything where the predicate is a subproperty of a defined property</li>
///     <li>Domain &amp; Range reasoning - asserts additional type triples based on the domains and ranges of properties</li>
/// </ul>
/// </remarks>
public class StaticRdfsReasoner : IInferenceEngine
{
    private readonly Dictionary<INode, List<INode>> _classMappings = new();
    private readonly Dictionary<INode, List<INode>> _propertyMappings = new();
    private readonly MultiDictionary<INode, List<INode>> _domainMappings = new MultiDictionary<INode, List<INode>>(new FastVirtualNodeComparer());
    private readonly MultiDictionary<INode, List<INode>> _rangeMappings = new MultiDictionary<INode, List<INode>>(new FastVirtualNodeComparer());
    private readonly IUriNode _rdfType, _rdfsClass, _rdfsSubClass, _rdfProperty, _rdfsSubProperty, _rdfsRange, _rdfsDomain;

    /// <summary>
    /// Creates a new instance of the Static RdfsReasoner.
    /// </summary>
    public StaticRdfsReasoner()
    {
        var g = new Graph();
        _rdfType = g.CreateUriNode("rdf:type");
        _rdfsClass = g.CreateUriNode("rdfs:Class");
        _rdfsSubClass = g.CreateUriNode("rdfs:subClassOf");
        _rdfProperty = g.CreateUriNode("rdf:Property");
        _rdfsSubProperty = g.CreateUriNode("rdfs:subPropertyOf");
        _rdfsDomain = g.CreateUriNode("rdfs:domain");
        _rdfsRange = g.CreateUriNode("rdfs:range");
    }

    /// <summary>
    /// Applies inference to the given Graph and outputs the inferred information to that Graph.
    /// </summary>
    /// <param name="g">Graph.</param>
    public virtual void Apply(IGraph g)
    {
        Apply(g, g);
    }

    /// <summary>
    /// Applies inference to the Input Graph and outputs the inferred information to the Output Graph.
    /// </summary>
    /// <param name="input">Graph to apply inference to.</param>
    /// <param name="output">Graph inferred information is output to.</param>
    public virtual void Apply(IGraph input, IGraph output)
    {
        // Infer information
        var inferences = new List<Triple>();
        foreach (Triple t in input.Triples)
        {
            // Apply class/property hierarchy inferencing
            if (t.Predicate.Equals(_rdfType))
            {
                if (!t.Object.Equals(_rdfsClass) && !t.Object.Equals(_rdfProperty))
                {
                    InferClasses(t, inferences);
                }
            }
            else if (t.Predicate.Equals(_rdfsSubClass))
            {
                // Assert that this thing is a Class
                inferences.Add(new Triple(t.Subject, _rdfType, _rdfsClass));
            }
            else if (t.Predicate.Equals(_rdfsSubProperty))
            {
                // Assert that this thing is a Property
                inferences.Add(new Triple(t.Subject, _rdfType, _rdfProperty));
            }
            else if (_propertyMappings.ContainsKey(t.Predicate))
            {
                InferPredicates(t, inferences);
            }

            // Apply Domain and Range inferencing on Predicates
            if (_rangeMappings.TryGetValue(t.Predicate, out List<INode> rangeMapping))
            {
                // Assert additional type information
                foreach (INode n in rangeMapping)
                {
                    inferences.Add(new Triple(t.Object, _rdfType, n));
                }

                // Call InferClasses to get extra type information
                InferClasses(inferences[inferences.Count - 1], inferences);
            }
            if (_domainMappings.TryGetValue(t.Predicate, out List<INode> domainMapping))
            {
                // Assert additional type information
                foreach (INode n in domainMapping)
                {
                    inferences.Add(new Triple(t.Subject, _rdfType, n));
                }
                
                // Call InferClasses to get extra type information
                InferClasses(inferences[inferences.Count - 1], inferences);
            }
        }

        // Assert the inferred information
        inferences.RemoveAll(t => t.Subject.NodeType == NodeType.Literal);
        if (inferences.Count > 0)
        {
            output.Assert(inferences);
        }
    }

    /// <summary>
    /// Imports any Class hierarchy information from the given Graph into the Reasoners Knowledge Base in order to initialise the Reasoner.
    /// </summary>
    /// <param name="g">Graph to import from.</param>
    /// <remarks>
    /// Looks for Triples defining things to be classes and those defining that something is a subClass of something.
    /// </remarks>
    public void Initialise(IGraph g)
    {
        foreach (Triple t in g.Triples)
        {
            if (t.Predicate.Equals(_rdfType))
            {
                if (t.Object.Equals(_rdfsClass))
                {
                    // The Triple defines a Class
                    if (!_classMappings.ContainsKey(t.Subject))
                    {
                        _classMappings.Add(t.Subject, null);
                    }
                } 
                else if (t.Object.Equals(_rdfProperty)) 
                {
                    // The Triple defines a Property
                    if (!_propertyMappings.ContainsKey(t.Subject)) 
                    {
                        _propertyMappings.Add(t.Subject, null);
                    }
                }
            }
            else if (t.Predicate.Equals(_rdfsSubClass))
            {
                // The Triple defines a Sub Class
                if (!_classMappings.ContainsKey(t.Subject))
                {
                    _classMappings.Add(t.Subject, new List<INode>());
                }
                else if (_classMappings[t.Subject] == null)
                {
                    _classMappings[t.Subject] = new List<INode>();
                }
                _classMappings[t.Subject].Add(t.Object);
            }
            else if (t.Predicate.Equals(_rdfsSubProperty))
            {
                // The Triple defines a Sub property
                if (!_propertyMappings.ContainsKey(t.Subject))
                {
                    _propertyMappings.Add(t.Subject, new List<INode>());
                }
                else if (_propertyMappings[t.Subject] == null)
                {
                    _propertyMappings[t.Subject] = new List<INode>();
                }
                _propertyMappings[t.Subject].Add(t.Object);
            }
            else if (t.Predicate.Equals(_rdfsRange))
            {
                // This Triple defines a Range
                if (!_propertyMappings.ContainsKey(t.Subject))
                {
                    _propertyMappings.Add(t.Subject, null);
                }
                if (!_rangeMappings.ContainsKey(t.Subject))
                {
                    _rangeMappings.Add(t.Subject, new List<INode> { t.Object });
                }
                if (!_classMappings.ContainsKey(t.Object))
                {
                    _classMappings.Add(t.Object, null);
                }
            }
            else if (t.Predicate.Equals(_rdfsDomain))
            {
                // This Triple defines a Domain
                if (!_propertyMappings.ContainsKey(t.Subject))
                {
                    _propertyMappings.Add(t.Subject, null);
                }
                if (!_domainMappings.ContainsKey(t.Subject))
                {
                    _domainMappings.Add(t.Subject, new List<INode> { t.Object });
                }
                if (!_classMappings.ContainsKey(t.Object))
                {
                    _classMappings.Add(t.Object, null);
                }
            }
            else
            {
                // Just add the property as a predicate
                if (!_propertyMappings.ContainsKey(t.Predicate))
                {
                    _propertyMappings.Add(t.Predicate, null);
                }
            }
        }
    }

    /// <summary>
    /// Helper method which applies Class hierarchy inferencing.
    /// </summary>
    /// <param name="t">Triple defining the type for something.</param>
    /// <param name="inferences">List of Inferences.</param>
    private void InferClasses(Triple t, List<Triple> inferences)
    {
        InferFromMappings(_classMappings, t.Object, (node) => new Triple(t.Subject, t.Predicate, node), inferences, new HashSet<INode>());
    }
    
    private void InferPredicates(Triple t, List<Triple> inferences)
    {
        InferFromMappings(_propertyMappings, t.Predicate, (node) => new Triple(t.Subject, node, t.Object), inferences, new HashSet<INode>());
    }

    private void InferFromMappings(IDictionary<INode, List<INode>> mappings, INode mappingKey,
        Func<INode, Triple> inferenceFn, List<Triple> inferences, HashSet<INode> visited)
    {
        visited.Add(mappingKey);
        if (mappings.TryGetValue(mappingKey, out List<INode> mappingValues) && mappingValues != null)
        {
            foreach (INode mappingValue in mappingValues)
            {
                if (!visited.Contains(mappingValue))
                {
                    inferences.Add(inferenceFn(mappingValue));
                    InferFromMappings(mappings, mappingValue, inferenceFn, inferences, visited);
                }
            }
        }
    }
}

/// <summary>
/// An Inference Engine which uses RDFS reasoning.
/// </summary>
/// <remarks>
/// Does basic RDFS inferencing as detailed in the remarks for the <see cref="StaticRdfsReasoner">StaticRdfsReasoner</see> except every Graph that inference is applied to has the potential to alter the schema which is in use.
/// </remarks>
public class RdfsReasoner 
    : StaticRdfsReasoner
{
    /// <summary>
    /// Applies inference to the Input Graph and outputs the inferred information to the Output Graph.
    /// </summary>
    /// <param name="input">Graph to apply inference to.</param>
    /// <param name="output">Graph inferred information is output to.</param>
    public override void Apply(IGraph input, IGraph output)
    {
        // Use this Graph to further initialise the Reasoner
        Initialise(input);

        // Use the Base Reasoner to do the inference
        base.Apply(input, output);
    }
}
