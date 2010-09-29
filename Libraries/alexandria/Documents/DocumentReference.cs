using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Alexandria.Documents
{
    public class DocumentReference : IDisposable
    {
        private IDocument _document;
        private int _count = 0;

        public DocumentReference(IDocument document)
        {
            this._document = document;
        }

        public void IncrementReferenceCount()
        {
            Interlocked.Increment(ref this._count);
        }

        public void DecrementReferenceCount()
        {
            Interlocked.Decrement(ref this._count);
        }

        public int ReferenceCount
        {
            get
            {
                return this._count;
            }
        }

        public IDocument Document
        {
            get
            {
                return this._document;
            }
        }

        public void Dispose()
        {
            this._document.Dispose();
        }
    }
}
