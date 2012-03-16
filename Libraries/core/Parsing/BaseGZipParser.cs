using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
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
