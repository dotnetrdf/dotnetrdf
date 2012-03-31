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
        private IRdfReader _parser;

        /// <summary>
        /// Creates a new GZipped input parser
        /// </summary>
        /// <param name="parser">Underlying parser</param>
        public BaseGZipParser(IRdfReader parser)
        {
            if (parser == null) throw new ArgumentNullException("parser");
            this._parser = parser;
            this._parser.Warning += this.RaiseWarning;
        }

        /// <summary>
        /// Loads a Graph from GZipped input
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="input">Stream to load from</param>
        public void Load(IGraph g, StreamReader input)
        {
            if (g == null) throw new RdfParseException("Cannot parse RDF into a null Graph");
            this.Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Loads a Graph from GZipped input
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="input">Reader to load from</param>
        public void Load(IGraph g, TextReader input)
        {
            if (g == null) throw new RdfParseException("Cannot parse RDF into a null Graph");
            this.Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Loads a Graph from GZipped input
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="filename">File to load from</param>
        public void Load(IGraph g, string filename)
        {
            if (g == null) throw new RdfParseException("Cannot parse RDF into a null Graph");
            this.Load(new GraphHandler(g), filename);
        }

        /// <summary>
        /// Loads RDF using a RDF Handler from GZipped input
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Stream to load from</param>
        public void Load(IRdfHandler handler, StreamReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot parse RDF using a null Handler");
            if (input == null) throw new RdfParseException("Cannot parse RDF from a null input");

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
        /// Loads RDF using a RDF Handler from GZipped input
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Reader to load from</param>
        public void Load(IRdfHandler handler, TextReader input)
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
        /// Loads RDF using a RDF Handler from GZipped input
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">File to load from</param>
        public void Load(IRdfHandler handler, string filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parse RDF from a null file");
            this.Load(handler, new StreamReader(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));
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
    }

    /// <summary>
    /// Parser for loading GZipped RDF/JSON
    /// </summary>
    public class GZippedRdfJsonParser
        : BaseGZipParser
    {
        /// <summary>
        /// Creates a new GZipped RDF/JSON parser
        /// </summary>
        public GZippedRdfJsonParser()
            : base(new RdfJsonParser()) { }
    }

    /// <summary>
    /// Parser for loading GZipped RDFa
    /// </summary>
    public class GZippedRdfAParser
        : BaseGZipParser
    {
        /// <summary>
        /// Creates a new GZipped RDFa parser
        /// </summary>
        public GZippedRdfAParser()
            : base(new RdfAParser()) { }
    }
}

#endif