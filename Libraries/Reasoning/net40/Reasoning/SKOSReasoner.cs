/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Text;

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
            this._rdfType = g.CreateUriNode("rdf:type");
            this._skosBroader = g.CreateUriNode("skos:broader");
            this._skosConcept = g.CreateUriNode("skos:Concept");
            this._skosNarrower = g.CreateUriNode("skos:narrower");
        }

        /// <summary>
        /// Applies inference to the given Graph and outputs the inferred information to that Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public virtual void Apply(IGraph g)
        {
            this.Apply(g, g);
        }

        /// <summary>
        /// Applies inference to the Input Graph and outputs the inferred information to the Output Graph
        /// </summary>
        /// <param name="input">Graph to apply inference to</param>
        /// <param name="output">Graph inferred information is output to</param>
        public virtual void Apply(IGraph input, IGraph output)
        {
            List<Triple> inferences = new List<Triple>();
            lock (this._conceptMappings)
            {
                foreach (Triple t in input.Triples)
                {
                    if (!(t.Predicate.Equals(this._skosBroader) || t.Predicate.Equals(this._skosNarrower)) && this._conceptMappings.ContainsKey(t.Object))
                    {
                        INode concept = t.Object;
                        while (this._conceptMappings.ContainsKey(concept))
                        {
                            if (this._conceptMappings[concept] != null)
                            {
                                //Assert additional information
                                inferences.Add(new Triple(t.Subject, t.Predicate, this._conceptMappings[concept]));
                                concept = this._conceptMappings[concept];
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
            lock (this._conceptMappings)
            {
                foreach (Triple t in g.Triples)
                {
                    if (t.Predicate.Equals(this._rdfType) && t.Object.Equals(this._skosConcept))
                    {
                        //Defines a SKOS Concept
                        if (!this._conceptMappings.ContainsKey(t.Subject))
                        {
                            this._conceptMappings.Add(t.Subject, null);
                        }
                    }
                    else if (t.Predicate.Equals(this._skosNarrower))
                    {
                        //Links a SKOS Concept to a child concept
                        if (!this._conceptMappings.ContainsKey(t.Object))
                        {
                            this._conceptMappings.Add(t.Object, t.Subject);
                        }
                        else if (this._conceptMappings[t.Object] == null)
                        {
                            this._conceptMappings[t.Object] = t.Subject;
                        }
                    }
                    else if (t.Predicate.Equals(this._skosBroader))
                    {
                        //Links a SKOS Concept to a parent concept
                        if (!this._conceptMappings.ContainsKey(t.Subject))
                        {
                            this._conceptMappings.Add(t.Subject, t.Object);
                        }
                        else if (this._conceptMappings[t.Subject] == null)
                        {
                            this._conceptMappings[t.Subject] = t.Object;
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
            //Use this Graph to further initialise the Reasoner
            this.Initialise(input);

            //Use Base Reasoner to do the Inference
            base.Apply(input, output);
        }
    }
}
