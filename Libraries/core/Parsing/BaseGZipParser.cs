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

using System;
using System.IO.Compression;
using System.IO;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing
{
    public abstract class BaseGZipParser
        : IRdfReader
    {
        private IRdfReader _parser;

        public BaseGZipParser(IRdfReader parser)
        {
            if (parser == null) throw new ArgumentNullException("parser");
            this._parser = parser;
        }

        public void Load(IGraph g, StreamReader input)
        {
            if (g == null) throw new RdfParseException("Cannot parse RDF into a null Graph");
            this.Load(new GraphHandler(g), input);
        }

        public void Load(IGraph g, TextReader input)
        {
            if (g == null) throw new RdfParseException("Cannot parse RDF into a null Graph");
            this.Load(new GraphHandler(g), input);
        }

        public void Load(IGraph g, string filename)
        {
            if (g == null) throw new RdfParseException("Cannot parse RDF into a null Graph");
            this.Load(new GraphHandler(g), filename);
        }

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

        public void Load(IRdfHandler handler, string filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parse RDF from a null file");
            this.Load(handler, new StreamReader(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));
        }

        public event RdfReaderWarning Warning;

        public override string ToString()
        {
            return "GZipped " + this._parser.ToString();
        }
    }

    public class GZippedNTriplesParser
        : BaseGZipParser
    {
        public GZippedNTriplesParser()
            : base(new NTriplesParser()) { }
    }

    public class GZippedTurtleParser
        : BaseGZipParser
    {
        public GZippedTurtleParser()
            : base(new TurtleParser()) { }
    }

    public class GZippedNotation3Parser
        : BaseGZipParser
    {
        public GZippedNotation3Parser()
            : base(new Notation3Parser()) { }
    }

    public class GZippedRdfXmlParser
        : BaseGZipParser
    {
        public GZippedRdfXmlParser()
            : base(new RdfXmlParser()) { }
    }

    public class GZippedRdfJsonParser
        : BaseGZipParser
    {
        public GZippedRdfJsonParser()
            : base(new RdfJsonParser()) { }
    }

    public class GZippedRdfAParser
        : BaseGZipParser
    {
        public GZippedRdfAParser()
            : base(new RdfAParser()) { }
    }
}
