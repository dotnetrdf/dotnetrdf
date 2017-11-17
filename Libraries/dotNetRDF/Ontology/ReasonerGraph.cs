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
using VDS.RDF.Query.Inference;

namespace VDS.RDF.Ontology
{
    /// <summary>
    /// Represents a Graph with a reasoner attached
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class wraps an existing Graph and applies the given reasoner to it materialising the Triples in this Graph.  The original Graph itself is not modified but can be accessed if necessary using the <see cref="BaseGraph">BaseGraph</see> property
    /// </para>
    /// <para>
    /// Any changes to this Graph (via <see cref="IGraph.Assert(Triple)">Assert()</see> and <see cref="IGraph.Retract(Triple)">Retract()</see>) affect this Graph - specifically the set of materialised Triples - rather than the original Graph around which this Graph is a wrapper
    /// </para>
    /// <para>
    /// See <a href="http://www.dotnetrdf.org/content.asp?pageID=Ontology%20API">Using the Ontology API</a> for some informal documentation on the use of the Ontology namespace
    /// </para>
    /// </remarks>
    public class ReasonerGraph 
        : OntologyGraph
    {
        private List<IInferenceEngine> _reasoners = new List<IInferenceEngine>();
        private IGraph _baseGraph;

        /// <summary>
        /// Creates a new Reasoner Graph which is a wrapper around an existing Graph with a reasoner applied and the resulting Triples materialised
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="reasoner">Reasoner</param>
        public ReasonerGraph(IGraph g, IInferenceEngine reasoner)
        {
            _baseGraph = g;
            _reasoners.Add(reasoner);
            Initialise();
        }

        /// <summary>
        /// Creates a new Reasoner Graph which is a wrapper around an existing Graph with multiple reasoners applied and the resulting Triples materialised
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="reasoners">Reasoner</param>
        public ReasonerGraph(IGraph g, IEnumerable<IInferenceEngine> reasoners)
        {
            _baseGraph = g;
            _reasoners.AddRange(reasoners);
            Initialise();
        }

        /// <summary>
        /// Internal method which initialises the Graph by applying the reasoners and setting the Node and Triple collections to be union collections
        /// </summary>
        private void Initialise()
        {
            // Apply the reasoners
            foreach (IInferenceEngine reasoner in _reasoners)
            {
                reasoner.Apply(_baseGraph, this);
            }
            // Set the Triple and Node Collections to be Union Collections
            _triples = new UnionTripleCollection(_triples, _baseGraph.Triples);
        }

        /// <summary>
        /// Gets the Base Graph which the reasoning is based upon
        /// </summary>
        public IGraph BaseGraph
        {
            get
            {
                return _baseGraph;
            }
        }
    }
}
