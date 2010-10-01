using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using Alexandria.Documents;

namespace Alexandria.Indexing
{
    class FileIndexReader : IDisposable, IEnumerable<Triple>
    {
        private IDocument _doc;

        public FileIndexReader(IDocument doc)
        {
            this._doc = doc;
        }

        ~FileIndexReader()
        {
            this.Dispose(false);
        }

        public IEnumerator<Triple> GetEnumerator()
        {
            return new FileIndexEnumerator(this._doc);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);

            this._doc.DocumentManager.ReleaseDocument(this._doc.Name);

        }
    }

    class FileIndexEnumerator : IEnumerator<Triple>
    {
        private IDocument _doc;
        private Triple _current;
        private StreamingNQuadsParser _parser;

        public FileIndexEnumerator(IDocument doc)
        {
            this._doc = doc;
        }

        ~FileIndexEnumerator()
        {
            this.Dispose(false);
        }

        public Triple Current
        {
            get 
            {
                if (this._parser == null) throw new InvalidOperationException("The enumerator is positioned before the first element of the collection");
                if (this._parser.EOF)
                {
                    if (this._current == null) throw new InvalidOperationException("The enumerator is positioned after the last element of the collection");
                    Triple temp = this._current;
                    this._current = null;
                    return temp;
                }
                return this._current;
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get 
            { 
                return this.Current; 
            }
        }

        public bool MoveNext()
        {
            if (this._parser == null)
            {
                StreamReader reader = this._doc.BeginRead();
                this._parser = new StreamingNQuadsParser(reader);
            }

            if (this._parser.EOF) return false;

            this._current = this._parser.GetNextTriple();
            if (this._parser.EOF) this._doc.EndRead();
            return true;
        }

        public void Reset()
        {
            throw new NotSupportedException("Reset() is not supported by this enumerator");
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            if (this._parser != null)
            {
                if (!this._parser.EOF) this._doc.EndRead();
            }
        }
    }
}
