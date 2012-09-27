/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
            this._writer = writer;
            this._writer.Warning += this.RaiseWarning;
        }

        /// <summary>
        /// Saves a Result Set as GZipped output
        /// </summary>
        /// <param name="results">Result Set to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(SparqlResultSet results, string filename)
        {
            if (filename == null) throw new RdfOutputException("Cannot write RDF to a null file");
            this.Save(results, new StreamWriter(new GZipStream(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write), CompressionMode.Compress)));
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
                //Check for inner GZipStream and re-wrap if required
                StreamWriter streamOutput = (StreamWriter)output;
                if (streamOutput.BaseStream is GZipStream)
                {
                    this._writer.Save(results, streamOutput);
                }
                else
                {
                    streamOutput = new StreamWriter(new GZipStream(streamOutput.BaseStream, CompressionMode.Compress));
                    this._writer.Save(results, streamOutput);
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
            SparqlWarning d = this.Warning;
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
            return "GZipped " + this._writer.ToString();
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

#endif