using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Writing
{
    public abstract class BaseGZipDatasetWriter
        : IStoreWriter
    {
        private IStoreWriter _writer;

        public BaseGZipDatasetWriter(IStoreWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            this._writer = writer;
        }

        public void Save(ITripleStore store, IStoreParams parameters)
        {
            if (store == null) throw new RdfOutputException("Cannot output a new Triple Store");
            if (parameters == null) throw new RdfOutputException("Cannot output using null parameters");

            if (parameters is StreamParams)
            {
                StreamParams sp = (StreamParams)parameters;
                StreamWriter output = sp.StreamWriter;

                if (output.BaseStream is GZipStream)
                {
                    this._writer.Save(store, sp);
                }
                else
                {
                    this._writer.Save(store, new StreamParams(new GZipStream(output.BaseStream, CompressionMode.Compress)));
                }
            }
            else
            {
                throw new RdfOutputException("GZip Dataset Writers can only write to StreamParams instances");
            }
        }

        public event StoreWriterWarning Warning;

        public override string ToString()
        {
            return "GZipped " + this._writer.ToString();
        }
    }

    public class GZippedNQuadsWriter
        : BaseGZipDatasetWriter
    {
        public GZippedNQuadsWriter()
            : base(new NQuadsWriter()) { }
    }

    public class GZippedTriGWriter
        : BaseGZipDatasetWriter
    {
        public GZippedTriGWriter()
            : base(new TriGWriter()) { }
    }

    public class GZippedTriXWriter
        : BaseGZipDatasetWriter
    {
        public GZippedTriXWriter()
            : base(new TriXWriter()) { }
    }
}
