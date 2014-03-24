/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

#if !NO_COMPRESSION

using System;
using System.IO.Compression;
using System.IO;
using VDS.RDF.Graphs;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Specifications;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Abstract Base class for RDF parsers which can read GZipped input
    /// </summary>
    /// <remarks>
    /// <para>
    /// While the normal parsers can be used with GZip streams directly this class just abstracts the wrapping of file/stream input into a GZip stream if it is not already passed as such
    /// </para>
    /// </remarks>
    public abstract class BaseGZipParser
        : IRdfReader
    {
        private readonly IRdfReader _parser;

        /// <summary>
        /// Creates a new GZipped input parser
        /// </summary>
        /// <param name="parser">Underlying parser</param>
        protected BaseGZipParser(IRdfReader parser)
        {
            if (parser == null) throw new ArgumentNullException("parser");
            this._parser = parser;
            this._parser.Warning += this.RaiseWarning;
        }

        /// <summary>
        /// Loads RDF using a RDF Handler from GZipped input
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Reader to load from</param>
        /// <param name="profile"></param>
        public void Load(IRdfHandler handler, TextReader input, IParserProfile profile)
        {
            if (handler == null) throw new RdfParseException("Cannot parse RDF using a null Handler");
            if (input == null) throw new RdfParseException("Cannot parse RDF from a null input");

            if (input is StreamReader)
            {
                StreamReader streamInput = (StreamReader) input;
                if (streamInput.BaseStream is GZipStream)
                {
                    this._parser.Load(handler, input, profile);
                }
                else
                {
                    //Force the inner stream to be GZipped
                    input = new StreamReader(new GZipStream(streamInput.BaseStream, CompressionMode.Decompress));
                    this._parser.Load(handler, input, profile);
                }
            }
            else
            {
                // Assume GZipped input
                this._parser.Load(handler, input, profile);
            }
        }

        /// <summary>
        /// Helper method for raising warning events
        /// </summary>
        /// <param name="message"></param>
        private void RaiseWarning(String message)
        {
            RdfReaderWarning d = this.Warning;
            if (d != null) d(message);
        }

        /// <summary>
        /// Warning event which is raised when non-fatal errors are encounted parsing RDF
        /// </summary>
        public event RdfReaderWarning Warning;

        /// <summary>
        /// Gets the description of the parser
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "GZipped " + this._parser.ToString();
        }
    }

    /// <summary>
    /// Parser for loading GZipped NTriples
    /// </summary>
    public class GZippedNTriplesParser
        : BaseGZipParser
    {
        /// <summary>
        /// Creates a new GZipped NTriples parser
        /// </summary>
        public GZippedNTriplesParser()
            : base(new NTriplesParser()) { }
    }

    /// <summary>
    /// Parser for loading GZipped Turtle
    /// </summary>
    public class GZippedTurtleParser
        : BaseGZipParser
    {
        /// <summary>
        /// Creates a new GZipped Turtle parser
        /// </summary>
        public GZippedTurtleParser()
            : base(new TurtleParser()) { }

        /// <summary>
        /// Creates a new GZipped Turtle parser
        /// </summary>
        /// <param name="syntax">Turtle Syntax</param>
        public GZippedTurtleParser(TurtleSyntax syntax)
            : base(new TurtleParser(syntax)) { }
    }

    /// <summary>
    /// Parser for loading GZipped Notation 3
    /// </summary>
    public class GZippedNotation3Parser
        : BaseGZipParser
    {
        /// <summary>
        /// Creates a new GZipped Notation 3 parser
        /// </summary>
        public GZippedNotation3Parser()
            : base(new Notation3Parser()) { }
    }

    /// <summary>
    /// Parser for loading GZipped RDF/XML
    /// </summary>
    public class GZippedRdfXmlParser
        : BaseGZipParser
    {
        /// <summary>
        /// Creates a new GZipped RDF/XML parser
        /// </summary>
        public GZippedRdfXmlParser()
            : base(new RdfXmlParser()) { }

        /// <summary>
        /// Creates a new GZipped RDF/XML parser
        /// </summary>
        /// <param name="mode">RDF/XML parser mode</param>
        public GZippedRdfXmlParser(RdfXmlParserMode mode)
            : base(new RdfXmlParser(mode)) { }
    }

    public class GZippedNQuadsParser
        : BaseGZipParser
    {
        public GZippedNQuadsParser()
            : base(new NQuadsParser()) { }
    }

    public class GZippedTriGParser
        : BaseGZipParser
    {
        public GZippedTriGParser()
            : base(new TriGParser()) { }
    }

    public class GZippedTriXParser
        : BaseGZipParser
    {
        public GZippedTriXParser()
            : base(new TriXParser()) { }
    }
}

#endif