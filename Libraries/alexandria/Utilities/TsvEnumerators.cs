using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.Alexandria.Documents;

namespace VDS.Alexandria.Utilities
{
    class TsvEnumerator : IEnumerator<String[]>, IEnumerable<String[]>
    {
        private IDocument<StreamReader, TextWriter> _doc;
        private StreamReader _reader;
        private String[] _current;
        private int _maxItems = 0;
        private bool _join = false;

        public TsvEnumerator(IDocument<StreamReader, TextWriter> doc)
        {
            this._doc = doc;
        }

        public TsvEnumerator(IDocument<StreamReader, TextWriter> doc, int maxItems)
            : this(doc)
        {
            this._join = true;
            this._maxItems = Math.Max(1, maxItems);
        }

        ~TsvEnumerator()
        {
            this.Dispose(false);
        }

        public String[] Current
        {
            get
            {
                if (this._reader == null) throw new InvalidOperationException("The enumerator is positioned before the first element of the collection");
                if (this._reader.EndOfStream)
                {
                    if (this._current == null) throw new InvalidOperationException("The enumerator is positioned after the last element of the collection");
                    String[] temp = this._current;
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
            if (this._reader == null)
            {
                this._reader = this._doc.BeginRead();
            }

            if (this._reader.EndOfStream) return false;

            if (this._join)
            {
                this._current = this._reader.ReadLine().Split(new char[] { '\t' }, this._maxItems);
            }
            else
            {
                this._current = this._reader.ReadLine().Split('\t');
            }
            if (this._reader.EndOfStream) this._doc.EndRead();
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
            if (this._reader != null)
            {
                if (!this._reader.EndOfStream) this._doc.EndRead();
                this._reader.Close();
                this._reader = null;
            }
        }

        #region IEnumerable<string> Members

        public IEnumerator<string[]> GetEnumerator()
        {
            return this;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }

    class TsvSingleItemEnumerator : IEnumerator<String>, IEnumerable<String>
    {
        private IDocument<StreamReader, TextWriter> _doc;
        private StreamReader _reader;
        private String _current;
        private int _index = 0;
        private bool _join = false;

        public TsvSingleItemEnumerator(IDocument<StreamReader, TextWriter> doc)
        {
            this._doc = doc;
        }

        public TsvSingleItemEnumerator(IDocument<StreamReader, TextWriter> doc, int index)
            : this(doc)
        {
            this._index = index;
        }

        public TsvSingleItemEnumerator(IDocument<StreamReader, TextWriter> doc, int index, bool join)
            : this(doc, index)
        {
            this._join = join;
        }

        ~TsvSingleItemEnumerator()
        {
            this.Dispose(false);
        }

        public String Current
        {
            get
            {
                if (this._reader == null) throw new InvalidOperationException("The enumerator is positioned before the first element of the collection");
                if (this._reader.EndOfStream)
                {
                    if (this._current == null) throw new InvalidOperationException("The enumerator is positioned after the last element of the collection");
                    String temp = this._current;
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
            if (this._reader == null)
            {
                this._reader = this._doc.BeginRead();
            }

            if (this._reader.EndOfStream) return false;

            String[] values;
            if (this._join && this._index == 0)
            {
                values = null;
            }
            else if (this._join)
            {
                values = this._reader.ReadLine().Split(new char[] { '\t' }, this._index + 1);
            }
            else
            {
                values = this._reader.ReadLine().Split('\t');
            }
            if (values == null)
            {
                this._current = this._reader.ReadLine();
            }
            else
            {
                this._current = values[this._index];
            }
            if (this._reader.EndOfStream) this._doc.EndRead();
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
            if (this._reader != null)
            {
                if (!this._reader.EndOfStream) this._doc.EndRead();
                this._reader.Close();
                this._reader = null;
            }
        }

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            return this;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
