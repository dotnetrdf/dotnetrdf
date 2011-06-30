using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.Alexandria.Documents.Adaptors
{
    public abstract class RdfAdaptor : IDataAdaptor<StreamReader, TextWriter>
    {
        private IRdfReader _parser;
        private IRdfWriter _writer;

        public RdfAdaptor(IRdfReader parser, IRdfWriter writer)
        {
            if (parser == null) throw new ArgumentNullException("parser", "Cannot use a null parser for an RDF Adaptor");
            if (writer == null) throw new ArgumentNullException("writer", "Cannot use a null writer for an RDF Adaptor");
            this._parser = parser;
            this._writer = writer;
        }

        public virtual void ToGraph(IGraph g, IDocument<StreamReader,TextWriter> document)
        {
            if (document.Exists)
            {
                try
                {
                    this._parser.Load(g, document.BeginRead());
                    document.EndRead();
                }
                catch (AlexandriaException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    document.EndRead();
                    throw new AlexandriaException("Error reading Document " + document.Name + " into a Graph", ex);
                }
            }
        }

        public virtual void ToHandler(IRdfHandler handler, IDocument<StreamReader, TextWriter> document)
        {
            if (document.Exists)
            {
                try
                {
                    this._parser.Load(handler, document.BeginRead());
                    document.EndRead();
                }
                catch (AlexandriaException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    document.EndRead();
                    throw new AlexandriaException("Error reading Document " + document.Name + " with a RDF Handler", ex);
                }
            }
        }

        public virtual void ToDocument(IGraph g, IDocument<StreamReader,TextWriter> document)
        {
            try
            {
                this._writer.Save(g, document.BeginWrite(false));
                document.EndWrite();
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                document.EndWrite();
                throw new AlexandriaException("Error writing Graph to Document " + document.Name, ex);
            }
        }

        public abstract void AppendTriples(IEnumerable<Triple> ts, IDocument<StreamReader,TextWriter> document);

        public abstract void DeleteTriples(IEnumerable<Triple> ts, IDocument<StreamReader,TextWriter> document);
    }
}
