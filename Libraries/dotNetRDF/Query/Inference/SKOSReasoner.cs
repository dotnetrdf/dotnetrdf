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

using System;
using System.Collections.Generic;

namespace VDS.RDF.Query.Inference
{
    /// <summary>
    /// An Inference Engine that uses SKOS Concept Hierarchies
    /// </summary>
    /// <remarks>
    /// <para>
    /// Infers additional values for properties based on SKOS Concept Hierarcies.  If there is a Triple whose value is a Concept from the hierarchy then new versions of that Triple will be inferred where the object becomes each concept higher in the hierarchy.
    /// </para>
    /// </remarks>
    public class StaticSkosReasoner : IInferenceEngine
    {
        private Dictionary<INode, INode> _conceptMappings = new Dictionary<INode, INode>();
        private IUriNode _rdfType, _skosConcept, _skosNarrower, _skosBroader;

        /// <summary>
        /// Namespace for SKOS
        /// </summary>
        public const String SKOSNamespace = "http://www.w3.org/2004/02/skos/core#";

        /// <summary>
        /// Creates a new instance of the SKOS Reasoner
        /// </summary>
        public StaticSkosReasoner()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("skos", UriFactory.Create(SKOSNamespace));
            _rdfType = g.CreateUriNode("rdf:type");
            _skosBroader = g.CreateUriNode("skos:broader");
            _skosConcept = g.CreateUriNode("skos:Concept");
            _skosNarrower = g.CreateUriNode("skos:narrower");
        }

        /// <summary>
        /// Applies inference to the given Graph and outputs the inferred information to that Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public virtual void Apply(IGraph g)
        {
            Apply(g, g);
        }

        /// <summary>
        /// Applies inference to the Input Graph and outputs the inferred information to the Output Graph
        /// </summary>
        /// <param name="input">Graph to apply inference to</param>
        /// <param name="output">Graph inferred information is output to</param>
        public virtual void Apply(IGraph input, IGraph output)
        {
            List<Triple> inferences = new List<Triple>();
            lock (_conceptMappings)
            {
                foreach (Triple t in input.Triples)
                {
                    if (!(t.Predicate.Equals(_skosBroader) || t.Predicate.Equals(_skosNarrower)) && _conceptMappings.ContainsKey(t.Object))
                    {
                        INode concept = t.Object;
                        while (_conceptMappings.ContainsKey(concept))
                        {
                            if (_conceptMappings[concept] != null)
                            {
                                // Assert additional information
                                inferences.Add(new Triple(t.Subject.CopyNode(output), t.Predicate.CopyNode(output), _conceptMappings[concept].CopyNode(output)));
                                concept = _conceptMappings[concept];
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            if (inferences.Count > 0)
            {
                output.Assert(inferences);
            }
        }

        /// <summary>
        /// Imports any Concept heirarchy information from the given Graph into the Reasoners Knowledge Base in order to initialise the Reasoner
        /// </summary>
        /// <param name="g">Graph to import from</param>
        /// <remarks>
        /// Looks for Triples defining SKOS concepts and relating them to narrower and broader concepts
        /// </remarks>
        public void Initialise(IGraph g)
        {
            lock (_conceptMappings)
            {
                foreach (Triple t in g.Triples)
                {
                    if (t.Predicate.Equals(_rdfType) && t.Object.Equals(_skosConcept))
                    {
                        // Defines a SKOS Concept
                        if (!_conceptMappings.ContainsKey(t.Subject))
                        {
                            _conceptMappings.Add(t.Subject, null);
                        }
                    }
                    else if (t.Predicate.Equals(_skosNarrower))
                    {
                        // Links a SKOS Concept to a child concept
                        if (!_conceptMappings.ContainsKey(t.Object))
                        {
                            _conceptMappings.Add(t.Object, t.Subject);
                        }
                        else if (_conceptMappings[t.Object] == null)
                        {
                            _conceptMappings[t.Object] = t.Subject;
                        }
                    }
                    else if (t.Predicate.Equals(_skosBroader))
                    {
                        // Links a SKOS Concept to a parent concept
                        if (!_conceptMappings.ContainsKey(t.Subject))
                        {
                            _conceptMappings.Add(t.Subject, t.Object);
                        }
                        else if (_conceptMappings[t.Subject] == null)
                        {
                            _conceptMappings[t.Subject] = t.Object;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// An Inference Engine that uses SKOS Concept Hierarchies
    /// </summary>
    public class SkosReasoner : StaticSkosReasoner
    {
        /// <summary>
        /// Applies inference to the Input Graph and outputs the inferred information to the Output Graph
        /// </summary>
        /// <param name="input">Graph to apply inference to</param>
        /// <param name="output">Graph inferred information is output to</param>
        public override void Apply(IGraph input, IGraph output)
        {
            // Use this Graph to further initialise the Reasoner
            Initialise(input);

            // Use Base Reasoner to do the Inference
            base.Apply(input, output);
        }
    }
}
