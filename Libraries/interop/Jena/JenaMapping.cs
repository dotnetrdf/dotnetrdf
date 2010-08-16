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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/


using System;
using System.Collections.Generic;
using com.hp.hpl.jena.rdf.model;

namespace VDS.RDF.Interop.Jena
{
    /// <summary>
    /// Represents a mapping of a Graph to and from Jena
    /// </summary>
    public class JenaMapping
    {
        private IGraph _g;
        private Model _m;
        private Dictionary<Resource, INode> _inputMapping = new Dictionary<Resource, INode>();
        private Dictionary<INode, Resource> _outputMapping = new Dictionary<INode, Resource>();

        /// <summary>
        /// Creates a new Jena mapping
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="m">Model</param>
        public JenaMapping(IGraph g, Model m)
        {
            this._g = g;
            this._m = m;
        }

        /// <summary>
        /// Gets the Graph this is a mapping for
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return this._g;
            }
        }

        /// <summary>
        /// Gets the Model that this is a mapping for
        /// </summary>
        public Model Model
        {
            get
            {
                return this._m;
            }
        }

        /// <summary>
        /// Gets the mapping from Jena Blank Node to dotNetRDF Blank Nodes
        /// </summary>
        public Dictionary<Resource, INode> InputMapping
        {
            get
            {
                return this._inputMapping;
            }
        }

        /// <summary>
        /// Gets the mapping from dotNetRDF Blank Nodes to Jena Blank Nodes
        /// </summary>
        public Dictionary<INode, Resource> OutputMapping
        {
            get
            {
                return this._outputMapping;
            }
        }

    }
}
