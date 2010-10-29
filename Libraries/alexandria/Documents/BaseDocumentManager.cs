using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VDS.RDF;
using VDS.Alexandria.Documents.Adaptors;
using VDS.Alexandria.Documents.GraphRegistry;
using VDS.Alexandria.Utilities;

namespace VDS.Alexandria.Documents
{
    public abstract class BaseDocumentManager<TReader,TWriter> : IDocumentManager<TReader,TWriter>
    {
        private Dictionary<String,DocumentReference<TReader,TWriter>> _activeDocuments = new Dictionary<string,DocumentReference<TReader,TWriter>>();
        private IDataAdaptor<TReader, TWriter> _adaptor;
        private GraphFactory _nodeFactory = new GraphFactory();

        public BaseDocumentManager(IDataAdaptor<TReader,TWriter> adaptor)
        {
            this._adaptor = adaptor;
        }

        public IDataAdaptor<TReader,TWriter> DataAdaptor
        {
            get
            {
                return this._adaptor;
            }
            protected set 
            {
                this._adaptor = value;
            }
        }

        public GraphFactory GraphFactory
        {
            get
            {
                return this._nodeFactory;
            }
        }

        public bool HasDocument(string name)
        {
            if (this._activeDocuments.ContainsKey(name))
            {
                return true;
            }
            else
            {
                return this.HasDocumentInternal(name);
            }
        }

        protected abstract bool HasDocumentInternal(String name);

        public bool CreateDocument(string name)
        {
            if (this.HasDocument(name))
            {
                return false;
            }
            else
            {
                return this.CreateDocumentInternal(name);
            }
        }

        protected abstract bool CreateDocumentInternal(String name);

        public bool DeleteDocument(string name)
        {
            if (this._activeDocuments.ContainsKey(name))
            {
                if (this._activeDocuments[name].ReferenceCount > 0)
                {
                    return false;
                }
            }

            return this.DeleteDocumentInternal(name);
        }

        protected abstract bool DeleteDocumentInternal(String name);

        public IDocument<TReader,TWriter> GetDocument(string name)
        {
            if (this._activeDocuments.ContainsKey(name))
            {
                this._activeDocuments[name].IncrementReferenceCount();
                return this._activeDocuments[name].Document;
            }
            else
            {
                IDocument<TReader,TWriter> doc = this.GetDocumentInternal(name);
                if (!this._activeDocuments.ContainsKey(name))
                {
                    this._activeDocuments.Add(name, new DocumentReference<TReader,TWriter>(doc));
                }
                this._activeDocuments[name].IncrementReferenceCount();
                return this._activeDocuments[name].Document;
            }
        }

        protected abstract IDocument<TReader,TWriter> GetDocumentInternal(String name);

        public abstract IGraphRegistry GraphRegistry
        {
            get;
        }

        public bool ReleaseDocument(String name)
        {
            if (this._activeDocuments.ContainsKey(name))
            {
                this._activeDocuments[name].DecrementReferenceCount();
                if (this._activeDocuments[name].ReferenceCount == 0)
                {
                    this._activeDocuments.Remove(name);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual void Flush()
        {

        }

        public virtual void Dispose()
        {
            foreach (DocumentReference<TReader,TWriter> reference in this._activeDocuments.Values)
            {
                reference.Dispose();
            }
        }
    }
}
