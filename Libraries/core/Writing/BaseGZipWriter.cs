/*

Copyright Robert Vesse 2009-12
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

#if !NO_COMPRESSION

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Abstract base class for RDF writers that generate GZipped output
    /// </summary>
    /// <remarks>
    /// <para>
    /// While the normal witers can be used with GZip streams directly this class just abstracts the wrapping of file/stream output into a GZip stream if it is not already passed as such
    /// </para>
    /// </remarks>
    public abstract class BaseGZipWriter
        : IRdfWriter
    {
        private IRdfWriter _writer;

        /// <summary>
        /// Creates a new GZipped writer
        /// </summary>
        /// <param name="writer">Underlying writer</param>
        public BaseGZipWriter(IRdfWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            this._writer = writer;
            this._writer.Warning += this.RaiseWarning;
        }

        /// <summary>
        /// Saves a Graph as GZipped output
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(IGraph g, string filename)
        {
            if (filename == null) throw new RdfOutputException("Cannot write RDF to a null file");
            this.Save(g, new StreamWriter(new GZipStream(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write), CompressionMode.Compress)));
        }


        /// <summary>
        /// Saves a Graph as GZipped output
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Writer to save to</param>
        public void Save(IGraph g, TextWriter output)
        {
            if (g == null) throw new RdfOutputException("Cannot write RDF from a null Graph");

            if (output is StreamWriter)
            {
                //Check for inner GZipStream and re-wrap if required
                StreamWriter streamOutput = (StreamWriter)output;
                if (streamOutput.BaseStream is GZipStream)
                {
                    this._writer.Save(g, streamOutput);
                }
                else
                {
                    streamOutput = new StreamWriter(new GZipStream(streamOutput.BaseStream, CompressionMode.Compress));
                    this._writer.Save(g, streamOutput);
                }
            }
            else
            {
                throw new RdfOutputException("GZipped Output can only be written to StreamWriter instances");
            }
        }

        /// <summary>
        /// Helper method for raising warning events
        /// </summary>
        /// <param name="message">Warning message</param>
        private void RaiseWarning(String message)
        {
            RdfWriterWarning d = this.Warning;
            if (d != null) d(message);
        }

        /// <summary>
        /// Event which is raised if non-fatal errors occur writing RDF output
        /// </summary>
        public event RdfWriterWarning Warning;

        /// <summary>
        /// Gets the description of the writer
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "GZipped " + this._writer.ToString();
        }
    }

    /// <summary>
    /// Writer for GZipped NTriples
    /// </summary>
    public class GZippedNTriplesWriter
        : BaseGZipWriter
    {
        /// <summary>
        /// Creates a new GZipped NTriples writer
        /// </summary>
        public GZippedNTriplesWriter()
            : base(new NTriplesWriter()) { }
    }

    /// <summary>
    /// Writer for GZipped Turtle
    /// </summary>
    public class GZippedTurtleWriter
        : BaseGZipWriter
    {
        /// <summary>
        /// Creates a new GZipped Turtle writer
        /// </summary>
        public GZippedTurtleWriter()
            : base(new CompressingTurtleWriter()) { }
    }

    /// <summary>
    /// Writer for GZipped Notation 3
    /// </summary>
    public class GZippedNotation3Writer
        : BaseGZipWriter
    {
        /// <summary>
        /// Creates a new GZipped Notation 3 writer
        /// </summary>
        public GZippedNotation3Writer()
            : base(new Notation3Writer()) { }
    }

    /// <summary>
    /// Writer for GZipped RDF/XML
    /// </summary>
    public class GZippedRdfXmlWriter
        : BaseGZipWriter
    {
        /// <summary>
        /// Creates a new GZipped RDF/XML writer
        /// </summary>
        public GZippedRdfXmlWriter()
            : base(new RdfXmlWriter()) { }
    }

    /// <summary>
    /// Writer for GZipped RDF/JSON
    /// </summary>
    public class GZippedRdfJsonWriter
        : BaseGZipWriter
    {
        /// <summary>
        /// Creates a new GZipped RDF/JSON writer
        /// </summary>
        public GZippedRdfJsonWriter()
            : base(new RdfJsonWriter()) { }
    }

    /// <summary>
    /// Writer for GZipped RDFa
    /// </summary>
    public class GZippedRdfAWriter
        : BaseGZipWriter
    {
        /// <summary>
        /// Creates a new GZipped RDFa writer
        /// </summary>
        public GZippedRdfAWriter()
            : base(new HtmlWriter()) { }
    }
}

#endif