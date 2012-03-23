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
    public abstract class BaseGZipResultsWriter
        : ISparqlResultsWriter
    {
        private ISparqlResultsWriter _writer;

        public BaseGZipResultsWriter(ISparqlResultsWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            this._writer = writer;
        }

        public void Save(SparqlResultSet results, string filename)
        {
            if (filename == null) throw new RdfOutputException("Cannot write RDF to a null file");
            this.Save(results, new StreamWriter(new GZipStream(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write), CompressionMode.Compress)));
        }

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

        public event SparqlWarning Warning;

        public override string ToString()
        {
            return "GZipped " + this._writer.ToString();
        }
    }

    public class GZippedSparqlXmlWriter
        : BaseGZipResultsWriter
    {
        public GZippedSparqlXmlWriter()
            : base(new SparqlXmlWriter()) { }
    }

    public class GZippedSparqlJsonWriter
        : BaseGZipResultsWriter
    {
        public GZippedSparqlJsonWriter()
            : base(new SparqlJsonWriter()) { }
    }

    public class GZippedSparqlCsvWriter
        : BaseGZipResultsWriter
    {
        public GZippedSparqlCsvWriter()
            : base(new SparqlCsvWriter()) { }
    }

    public class GZippedSparqlTsvWriter
        : BaseGZipResultsWriter
    {
        public GZippedSparqlTsvWriter()
            : base(new SparqlTsvWriter()) { }
    }
}

#endif