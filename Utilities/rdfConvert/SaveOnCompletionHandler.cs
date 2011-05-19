using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Utilities.Convert
{
    class SaveOnCompletionHandler : GraphHandler
    {
        private IRdfWriter _writer;
        private String _file;
        private TextWriter _textWriter;

        public SaveOnCompletionHandler(IRdfWriter writer, String file)
            : base(new Graph())
        {
            if (writer == null) throw new ArgumentNullException("writer", "Must specify a RDF Writer to use when the Handler completes RDF handling");
            if (file == null) throw new ArgumentNullException("file", "Cannot save RDF to a null file");
            this._writer = writer;
            this._file = file;
        }

        public SaveOnCompletionHandler(IRdfWriter writer, TextWriter textWriter)
            : base(new Graph())
        {
            if (writer == null) throw new ArgumentNullException("writer", "Must specify a RDF Writer to use when the Handler completes RDF handling");
            if (textWriter == null) throw new ArgumentNullException("textWriter", "Cannot save RDF to a null TextWriter");
            this._writer = writer;
            this._textWriter = textWriter;            
        }

        protected override void EndRdfInternal(bool ok)
        {
            base.EndRdfInternal(ok);

            if (ok)
            {
                if (this._textWriter != null)
                {
                    this._writer.Save(this.Graph, this._textWriter);
                }
                else
                {
                    this._writer.Save(this.Graph, this._file);
                }
            }
        }
    }

    class SaveStoreOnCompletionHandler : StoreHandler
    {
        private IStoreWriter _writer;
        private String _file;
        private TextWriter _textWriter;

        public SaveStoreOnCompletionHandler(IStoreWriter writer, String file)
            : base(new TripleStore())
        {
            if (writer == null) throw new ArgumentNullException("writer", "Must specify a RDF Dataset Writer to use when the Handler completes RDF handling");
            if (file == null) throw new ArgumentNullException("file", "Cannot save RDF to a null file");
            this._writer = writer;
            this._file = file;
        }

        public SaveStoreOnCompletionHandler(IStoreWriter writer, TextWriter textWriter)
            : base(new TripleStore())
        {
            if (writer == null) throw new ArgumentNullException("writer", "Must specify a RDF Dataset Writer to use when the Handler completes RDF handling");
            if (textWriter == null) throw new ArgumentNullException("textWriter", "Cannot save RDF to a null TextWriter");
            this._writer = writer;
            this._textWriter = textWriter;            
        }

        protected override void EndRdfInternal(bool ok)
        {
            base.EndRdfInternal(ok);

            if (ok)
            {
                if (this._textWriter != null)
                {
                    this._writer.Save(this.Store, new TextWriterParams(this._textWriter));
                }
                else
                {
                    this._writer.Save(this.Store, new StreamParams(this._file));
                }
            }
        }
    }
}
