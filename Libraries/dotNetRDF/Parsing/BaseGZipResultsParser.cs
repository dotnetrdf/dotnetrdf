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
            _parser = parser;
            _parser.Warning += RaiseWarning;
        }

        /// <summary>
        /// Loads a Result Set from GZipped input
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="input">Input to load from</param>
        public void Load(SparqlResultSet results, StreamReader input)
        {
            if (results == null) throw new RdfParseException("Cannot parse SPARQL Results into a null Result Set");
            Load(new ResultSetHandler(results), input);
        }

        /// <summary>
        /// Loads a Result Set from GZipped input
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="input">Input to load from</param>
        public void Load(SparqlResultSet results, TextReader input)
        {
            if (results == null) throw new RdfParseException("Cannot parse SPARQL Results into a null Result Set");
            Load(new ResultSetHandler(results), input);
        }

        /// <summary>
        /// Loads a Result Set from GZipped input
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="filename">File to load from</param>
        public void Load(SparqlResultSet results, string filename)
        {
            if (results == null) throw new RdfParseException("Cannot parse SPARQL Results into a null Result Set");
            Load(new ResultSetHandler(results), filename);
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
                _parser.Load(handler, input);
            }
            else
            {
                // Force the inner stream to be GZipped
                input = new StreamReader(new GZipStream(input.BaseStream, CompressionMode.Decompress));
                _parser.Load(handler, input);
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
                Load(handler, (StreamReader)input);
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
            Load(handler, new StreamReader(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));
        }

        /// <summary>
        /// Gets the description of the parser
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "GZipped " + _parser.ToString();
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
