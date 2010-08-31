using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace rdfEditor.AutoComplete
{
    public abstract class BaseAutoCompleter : IAutoCompleter
    {
        private AutoCompleteState _state = AutoCompleteState.None;
        private AutoCompleteState _lastCompletion = AutoCompleteState.None;
        private AutoCompleteState _temp = AutoCompleteState.None;

        private int _startOffset = 0;

        protected CompletionWindow _c;
        protected TextEditor _editor;

        #region Completion Window Management

        protected void SetupCompletionWindow(TextArea area)
        {
            this._c = new CompletionWindow(area);
            this._c.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            this._c.CompletionList.InsertionRequested += new EventHandler(CompletionList_InsertionRequested);
            this._c.CloseAutomatically = true;
            this._c.CloseWhenCaretAtBeginning = true;
            this._c.Closed += new EventHandler(this.CompletionWindowClosed);
            this._startOffset = this._c.StartOffset;
        }

        protected void SetupCompletionWindow(TextArea area, IEnumerable<ICompletionData> data)
        {
            this.SetupCompletionWindow(area);
            this.AddCompletionData(data);
        }

        protected void AddCompletionData(ICompletionData data)
        {
            if (this._c != null)
            {
                this._c.CompletionList.CompletionData.Add(data);
            }
        }

        protected void AddCompletionData(IEnumerable<ICompletionData> data)
        {
            if (this._c != null)
            {
                foreach (ICompletionData dataItem in data)
                {
                    this._c.CompletionList.CompletionData.Add(dataItem);
                }
            }
        }

        protected void RemoveCompletionData(Func<ICompletionData, bool> filter)
        {
            if (this._c != null)
            {
                for (int i = 0; i < this._c.CompletionList.CompletionData.Count; i++)
                {
                    if (filter(this._c.CompletionList.CompletionData[i]))
                    {
                        this._c.CompletionList.CompletionData.RemoveAt(i);
                        i--;
                    }
                }

                //Close if no auto-completion left
                if (this._c.CompletionList.CompletionData.Count == 0)
                {
                    this._c.Close();
                }
            }
        }

        protected void AbortAutoComplete()
        {
            this.State = AutoCompleteState.None;
            this.TemporaryState = AutoCompleteState.None;
            this.LastCompletion = AutoCompleteState.None;
            if (this._c != null) this._c.Close();
        }

        protected void FinishAutoComplete(bool saveCompletion, bool noAutoEnd)
        {
            if (saveCompletion) this.TemporaryState = this.State;
            if (!noAutoEnd)
            {
                this.LastCompletion = this.TemporaryState;
                if (this.LastCompletion != AutoCompleteState.None)
                {
                    this.State = AutoCompleteState.Inserted;
                }
            }
            else
            {
                this.FinishAutoComplete();
            }
        }

        protected void FinishAutoComplete()
        {
            this.LastCompletion = this.TemporaryState;
            if (this.LastCompletion != AutoCompleteState.None)
            {
                this.State = AutoCompleteState.Inserted;
                this.EndAutoComplete(this._editor);
            }
        }

        private void CompletionList_InsertionRequested(object sender, EventArgs e)
        {
            this.FinishAutoComplete();
        }

        private void CompletionWindowClosed(Object sender, EventArgs e)
        {
            bool autoClosed = this._c.CloseAutomatically && this._c.CloseWhenCaretAtBeginning;
            this._c = null;

            //Should we be aborting an auto-complete?
            if (this.Length == 0 && autoClosed)
            {
                this.AbortAutoComplete();
            }

            //Reset State
            this.TemporaryState = this.State;
        }

        #endregion

        #region Current Text Management

        public int StartOffset
        {
            get
            {
                return this._startOffset;
            }
            set
            {
                this._startOffset = value;
            }
        }

        public int Length
        {
            get
            {
                return this._editor.CaretOffset - this.StartOffset;
            }
        }

        public String CurrentText
        {
            get
            {
                if (this._editor == null)
                {
                    return String.Empty;
                }
                else
                {
                    return this._editor.Document.GetText(this._startOffset, this.Length);
                }
            }
        }

        #endregion

        public abstract void Initialise(TextEditor editor);

        public abstract void DetectState(TextEditor editor);

        public abstract void TryAutoComplete(TextEditor editor, TextCompositionEventArgs e);

        public abstract void EndAutoComplete(TextEditor editor);

        public AutoCompleteState State
        {
            get
            {
                return this._state;
            }
            set
            {
                this._state = value;
            }
        }

        public AutoCompleteState LastCompletion
        {
            get
            {
                return this._lastCompletion;
            }
            set
            {
                this._lastCompletion = value;
            }
        }

        protected AutoCompleteState TemporaryState
        {
            get
            {
                return this._temp;
            }
            set
            {
                this._temp = value;
            }
        }
    }
}
