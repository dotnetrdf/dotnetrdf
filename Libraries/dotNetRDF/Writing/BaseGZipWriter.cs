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
using System.IO;
using System.IO.Compression;

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
        : BaseRdfWriter
    {
        private IRdfWriter _writer;

        /// <summary>
        /// Creates a new GZipped writer
        /// </summary>
        /// <param name="writer">Underlying writer</param>
        /// <exception cref="ArgumentNullException">raised if <paramref name="writer"/> is null</exception>
        protected BaseGZipWriter(IRdfWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _writer.Warning += RaiseWarning;
        }

        /// <summary>
        /// Saves a Graph as GZipped output
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">File to save to</param>
        public override void Save(IGraph g, string filename)
        {
            if (filename == null) throw new RdfOutputException("Cannot write RDF to a null file");
            Save(g, new StreamWriter(new GZipStream(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write), CompressionMode.Compress)));
        }


        /// <summary>
        /// Saves a Graph as GZipped output
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Writer to save to</param>
        protected override void SaveInternal(IGraph g, TextWriter output)
        {
            if (g == null) throw new RdfOutputException("Cannot write RDF from a null Graph");
            // Check for inner GZipStream and re-wrap if required

            if (output is StreamWriter streamOutput)
            {
                if (streamOutput.BaseStream is GZipStream)
                {
                    _writer.Save(g, streamOutput);
                }
                else
                {
                    streamOutput = new StreamWriter(new GZipStream(streamOutput.BaseStream, CompressionMode.Compress));
                    _writer.Save(g, streamOutput);
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
        private void RaiseWarning(string message)
        {
            Warning?.Invoke(message);
        }

        /// <summary>
        /// Event which is raised if non-fatal errors occur writing RDF output
        /// </summary>
        public override event RdfWriterWarning Warning;

        /// <summary>
        /// Gets the description of the writer
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "GZipped " + _writer.ToString();
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

    /// <summary>
    /// Writer for GZipped JSON-LD
    /// </summary>
    public class GZippedJsonLdWriter : BaseGZipDatasetWriter
    {
        /// <summary>
        /// Create a new GZippedJsonLdWriter
        /// </summary>
        public GZippedJsonLdWriter() : base(new JsonLdWriter())
        {
        }

        /// <summary>
        /// Create a new GZippedJsonLdWriter with a specific set of 
        /// <see cref="JsonLdWriterOptions"/>
        /// </summary>
        /// <param name="writerOptions">The writer options to pass through
        /// to the underlying <see cref="JsonLdWriter"/></param>
        public GZippedJsonLdWriter(JsonLdWriterOptions writerOptions) : base(new JsonLdWriter(writerOptions))
        {
        }
    }
}
