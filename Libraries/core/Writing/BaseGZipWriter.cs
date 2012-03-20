using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace VDS.RDF.Writing
{
    public abstract class BaseGZipWriter
        : IRdfWriter
    {
        private IRdfWriter _writer;

        public BaseGZipWriter(IRdfWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            this._writer = writer;
        }

        public void Save(IGraph g, string filename)
        {
            if (filename == null) throw new RdfOutputException("Cannot write RDF to a null file");
            this.Save(g, new StreamWriter(new GZipStream(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write), CompressionMode.Compress)));
        }

        public void Save(IGraph g, TextWriter output)
        {
            if (g == null) throw new RdfOutputException("Cannot write RDF from a null Graph");

            if (output is StreamWriter)
            {
                //Check for inner GZipStream and re-wrap if required
                StreamWriter streamOutput = (StreamWriter)output;
                if (streamOutput.BaseStream is GZipStream)
                {
                    this._writer.Save(g, streamOutput);
                }
                else
                {
                    streamOutput = new StreamWriter(new GZipStream(streamOutput.BaseStream, CompressionMode.Compress));
                    this._writer.Save(g, streamOutput);
                }
            }
            else
            {
                throw new RdfOutputException("GZipped Output can only be written to StreamWriter instances");
            }
        }

        public event RdfWriterWarning Warning;

        public override string ToString()
        {
            return "GZipped " + this._writer.ToString();
        }
    }

    public class GZippedNTriplesWriter
        : BaseGZipWriter
    {
        public GZippedNTriplesWriter()
            : base(new NTriplesWriter()) { }
    }

    public class GZippedTurtleWriter
        : BaseGZipWriter
    {
        public GZippedTurtleWriter()
            : base(new CompressingTurtleWriter()) { }
    }

    public class GZippedNotation3Writer
        : BaseGZipWriter
    {
        public GZippedNotation3Writer()
            : base(new Notation3Writer()) { }
    }

    public class GZippedRdfXmlWriter
        : BaseGZipWriter
    {
        public GZippedRdfXmlWriter()
            : base(new RdfXmlWriter()) { }
    }

    public class GZippedRdfJsonWriter
        : BaseGZipWriter
    {
        public GZippedRdfJsonWriter()
            : base(new RdfJsonWriter()) { }
    }

    public class GZippedRdfAWriter
        : BaseGZipWriter
    {
        public GZippedRdfAWriter()
            : base(new HtmlWriter()) { }
    }
}
