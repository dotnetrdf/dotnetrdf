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
using VDS.RDF.Query;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Abstract Base class for Results writers which generate GZipped output
    /// </summary>
    /// <remarks>
    /// <para>
    /// While the normal witers can be used with GZip streams directly this class just abstracts the wrapping of file/stream output into a GZip stream if it is not already passed as such
    /// </para>
    /// </remarks>
    public abstract class BaseGZipResultsWriter
        : ISparqlResultsWriter
    {
        private ISparqlResultsWriter _writer;

        /// <summary>
        /// Creates a new GZipped Results writer
        /// </summary>
        /// <param name="writer">Underlying writer</param>
        public BaseGZipResultsWriter(ISparqlResultsWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            _writer = writer;
            _writer.Warning += RaiseWarning;
        }

        /// <summary>
        /// Saves a Result Set as GZipped output
        /// </summary>
        /// <param name="results">Result Set to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(SparqlResultSet results, string filename)
        {
            if (filename == null) throw new RdfOutputException("Cannot write RDF to a null file");
            Save(results, new StreamWriter(new GZipStream(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write), CompressionMode.Compress)));
        }

        /// <summary>
        /// Saves a Result Set as GZipped output
        /// </summary>
        /// <param name="results">Result Set to save</param>
        /// <param name="output">Writer to save to</param>
        public void Save(SparqlResultSet results, TextWriter output)
        {
            if (results == null) throw new RdfOutputException("Cannot write RDF from a null Graph");

            if (output is StreamWriter)
            {
                // Check for inner GZipStream and re-wrap if required
                StreamWriter streamOutput = (StreamWriter)output;
                if (streamOutput.BaseStream is GZipStream)
                {
                    _writer.Save(results, streamOutput);
                }
                else
                {
                    streamOutput = new StreamWriter(new GZipStream(streamOutput.BaseStream, CompressionMode.Compress));
                    _writer.Save(results, streamOutput);
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
            SparqlWarning d = Warning;
            if (d != null) d(message);
        }

        /// <summary>
        /// Event which is raised if non-fatal errors occur writing results
        /// </summary>
        public event SparqlWarning Warning;

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
    /// Writer for GZipped SPARQL XML
    /// </summary>
    public class GZippedSparqlXmlWriter
        : BaseGZipResultsWriter
    {
        /// <summary>
        /// Creates a new GZipped SPARQL XML writer
        /// </summary>
        public GZippedSparqlXmlWriter()
            : base(new SparqlXmlWriter()) { }
    }

    /// <summary>
    /// Writer for GZipped SPARQL JSON
    /// </summary>
    public class GZippedSparqlJsonWriter
        : BaseGZipResultsWriter
    {
        /// <summary>
        /// Creates a new GZipped SPARQL JSON writer
        /// </summary>
        public GZippedSparqlJsonWriter()
            : base(new SparqlJsonWriter()) { }
    }

    /// <summary>
    /// Writer for GZipped SPARQL CSV
    /// </summary>
    public class GZippedSparqlCsvWriter
        : BaseGZipResultsWriter
    {
        /// <summary>
        /// Creates a new GZipped SPARQL CSV writer
        /// </summary>
        public GZippedSparqlCsvWriter()
            : base(new SparqlCsvWriter()) { }
    }

    /// <summary>
    /// Writer for GZipped SPARQL TSV
    /// </summary>
    public class GZippedSparqlTsvWriter
        : BaseGZipResultsWriter
    {
        /// <summary>
        /// Creates a new GZipped SPARQL TSV writer
        /// </summary>
        public GZippedSparqlTsvWriter()
            : base(new SparqlTsvWriter()) { }
    }
}
