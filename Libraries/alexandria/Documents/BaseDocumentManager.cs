using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Alexandria.Documents.Adaptors;

namespace Alexandria.Documents
{
    public abstract class BaseDocumentManager : IDocumentManager
    {
        private Dictionary<String,DocumentReference> _activeDocuments = new Dictionary<string,DocumentReference>();
        private IDataAdaptor _adaptor = new NTriplesAdaptor();

        public BaseDocumentManager() { }

        public BaseDocumentManager(IDataAdaptor adaptor)
        {
            this._adaptor = adaptor;
        }

        public IDataAdaptor DataAdaptor
        {
            get
            {
                return this._adaptor;
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

        public IDocument GetDocument(string name)
        {
            if (this._activeDocuments.ContainsKey(name))
            {
                this._activeDocuments[name].IncrementReferenceCount();
                return this._activeDocuments[name].Document;
            }
            else
            {
                IDocument doc = this.GetDocumentInternal(name);
                if (!this._activeDocuments.ContainsKey(name))
                {
                    this._activeDocuments.Add(name, new DocumentReference(doc));
                }
                this._activeDocuments[name].IncrementReferenceCount();
                return this._activeDocuments[name].Document;
            }
        }

        protected abstract IDocument GetDocumentInternal(String name);

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

        public virtual void Dispose()
        {
            foreach (DocumentReference reference in this._activeDocuments.Values)
            {
                reference.Dispose();
            }
        }
    }
}
