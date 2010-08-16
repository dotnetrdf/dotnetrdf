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
using System.IO;

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// Writer Context for Turtle Writers
    /// </summary>
    public class TurtleWriterContext : BaseWriterContext
    {
        private BlankNodeOutputMapper _bnodeRemapper = new BlankNodeOutputMapper(WriterHelper.IsValidBlankNodeID);

        /// <summary>
        /// Creates a new Turtle Writer Context with default settings
        /// </summary>
        /// <param name="g">Graph to write</param>
        /// <param name="output">TextWriter to write to</param>
        public TurtleWriterContext(IGraph g, TextWriter output)
            : base(g, output) { }

        /// <summary>
        /// Creates a new Turtle Writer Context with custom settings
        /// </summary>
        /// <param name="g">Graph to write</param>
        /// <param name="output">TextWriter to write to</param>
        /// <param name="prettyPrint">Pretty Print Mode</param>
        /// <param name="hiSpeed">High Speed Mode</param>
        public TurtleWriterContext(IGraph g, TextWriter output, bool prettyPrint, bool hiSpeed)
            : base(g, output, WriterCompressionLevel.Default, prettyPrint, hiSpeed) { }

        /// <summary>
        /// Creates a new Turtle Writer Context with custom settings
        /// </summary>
        /// <param name="g">Graph to write</param>
        /// <param name="output">TextWriter to write to</param>
        /// <param name="compressionLevel">Compression Level to use</param>
        /// <param name="prettyPrint">Pretty Print Mode</param>
        /// <param name="hiSpeed">High Speed Mode</param>
        public TurtleWriterContext(IGraph g, TextWriter output, int compressionLevel, bool prettyPrint, bool hiSpeed)
            : base(g, output, compressionLevel, prettyPrint, hiSpeed) { }

        /// <summary>
        /// Gets the ID that should be used to output a Blank Node as Turtle
        /// </summary>
        /// <param name="id">Blank Node ID</param>
        /// <returns></returns>
        /// <remarks>
        /// If the Blank Node ID would not be valid in Turtle syntax it gets remapped appropriately
        /// </remarks>
        public String GetBlankNodeOutputID(String id)
        {
            return this._bnodeRemapper.GetOutputID(id);
        }

        /// <summary>
        /// Formats a Blank Node as a String for Turtle
        /// </summary>
        /// <param name="b">Blank Node to format</param>
        /// <param name="format">Format to output in</param>
        /// <returns></returns>
        protected override string FormatBlankNode(BlankNode b, NodeFormat format)
        {
            return "_:" + this.GetBlankNodeOutputID(b.InternalID);
        }
    }

    /// <summary>
    /// Writer Context for Compressing Turtle Writers
    /// </summary>
    public class CompressingTurtleWriterContext : TurtleWriterContext, ICollectionCompressingWriterContext
    {
        private Dictionary<INode, OutputRDFCollection> _collections = new Dictionary<INode, OutputRDFCollection>();
        private TripleCollection _triplesDone = new TripleCollection();

        /// <summary>
        /// Creates a new Turtle Writer Context with default settings
        /// </summary>
        /// <param name="g">Graph to write</param>
        /// <param name="output">TextWriter to write to</param>
        public CompressingTurtleWriterContext(IGraph g, TextWriter output)
            : base(g, output) { }

        /// <summary>
        /// Creates a new Turtle Writer Context with custom settings
        /// </summary>
        /// <param name="g">Graph to write</param>
        /// <param name="output">TextWriter to write to</param>
        /// <param name="prettyPrint">Pretty Print Mode</param>
        /// <param name="hiSpeed">High Speed Mode</param>
        public CompressingTurtleWriterContext(IGraph g, TextWriter output, bool prettyPrint, bool hiSpeed)
            : base(g, output, WriterCompressionLevel.Default, prettyPrint, hiSpeed) { }

        /// <summary>
        /// Creates a new Turtle Writer Context with custom settings
        /// </summary>
        /// <param name="g">Graph to write</param>
        /// <param name="output">TextWriter to write to</param>
        /// <param name="compressionLevel">Compression Level to use</param>
        /// <param name="prettyPrint">Pretty Print Mode</param>
        /// <param name="hiSpeed">High Speed Mode</param>
        public CompressingTurtleWriterContext(IGraph g, TextWriter output, int compressionLevel, bool prettyPrint, bool hiSpeed)
            : base(g, output, compressionLevel, prettyPrint, hiSpeed) { }

        /// <summary>
        /// Represents the mapping from Blank Nodes to Collections
        /// </summary>
        public Dictionary<INode, OutputRDFCollection> Collections
        {
            get
            {
                return this._collections;
            }
        }

        /// <summary>
        /// Stores the Triples that should be excluded from standard output as they are part of collections
        /// </summary>
        public BaseTripleCollection TriplesDone
        {
            get
            {
                return this._triplesDone;
            }
        }
    }
}
