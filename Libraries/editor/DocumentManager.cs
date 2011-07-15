using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    public class DocumentManager<T>
    {
        private List<Document<T>> _documents = new List<Document<T>>();
        private int _current = 0;
        private GlobalOptions<T> _options = new GlobalOptions<T>();

        public GlobalOptions<T> Options
        {
            get
            {
                return this._options;
            }
        }

        public Document<T> ActiveDocument
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

        public Document<T> this[int index]
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

        public IEnumerable<Document<T>> Documents
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

        #region Document Management

        private void CorrectIndex()
        {
            if (this._documents.Count > 0)
            {
                if (this._current >= this._documents.Count)
                {
                    this._current = this._documents.Count - 1;
                }
                else if (this._current < 0)
                {
                    this._current = this._documents.Count - 1;
                }
                this.RaiseActiveDocumentChanged(this.ActiveDocument);
            }
        }

        public void Add(Document<T> doc)
        {
            this.Add(doc, false);
        }

        public void Add(Document<T> doc, bool switchTo)
        {
            this._documents.Add(doc);
            if (switchTo)
            {
                this._current = this._documents.Count - 1;
                this.RaiseActiveDocumentChanged(this.ActiveDocument);
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

        public void ReloadAll()
        {
            this._documents.ForEach(d => d.Reload());
        }

        public void SaveAll()
        {
            this._documents.ForEach(d => d.Save());
        }

        public void SwitchTo(int index)
        {
            if (index >= 0 && index < this._documents.Count)
            {
                if (index != this._current)
                {
                    this._current = index;
                    this.RaiseActiveDocumentChanged(this.ActiveDocument);
                }
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public void PrevDocument()
        {
            if (this._documents.Count > 1)
            {
                this._current--;
                this.CorrectIndex();
            }
        }

        public void NextDocument()
        {
            if (this._documents.Count > 1)
            {
                this._current++;
                this.CorrectIndex();
            }
        }

        public void Copy(ITextEditorAdaptorFactory<T> factory)
        {
            this.Copy(factory, this.ActiveDocument, false);
        }

        public void Copy(ITextEditorAdaptorFactory<T> factory, bool switchTo)
        {
            this.Copy(factory, this.ActiveDocument, switchTo);
        }

        public void Copy(ITextEditorAdaptorFactory<T> factory, Document<T> doc, bool switchTo)
        {
            Document<T> clonedDoc = new Document<T>(factory.CreateAdaptor());
            clonedDoc.Text = doc.Text;
            this._documents.Add(clonedDoc);
            if (switchTo)
            {
                this._current = this._documents.Count - 1;
                this.RaiseActiveDocumentChanged(this.ActiveDocument);
            }
        }

        #endregion

        private void RaiseActiveDocumentChanged(Document<T> doc)
        {
            DocumentChangedHandler<T> d = this.ActiveDocumentChanged;
            if (d != null)
            {
                d(this, new DocumentChangedEventArgs<T>(doc));
            }
        }

        public event DocumentChangedHandler<T> ActiveDocumentChanged;
    }
}
