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
using SemWeb;

namespace VDS.RDF.Interop.SemWeb
{
    /// <summary>
    /// Represents a mapping of a Graph to and from SemWeb
    /// </summary>
    public class SemWebMapping
    {
        private IGraph _g;
        private Dictionary<String, INode> _inputMapping = new Dictionary<string, INode>();
        private Dictionary<INode, Entity> _outputMapping = new Dictionary<INode, Entity>();

        /// <summary>
        /// Creates a new SemWeb mapping
        /// </summary>
        /// <param name="g">Graph</param>
        public SemWebMapping(IGraph g)
        {
            this._g = g;
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
        /// Gets the mapping from SemWeb Blank Node Entities to dotNetRDF Blank Nodes
        /// </summary>
        public Dictionary<String, INode> InputMapping
        {
            get
            {
                return this._inputMapping;
            }
        }

        /// <summary>
        /// Gets the mapping from dotNetRDF Blank Nodes to SemWeb Blank Node Entities
        /// </summary>
        public Dictionary<INode, Entity> OutputMapping
        {
            get
            {
                return this._outputMapping;
            }
        }

    }
}
