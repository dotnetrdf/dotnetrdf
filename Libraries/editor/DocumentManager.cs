using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Validation;

namespace VDS.RDF.Utilities.Editor
{
    public class DocumentManager<T>
    {
        //General State
        private ITextEditorAdaptorFactory<T> _factory;
        private List<Document<T>> _documents = new List<Document<T>>();
        private int _current = 0;
        private ManagerOptions<T> _options = new ManagerOptions<T>();
        private String _defaultTitle = "Untitled";
        private int _nextID = 0;

        //Default Callbacks
        private SaveChangesCallback<T> _defaultSaveChangesCallback = new SaveChangesCallback<T>(d => SaveChangesMode.Discard);
        private SaveAsCallback<T> _defaultSaveAsCallback = new SaveAsCallback<T>(d => null);

        public DocumentManager(ITextEditorAdaptorFactory<T> factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            this._factory = factory;
        }

        #region General State

        public String DefaultTitle
        {
            get
            {
                return this._defaultTitle;
            }
            set
            {
                this._defaultTitle = value;
            }
        }

        public ManagerOptions<T> Options
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

        public int ActiveDocumentIndex
        {
            get
            {
                if (this._documents.Count > 0)
                {
                    return this._current;
                }
                else
                {
                    return -1;
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

        #endregion

        #region Default Callbacks

        public SaveChangesCallback<T> DefaultSaveChangesCallback
        {
            get
            {
                return this._defaultSaveChangesCallback;
            }
            set
            {
                if (value != null)
                {
                    this._defaultSaveChangesCallback = value;
                }
            }
        }

        public SaveAsCallback<T> DefaultSaveAsCallback
        {
            get
            {
                return this._defaultSaveAsCallback;
            }
            set
            {
                if (value != null)
                {
                    this._defaultSaveAsCallback = value;
                }
            }
        }

        #endregion

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

        public Document<T> New()
        {
            return this.New(this._defaultTitle + (++this._nextID));
        }

        public Document<T> New(String title)
        {
            return this.New(title, false);
        }

        public Document<T> New(String title, bool switchTo)
        {
            Document<T> doc = new Document<T>(this._factory.CreateAdaptor(), null, title);

            //Add the document to the collection of documents
            //Register for appropriate events on the document
            this._documents.Add(doc);
            doc.ValidatorChanged += new DocumentChangedHandler<T>(this.HandleValidatorChanged);
            doc.Opened += new DocumentChangedHandler<T>(this.HandleTextChanged);
            doc.TextChanged += new DocumentChangedHandler<T>(this.HandleTextChanged);

            //Apply relevant global options to the Document
            doc.IsHighlightingEnabled = this._options.IsSyntaxHighlightingEnabled;

            //Switch to the new document if required
            if (switchTo)
            {
                this._current = this._documents.Count - 1;
                this.RaiseActiveDocumentChanged(this.ActiveDocument);
            }
            return doc;
        }

        public Document<T> NewFromActive()
        {
            return this.NewFromExisting(this.ActiveDocument, false);
        }

        public Document<T> NewFromActive(bool switchTo)
        {
            return this.NewFromExisting(this.ActiveDocument, switchTo);
        }

        public Document<T> NewFromExisting(Document<T> doc)
        {
            return this.NewFromExisting(doc, false);
        }

        public Document<T> NewFromExisting(Document<T> doc, bool switchTo)
        {
            Document<T> clonedDoc = this.New();
            clonedDoc.Text = doc.Text;
            if (switchTo)
            {
                this._current = this._documents.Count - 1;
                this.RaiseActiveDocumentChanged(this.ActiveDocument);
            }
            return clonedDoc;
        }

        public bool Close()
        {
            return this.Close(this._defaultSaveChangesCallback, this._defaultSaveAsCallback);
        }

        public bool Close(SaveChangesCallback<T> callback, SaveAsCallback<T> saveAs)
        {
            if (this._documents.Count > 0)
            {
                if (!this.Close(this.ActiveDocument, callback, saveAs)) return false;
                this._documents.RemoveAt(this._current);
                this.CorrectIndex();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Close(int index)
        {
            return this.Close(index, this._defaultSaveChangesCallback, this._defaultSaveAsCallback);
        }

        public bool Close(int index, SaveChangesCallback<T> callback, SaveAsCallback<T> saveAs)
        {
            if (index >= 0 && index < this._documents.Count)
            {
                Document<T> doc = this._documents[index];
                if (!this.Close(doc, callback, saveAs)) return false;
                this._documents.RemoveAt(index);
                this.CorrectIndex();
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool Close(Document<T> doc, SaveChangesCallback<T> callback, SaveAsCallback<T> saveAs)
        {
            //Get Confirmation to save/discard changes - allows application to cancel close
            if (doc.HasChanged)
            {
                switch (callback(doc))
                {
                    case SaveChangesMode.Cancel:
                        return false;
                    case SaveChangesMode.Discard:
                        //Do nothing
                        return true;
                    case SaveChangesMode.Save:
                        if (doc.Filename != null && !doc.Filename.Equals(String.Empty))
                        {
                            doc.Save();
                        }
                        else
                        {
                            String filename = saveAs(doc);
                            if (filename != null && !filename.Equals(String.Empty))
                            {
                                doc.SaveAs(filename);
                            }
                        }
                        return true;
                    default:
                        return true;
                }
            }
            else
            {
                return true;
            }
        }

        public void CloseAll()
        {
            this.CloseAll(this._defaultSaveChangesCallback, this._defaultSaveAsCallback);
        }

        public void CloseAll(SaveChangesCallback<T> callback, SaveAsCallback<T> saveAs)
        {
            while (this._documents.Count > 0)
            {
                //Get Confirmation to save/discard changes - allows application to cancel close
                Document<T> doc = this._documents[0];
                if (!this.Close(doc, callback, saveAs)) return;
                this._documents.RemoveAt(0);
            }
        }

        public void ReloadAll()
        {
            this._documents.ForEach(d => d.Reload());
        }

        public void SaveAll()
        {
            this.SaveAll(this._defaultSaveAsCallback);
        }

        public void SaveAll(SaveAsCallback<T> saveAs)
        {
            foreach (Document<T> doc in this._documents)
            {
                if (doc.Filename != null && !doc.Filename.Equals(String.Empty))
                {
                    doc.Save();
                }
                else
                {
                    String filename = saveAs(doc);
                    if (filename != null && !filename.Equals(String.Empty))
                    {
                        doc.SaveAs(filename);
                    }
                }
            }
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

        #endregion

        #region Handling of Document Events

        private void HandleValidatorChanged(Object sender, DocumentChangedEventArgs<T> args)
        {
            //Update Syntax Validation if appropriate
            if (args.Document.SyntaxValidator != null && this._options.IsValidateAsYouTypeEnabled)
            {
                ISyntaxValidationResults results = args.Document.Validate();
                if (results != null && this._options.IsHighlightErrorsEnabled)
                {
                    if (results.Error != null && !results.IsValid)
                    {
                        args.Document.TextEditor.ClearErrorHighlights();
                        args.Document.TextEditor.AddErrorHighlight(results.Error);
                    }
                    else
                    {
                        args.Document.TextEditor.ClearErrorHighlights();
                    }
                }
                else
                {
                    args.Document.TextEditor.ClearErrorHighlights();
                }
            }
        }

        private void HandleTextChanged(Object sender, DocumentChangedEventArgs<T> args)
        {
            //Update Syntax Validation if appropriate
            if (args.Document.SyntaxValidator != null && this._options.IsValidateAsYouTypeEnabled)
            {
                ISyntaxValidationResults results = args.Document.Validate();
                if (results != null && this._options.IsHighlightErrorsEnabled)
                {
                    if (results.Error != null && !results.IsValid)
                    {
                        args.Document.TextEditor.ClearErrorHighlights();
                        args.Document.TextEditor.AddErrorHighlight(results.Error);
                    }
                    else
                    {
                        args.Document.TextEditor.ClearErrorHighlights();
                    }
                }
                else
                {
                    args.Document.TextEditor.ClearErrorHighlights();
                }
            }
        }

        #endregion

        #region Events

        private void RaiseActiveDocumentChanged(Document<T> doc)
        {
            DocumentChangedHandler<T> d = this.ActiveDocumentChanged;
            if (d != null)
            {
                d(this, new DocumentChangedEventArgs<T>(doc));
            }
        }

        public event DocumentChangedHandler<T> ActiveDocumentChanged;

        #endregion
    }
}
