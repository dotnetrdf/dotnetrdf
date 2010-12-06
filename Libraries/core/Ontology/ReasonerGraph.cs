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
    /// Any changes to this Graph (via <see cref="IGraph.Assert">Assert()</see> and <see cref="IGraph.Retract">Retract()</see>) affect this Graph - specifically the set of materialised Triples - rather than the original Graph around which this Graph is a wrapper
    /// </para>
    /// <para>
    /// See <a href="http://www.dotnetrdf.org/content.asp?pageID=Ontology%20API">Using the Ontology API</a> for some informal documentation on the use of the Ontology namespace
    /// </para>
    /// </remarks>
    public class ReasonerGraph : OntologyGraph
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
            this._baseGraph = g;
            this._reasoners.Add(reasoner);
            this.Initialise();
        }

        /// <summary>
        /// Creates a new Reasoner Graph which is a wrapper around an existing Graph with multiple reasoners applied and the resulting Triples materialised
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="reasoners">Reasoner</param>
        public ReasonerGraph(IGraph g, IEnumerable<IInferenceEngine> reasoners)
        {
            this._baseGraph = g;
            this._reasoners.AddRange(reasoners);
            this.Initialise();
        }

        /// <summary>
        /// Internal method which initialises the Graph by applying the reasoners and setting the Node and Triple collections to be union collections
        /// </summary>
        private void Initialise()
        {
            //Apply the reasoners
            foreach (IInferenceEngine reasoner in this._reasoners)
            {
                reasoner.Apply(this._baseGraph, this);
            }
            //Set the Triple and Node Collections to be Union Collections
            this._triples = new UnionTripleCollection(this._triples, this._baseGraph.Triples);
            this._nodes = new UnionNodeCollection(this._nodes, this._baseGraph.Nodes);
        }

        /// <summary>
        /// Gets the Base Graph which the reasoning is based upon
        /// </summary>
        public IGraph BaseGraph
        {
            get
            {
                return this._baseGraph;
            }
        }
    }
}
