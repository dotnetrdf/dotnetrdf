using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace Alexandria.Documents.Adaptors
{
    public abstract class RdfAdaptor : IDocumentToGraphAdaptor
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

        public virtual void ToGraph(IGraph g, IDocument document)
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

        public virtual void ToDocument(IGraph g, IDocument document)
        {
            try
            {
                this._writer.Save(g, document.BeginWrite());
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
    }
}
