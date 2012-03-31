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
using System.IO.Compression;
using System.IO;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Abstract Base class for Results parser that read GZipped input
    /// </summary>
    /// <remarks>
    /// <para>
    /// While the normal parsers can be used with GZip streams directly this class just abstracts the wrapping of file/stream input into a GZip stream if it is not already passed as such
    /// </para>
    /// </remarks>
    public abstract class BaseGZipResultsParser
        : ISparqlResultsReader
    {
        private ISparqlResultsReader _parser;

        /// <summary>
        /// Creates a new GZipped results parser
        /// </summary>
        /// <param name="parser">Underlying parser</param>
        public BaseGZipResultsParser(ISparqlResultsReader parser)
        {
            if (parser == null) throw new ArgumentNullException("parser");
            this._parser = parser;
            this._parser.Warning += this.RaiseWarning;
        }

        /// <summary>
        /// Loads a Result Set from GZipped input
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="input">Input to load from</param>
        public void Load(SparqlResultSet results, StreamReader input)
        {
            if (results == null) throw new RdfParseException("Cannot parse SPARQL Results into a null Result Set");
            this.Load(new ResultSetHandler(results), input);
        }

        /// <summary>
        /// Loads a Result Set from GZipped input
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="input">Input to load from</param>
        public void Load(SparqlResultSet results, TextReader input)
        {
            if (results == null) throw new RdfParseException("Cannot parse SPARQL Results into a null Result Set");
            this.Load(new ResultSetHandler(results), input);
        }

        /// <summary>
        /// Loads a Result Set from GZipped input
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="filename">File to load from</param>
        public void Load(SparqlResultSet results, string filename)
        {
            if (results == null) throw new RdfParseException("Cannot parse SPARQL Results into a null Result Set");
            this.Load(new ResultSetHandler(results), filename);
        }

        /// <summary>
        /// Loads a Result Set from GZipped input
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="input">Input to load from</param>
        public void Load(ISparqlResultsHandler handler, StreamReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot parse SPARQL Results using a null Handler");
            if (input == null) throw new RdfParseException("Cannot parse SPARQL Results from a null input");

            if (input.BaseStream is GZipStream)
            {
                this._parser.Load(handler, input);
            }
            else
            {
                //Force the inner stream to be GZipped
                input = new StreamReader(new GZipStream(input.BaseStream, CompressionMode.Decompress));
                this._parser.Load(handler, input);
            }
        }

        /// <summary>
        /// Loads a Result Set from GZipped input
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="input">Input to load from</param>
        public void Load(ISparqlResultsHandler handler, TextReader input)
        {
            if (input is StreamReader)
            {
                this.Load(handler, (StreamReader)input);
            }
            else
            {
                throw new RdfParseException("GZipped input can only be parsed from StreamReader instances");
            }
        }

        /// <summary>
        /// Loads a Result Set from GZipped input
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="filename">File to load from</param>
        public void Load(ISparqlResultsHandler handler, string filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parse SPARQL Results from a null file");
            this.Load(handler, new StreamReader(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));
        }

        /// <summary>
        /// Gets the description of the parser
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "GZipped " + this._parser.ToString();
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
        /// Event which is raised if non-fatal errors are countered with parsing results
        /// </summary>
        public event SparqlWarning Warning;
    }

    /// <summary>
    /// Parser for GZipped SPARQL XML
    /// </summary>
    public class GZippedSparqlXmlParser
        : BaseGZipResultsParser
    {
        /// <summary>
        /// Creates a new GZipped SPARQL XML parser
        /// </summary>
        public GZippedSparqlXmlParser()
            : base(new SparqlXmlParser()) { }
    }

    /// <summary>
    /// Parser for GZipped SPARQL JSON
    /// </summary>
    public class GZippedSparqlJsonParser
        : BaseGZipResultsParser
    {
        /// <summary>
        /// Creates a new GZipped SPARQL JSON parser
        /// </summary>
        public GZippedSparqlJsonParser()
            : base(new SparqlJsonParser()) { }
    }

    /// <summary>
    /// Parser for GZipped SPARQL CSV
    /// </summary>
    public class GZippedSparqlCsvParser
        : BaseGZipResultsParser
    {
        /// <summary>
        /// Creates a new GZipped SPARQL CSV parser
        /// </summary>
        public GZippedSparqlCsvParser()
            : base(new SparqlCsvParser()) { }
    }

    /// <summary>
    /// Parser for GZipped SPARQL TSV
    /// </summary>
    public class GZippedSparqlTsvParser
        : BaseGZipResultsParser
    {
        /// <summary>
        /// Creates a new GZipped SPARQL TSV parser
        /// </summary>
        public GZippedSparqlTsvParser()
            : base(new SparqlTsvParser()) { }
    }
}

#endif