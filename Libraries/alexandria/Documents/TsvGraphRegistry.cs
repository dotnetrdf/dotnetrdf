using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Alexandria.Documents
{
    public class TsvGraphRegistry : IGraphRegistry, IDisposable
    {
        private IDocument _doc;
        private SHA256Managed _hash;

        public TsvGraphRegistry(IDocument doc)
        {
            this._doc = doc;
        }

        ~TsvGraphRegistry()
        {
            this.Dispose(false);
        }

        public virtual string GetDocumentName(string graphUri)
        {
            //Only instantiate the SHA256 class when we first use it
            if (_hash == null) _hash = new SHA256Managed();

            if (graphUri.Equals(String.Empty) || graphUri == null) return "default-graph";

            Byte[] input = Encoding.UTF8.GetBytes(graphUri);
            Byte[] output = _hash.ComputeHash(input);

            StringBuilder hash = new StringBuilder();
            foreach (Byte b in output)
            {
                hash.Append(b.ToString("x2"));
            }

            return hash.ToString();
        }

        public virtual String GetGraphUri(String name)
        {
            throw new NotImplementedException();
        }

        public bool RegisterGraph(String graphUri, String name)
        {
            try
            {
                TextWriter writer = this._doc.BeginWrite(true);
                writer.WriteLine(name + "\t" + graphUri);
                writer.Close();
                this._doc.EndWrite();
                return true;
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while trying to register a Graph", ex);
            }
        }

        public bool UnregisterGraph(String graphUri, String name)
        {
            try
            {
                StringBuilder editedOutput = new StringBuilder();
                int lineCount = 0, editedLineCount = 0;

                using (StreamReader reader = this._doc.BeginRead())
                {
                    String toRemove = name + "\t" + graphUri;
                    while (!reader.EndOfStream)
                    {
                        String line = reader.ReadLine();
                        if (!line.Equals(toRemove))
                        {
                            editedOutput.AppendLine(line);
                            editedLineCount++;
                        }
                        lineCount++;
                    }
                    reader.Close();
                }
                this._doc.EndRead();

                if (lineCount > editedLineCount)
                {
                    if (editedLineCount > 0)
                    {
                        TextWriter writer = this._doc.BeginWrite(false);
                        writer.Write(editedOutput.ToString());
                        writer.Close();
                        this._doc.EndWrite();
                    }
                    else
                    {
                        this._doc.DocumentManager.ReleaseDocument(this._doc.Name);
                        this._doc.DocumentManager.DeleteDocument(this._doc.Name);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while trying to unregister a Graph", ex);
            }
        }

        public IEnumerable<String> DocumentNames
        {
            get
            {
                return new TsvSingleItemEnumerator(this._doc, 0);
            }
        }

        public IEnumerable<String> GraphUris
        {
            get
            {
                return new TsvSingleItemEnumerator(this._doc, 1, true);
            }
        }

        public IEnumerable<KeyValuePair<String, String>> DocumentToGraphMappings
        {
            get
            {
                return new TsvEnumerator(this._doc, 2).Select(values => new KeyValuePair<String, String>(values[0], values[1]));                        
            }
        }

        public IEnumerable<KeyValuePair<String, String>> GraphToDocumentMappings
        {
            get
            {
                return new TsvEnumerator(this._doc, 2).Select(values => new KeyValuePair<String, String>(values[1], values[0]));
            }
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

    class TsvEnumerator : IEnumerator<String[]>, IEnumerable<String[]>
    {
        private IDocument _doc;
        private StreamReader _reader;
        private String[] _current;
        private int _maxItems = 0;
        private bool _join = false;

        public TsvEnumerator(IDocument doc)
        {
            this._doc = doc;
        }

        public TsvEnumerator(IDocument doc, int maxItems)
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
        private IDocument _doc;
        private StreamReader _reader;
        private String _current;
        private int _index = 0;
        private bool _join = false;

        public TsvSingleItemEnumerator(IDocument doc)
        {
            this._doc = doc;
        }

        public TsvSingleItemEnumerator(IDocument doc, int index)
            : this(doc)
        {
            this._index = index;
        }

        public TsvSingleItemEnumerator(IDocument doc, int index, bool join)
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
