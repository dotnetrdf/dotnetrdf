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

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Formatter for formatting as Notation 3 without any compression
    /// </summary>
    public class UncompressedNotation3Formatter : UncompressedTurtleFormatter
    {
        /// <summary>
        /// Creates a new Uncompressed Notation 3 Formatter
        /// </summary>
        public UncompressedNotation3Formatter()
            : base("Notation 3 (Uncompressed)") 
        {
            this._validEscapes = new char[] { '"', 'n', 't', 'u', 'U', 'r', '\\', '0', '\'' };
        }

        /// <summary>
        /// Formats a Variable Node for Notation 3
        /// </summary>
        /// <param name="v">Variable</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatVariableNode(IVariableNode v, TripleSegment? segment)
        {
            return v.ToString();
        }

        /// <summary>
        /// Formats a Graph Literal Node for Notation 3
        /// </summary>
        /// <param name="glit">Graph Literal</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatGraphLiteralNode(IGraphLiteralNode glit, TripleSegment? segment)
        {
            if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.GraphLiteralPredicatesUnserializable(this.FormatName));

            StringBuilder output = new StringBuilder();
            output.Append("{");
            foreach (Triple t in glit.SubGraph.Triples)
            {
                output.Append(this.Format(t));
            }
            output.Append("}");
            return output.ToString();
        }
    }

    /// <summary>
    /// Formatter for formatting as Notation 3
    /// </summary>
    public class Notation3Formatter : TurtleFormatter
    {
        /// <summary>
        /// Creates a new Notation 3 Formatter
        /// </summary>
        public Notation3Formatter()
            : base("Notation 3", new QNameOutputMapper()) 
        {
            this._validEscapes = new char[] { '"', 'n', 't', 'u', 'U', 'r', '\\', '0', '\'' };
        }

        /// <summary>
        /// Creates a new Notation 3 Formatter using the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public Notation3Formatter(IGraph g)
            : base("Notation 3", new QNameOutputMapper(g.NamespaceMap)) 
        {
            this._validEscapes = new char[] { '"', 'n', 't', 'u', 'U', 'r', '\\', '0', '\'' };
        }

        /// <summary>
        /// Creates a new Notation 3 Formatter using the given Namespace Map
        /// </summary>
        /// <param name="nsmap">Namespace Map</param>
        public Notation3Formatter(INamespaceMapper nsmap)
            : base("Notation 3", new QNameOutputMapper(nsmap)) 
        {
            this._validEscapes = new char[] { '"', 'n', 't', 'u', 'U', 'r', '\\', '0', '\'' };
        }

        /// <summary>
        /// Formats a Variable Node for Notation 3
        /// </summary>
        /// <param name="v">Variable</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatVariableNode(IVariableNode v, TripleSegment? segment)
        {
            return v.ToString();
        }

        /// <summary>
        /// Formats a Graph Literal Node for Notation 3
        /// </summary>
        /// <param name="glit">Graph Literal</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatGraphLiteralNode(IGraphLiteralNode glit, TripleSegment? segment)
        {
            if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.GraphLiteralPredicatesUnserializable(this.FormatName));

            StringBuilder output = new StringBuilder();
            output.Append("{");
            foreach (Triple t in glit.SubGraph.Triples)
            {
                output.Append(this.Format(t));
            }
            output.Append("}");
            return output.ToString();
        }
    }
}
