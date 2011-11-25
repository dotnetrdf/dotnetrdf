using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Validation;
using VDS.RDF.Utilities.Editor.AutoComplete;

namespace VDS.RDF.Utilities.Editor
{
    public class DocumentManager<TControl, TFont, TColor>
        where TFont : class
        where TColor : struct
    {
        //General State
        private ITextEditorAdaptorFactory<TControl> _factory;
        private List<Document<TControl>> _documents = new List<Document<TControl>>();
        private int _current = 0;
        private ManagerOptions<TControl> _options = new ManagerOptions<TControl>();
        private VisualOptions<TFont, TColor> _visualOptions;
        private String _defaultTitle = "Untitled";
        private String _defaultSyntax = "None";
        private int _nextID = 0;

        //Default Callbacks
        private SaveChangesCallback<TControl> _defaultSaveChangesCallback = new SaveChangesCallback<TControl>(d => SaveChangesMode.Discard);
        private SaveAsCallback<TControl> _defaultSaveAsCallback = new SaveAsCallback<TControl>(d => null);

        public DocumentManager(ITextEditorAdaptorFactory<TControl> factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            this._factory = factory;

            if (this._factory is IVisualTextEditorAdaptorFactory<TControl, TFont, TColor>)
            {
                this._visualOptions = ((IVisualTextEditorAdaptorFactory<TControl, TFont, TColor>)this._factory).GetDefaultVisualOptions();
            }

            //Wire Up Events
            this._options.HighlightingToggled += this.HandleHighlightingToggled;
            this._options.HighlightErrorsToggled += this.HandleHighlightErrorsToggled;
            this._options.AutoCompleteToggled += this.HandleAutoCompleteToggled;
            this._options.SymbolSelectorChanged += this.HandleSymbolSelectorChanged;
            if (this._visualOptions != null)
            {
                this._visualOptions.Changed += this.HandleVisualOptionsChanged;
            }
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

        public String DefaultSyntax
        {
            get
            {
                return this._defaultSyntax;
            }
            set
            {
                this._defaultSyntax = value;
            }
        }

        public ManagerOptions<TControl> Options
        {
            get
            {
                return this._options;
            }
        }

        public VisualOptions<TFont, TColor> VisualOptions
        {
            get
            {
                return this._visualOptions;
            }
            set
            {
                this._visualOptions = value;
            }
        }

        public Document<TControl> ActiveDocument
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

        public Document<TControl> this[int index]
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

        public IEnumerable<Document<TControl>> Documents
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

        public SaveChangesCallback<TControl> DefaultSaveChangesCallback
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

        public SaveAsCallback<TControl> DefaultSaveAsCallback
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

        public Document<TControl> New()
        {
            return this.New(this._defaultTitle + (++this._nextID));
        }

        public Document<TControl> New(bool switchTo)
        {
            return this.New(this._defaultTitle + (++this._nextID), switchTo);
        }

        public Document<TControl> New(String title)
        {
            return this.New(title, false);
        }

        public Document<TControl> New(String title, bool switchTo)
        {
            Document<TControl> doc = new Document<TControl>(this._factory.CreateAdaptor(), null, title);

            //Add the document to the collection of documents
            //Register for appropriate events on the document
            this._documents.Add(doc);
            this.RaiseDocumentCreated(doc);
            doc.ValidatorChanged += new DocumentChangedHandler<TControl>(this.HandleValidatorChanged);
            doc.Opened += new DocumentChangedHandler<TControl>(this.HandleTextChanged);
            doc.TextChanged += new DocumentChangedHandler<TControl>(this.HandleTextChanged);

            //Apply relevant global options to the Document
            doc.Syntax = this._defaultSyntax;
            doc.IsHighlightingEnabled = this._options.IsSyntaxHighlightingEnabled;
            doc.IsAutoCompleteEnabled = this._options.IsAutoCompletionEnabled;
            if (doc.IsAutoCompleteEnabled && doc.TextEditor.CanAutoComplete)
            {
                doc.TextEditor.AutoCompleter = AutoCompleteManager.GetAutoCompleter<TControl>(doc.Syntax, doc.TextEditor);
            }
            else
            {
                doc.TextEditor.AutoCompleter = null;
            }
            if (this._visualOptions != null) doc.TextEditor.Apply<TFont, TColor>(this._visualOptions);
            doc.TextEditor.SymbolSelector = this._options.CurrentSymbolSelector;

            //Switch to the new document if required
            if (switchTo)
            {
                this._current = this._documents.Count - 1;
                this.RaiseActiveDocumentChanged(this.ActiveDocument);
            }
            return doc;
        }

        public Document<TControl> NewFromActive()
        {
            return this.NewFromExisting(this.ActiveDocument, false);
        }

        public Document<TControl> NewFromActive(bool switchTo)
        {
            return this.NewFromExisting(this.ActiveDocument, switchTo);
        }

        public Document<TControl> NewFromExisting(Document<TControl> doc)
        {
            return this.NewFromExisting(doc, false);
        }

        public Document<TControl> NewFromExisting(Document<TControl> doc, bool switchTo)
        {
            Document<TControl> clonedDoc = this.New();
            clonedDoc.Text = doc.Text;
            clonedDoc.Syntax = doc.Syntax;
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

        public bool Close(SaveChangesCallback<TControl> callback, SaveAsCallback<TControl> saveAs)
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

        public bool Close(int index, SaveChangesCallback<TControl> callback, SaveAsCallback<TControl> saveAs)
        {
            if (index >= 0 && index < this._documents.Count)
            {
                Document<TControl> doc = this._documents[index];
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

        private bool Close(Document<TControl> doc, SaveChangesCallback<TControl> callback, SaveAsCallback<TControl> saveAs)
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

        public void CloseAll(SaveChangesCallback<TControl> callback, SaveAsCallback<TControl> saveAs)
        {
            while (this._documents.Count > 0)
            {
                //Get Confirmation to save/discard changes - allows application to cancel close
                Document<TControl> doc = this._documents[0];
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

        public void SaveAll(SaveAsCallback<TControl> saveAs)
        {
            foreach (Document<TControl> doc in this._documents)
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

        #region Event Handling

        private void HandleValidatorChanged(Object sender, DocumentChangedEventArgs<TControl> args)
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

        private void HandleTextChanged(Object sender, DocumentChangedEventArgs<TControl> args)
        {
            //Update Syntax Validation if appropriate
            if (args.Document.SyntaxValidator != null && this._options.IsValidateAsYouTypeEnabled)
            {
                args.Document.TextEditor.ClearErrorHighlights();
                ISyntaxValidationResults results = args.Document.Validate();
                if (results != null && this._options.IsHighlightErrorsEnabled)
                {
                    if (results.Error != null && !results.IsValid)
                    {
                        args.Document.TextEditor.AddErrorHighlight(results.Error);
                    }
                }
            }
        }

        private void HandleHighlightingToggled()
        {
            foreach (Document<TControl> doc in this._documents)
            {
                doc.IsHighlightingEnabled = this._options.IsSyntaxHighlightingEnabled;
            }
        }

        private void HandleHighlightErrorsToggled()
        {
            foreach (Document<TControl> doc in this._documents)
            {
                doc.TextEditor.Refresh();
            }
        }

        private void HandleAutoCompleteToggled()
        {
            foreach (Document<TControl> doc in this._documents)
            {
                doc.IsAutoCompleteEnabled = this._options.IsAutoCompletionEnabled;
            }
        }

        private void HandleSymbolSelectionToggled()
        {
            foreach (Document<TControl> doc in this._documents)
            {
                doc.TextEditor.SymbolSelector = (this.Options.IsSymbolSelectionEnabled ? this.Options.CurrentSymbolSelector : null);
            }
        }

        private void HandleSymbolSelectorChanged()
        {
            foreach (Document<TControl> doc in this._documents)
            {
                doc.TextEditor.SymbolSelector = (this.Options.IsSymbolSelectionEnabled ? this.Options.CurrentSymbolSelector : null);
            }
        }

        private void HandleVisualOptionsChanged()
        {
            if (this._visualOptions != null)
            {
                foreach (Document<TControl> doc in this._documents)
                {
                    doc.TextEditor.Apply<TFont, TColor>(this._visualOptions);
                }
            }
        }

        #endregion

        #region Events

        private void RaiseActiveDocumentChanged(Document<TControl> doc)
        {
            DocumentChangedHandler<TControl> d = this.ActiveDocumentChanged;
            if (d != null)
            {
                d(this, new DocumentChangedEventArgs<TControl>(doc));
            }
        }

        private void RaiseDocumentCreated(Document<TControl> doc)
        {
            DocumentChangedHandler<TControl> d = this.DocumentCreated;
            if (d != null)
            {
                d(this, new DocumentChangedEventArgs<TControl>(doc));
            }
        }

        public event DocumentChangedHandler<TControl> ActiveDocumentChanged;

        public event DocumentChangedHandler<TControl> DocumentCreated;

        #endregion
    }
}
