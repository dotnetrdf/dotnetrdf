using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    public class DocumentManager
    {
        private List<Document> _documents = new List<Document>();
        private int _current = 0;
        private GlobalOptions _options = new GlobalOptions();

        public GlobalOptions Options
        {
            get
            {
                return this._options;
            }
        }

        public Document Current
        {
            get
            {
                if (this._documents.Count > 0)
                {
                    return this._documents[this._current];
                }
                else
                {
                    return null;
                }
            }
        }

        public Document this[int index]
        {
            get
            {
                if (index >= 0 && index < this._documents.Count)
                {
                    return this._documents[index];
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        public IEnumerable<Document> Documents
        {
            get
            {
                return this._documents;
            }
        }

        public int Count
        {
            get
            {
                return this._documents.Count;
            }
        }

        private void CorrectIndex()
        {
            if (this._current > this._documents.Count && this._documents.Count > 0)
            {
                this._current = this._documents.Count - 1;
            }
        }

        public void Add(Document doc)
        {
            this.Add(doc, false);
        }

        public void Add(Document doc, bool switchTo)
        {
            this._documents.Add(doc);
            if (switchTo)
            {
                this._current = this._documents.Count - 1;
            }
        }

        public void Close()
        {
            if (this._documents.Count > 0)
            {
                this._documents.RemoveAt(this._current);
                this.CorrectIndex();
            }
        }

        public void Close(int index)
        {
            if (index >= 0 && index < this._documents.Count)
            {
                this._documents.RemoveAt(index);
                this.CorrectIndex();
            }
        }

        public void CloseAll()
        {
            this._documents.Clear();
        }

        public void SaveAll()
        {

        }
    }
}
