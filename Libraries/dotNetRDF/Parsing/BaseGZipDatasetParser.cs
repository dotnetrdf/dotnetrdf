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
using VDS.RDF.JsonLd;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Abstract Base Class for parsers that handle GZipped input
    /// </summary>
    /// <remarks>
    /// <para>
    /// While the normal parsers can be used with GZip streams directly this class just abstracts the wrapping of file/stream input into a GZip stream if it is not already passed as such
    /// </para>
    /// </remarks>
    public abstract class BaseGZipDatasetParser
        : IStoreReader
    {
        private IStoreReader _parser;

        /// <summary>
        /// Creates a new GZipped input parser
        /// </summary>
        /// <param name="parser">The underlying parser to use</param>
        public BaseGZipDatasetParser(IStoreReader parser)
        {
            if (parser == null) throw new ArgumentNullException("parser");
            _parser = parser;
            _parser.Warning += RaiseWarning;
        }

        /// <summary>
        /// Loads a RDF dataset from GZipped input
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="filename">File to load from</param>
        public void Load(ITripleStore store, String filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null file");
            Load(store, new StreamReader(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));
        }

        /// <summary>
        /// Loads a RDF dataset from GZipped input
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="input">Input to load from</param>
        public void Load(ITripleStore store, TextReader input)
        {
            if (store == null) throw new RdfParseException("Cannot parse an RDF Dataset into a null store");
            if (input == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null input");
            Load(new StoreHandler(store), input);
        }

        /// <summary>
        /// Loads a RDF dataset from GZipped input
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">File to load from</param>
        public void Load(IRdfHandler handler, String filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null file");
            Load(handler, new StreamReader(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));
        }

        /// <summary>
        /// Loads a RDF dataset from GZipped input
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Input to load from</param>
        public void Load(IRdfHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot parse an RDF Dataset using a null handler");
            if (input == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null input");

            if (input is StreamReader)
            {
                StreamReader reader = (StreamReader)input;
                if (reader.BaseStream is GZipStream)
                {
                    _parser.Load(handler, input);
                }
                else
                {
                    // Force the inner stream to be GZipped
                    _parser.Load(handler, new StreamReader(new GZipStream(reader.BaseStream, CompressionMode.Decompress)));
                }
            }
            else
            {
                throw new RdfParseException("GZip Dataset Parsers can only read from StreamReader instances");
            }
        }

        /// <summary>
        /// Warning Event raised on non-fatal errors encountered parsing
        /// </summary>
        public event StoreReaderWarning Warning;

        /// <summary>
        /// Helper method for raising warning events
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            StoreReaderWarning d = Warning;
            if (d != null) d(message);
        }

        /// <summary>
        /// Gets the description of the parser
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "GZipped " + _parser.ToString();
        }
    }

    /// <summary>
    /// Parser for loading GZipped NQuads
    /// </summary>
    public class GZippedNQuadsParser
        : BaseGZipDatasetParser
    {
        /// <summary>
        /// Creates a new GZipped NQuads Parser
        /// </summary>
        public GZippedNQuadsParser()
            : base(new NQuadsParser()) { }
    }

    /// <summary>
    /// Parser for loading GZipped TriG
    /// </summary>
    public class GZippedTriGParser
        : BaseGZipDatasetParser
    {
        /// <summary>
        /// Creates a new GZipped TriG Parser
        /// </summary>
        public GZippedTriGParser()
            : base(new TriGParser()) { }
    }

    /// <summary>
    /// Parser for loading GZipped TriX
    /// </summary>
    public class GZippedTriXParser
        : BaseGZipDatasetParser
    {
        /// <summary>
        /// Creates a new GZipped TriX Parser
        /// </summary>
        public GZippedTriXParser()
            : base(new TriXParser()) { }
    }

    /// <summary>
    /// Parser for oading GZipped JSON-LD
    /// </summary>
    public class GZippedJsonLdParser : BaseGZipDatasetParser
    {
        /// <summary>
        /// Creates a new GZipped JSON-LD parser
        /// </summary>
        public GZippedJsonLdParser():base(new JsonLdParser()) { }

        /// <summary>
        /// Creates a new GZipped JSON-LD parser with a specific set of <see cref="JsonLdProcessorOptions"/>.
        /// </summary>
        /// <param name="parserOptions">The options to pass to the underlying <see cref="JsonLdParser"/></param>
        public GZippedJsonLdParser(JsonLdProcessorOptions parserOptions):base(new JsonLdParser(parserOptions)) { }
    }
}
