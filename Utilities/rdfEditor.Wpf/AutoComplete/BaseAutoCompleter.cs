using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Utilities.Editor.AutoComplete
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
            this._c.KeyDown += new KeyEventHandler(CompletionWindowKeyDown);
            this._startOffset = this._c.StartOffset;
        }

        protected virtual void CompletionWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                this._c.CompletionList.RequestInsertion(e);
                e.Handled = true;
            }
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
                if (this._c != null) this._c.Close();
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
            if (this._c != null) this._c.Close();
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

        public void DetectState(TextEditor editor)
        {
            //Don't do anything if currently disabled
            if (this.State == AutoCompleteState.Disabled) return;

            //Then call the derived classes internal state detection (if any)
            this.DetectStateInternal(editor);

            //Work out what sort of token we are currently in and set our State appropriately
            DocumentHighlighter h = new DocumentHighlighter(editor.Document, Syntax.SyntaxManager.GetHighlighter("Turtle").MainRuleSet);
            DocumentLine line = editor.Document.GetLineByOffset(editor.CaretOffset);
            HighlightedLine result = h.HighlightLine(line);

            HighlightedSection current = result.Sections.FirstOrDefault(s => s.Offset <= editor.CaretOffset && (s.Offset + s.Length) > editor.CaretOffset);
            if (current != null)
            {
                if (current.Color != null)
                {
                    switch (current.Color.Name)
                    {
                        case "Keyword":
                            this.State = AutoCompleteState.Keyword;
                            this.StartOffset = current.Offset;
                            break;

                        case "URI":
                            this.State = AutoCompleteState.Uri;
                            this.StartOffset = current.Offset;
                            break;

                        case "QName":
                            this.State = AutoCompleteState.QName;
                            this.StartOffset = current.Offset;
                            break;

                        case "String":
                            String lit = editor.Document.GetText(current.Offset, current.Length);
                            if (lit.StartsWith("\"\"\""))
                            {
                                this.State = AutoCompleteState.LongLiteral;
                            }
                            else
                            {
                                this.State = AutoCompleteState.Literal;
                            }
                            break;

                        case "Comment":
                            this.State = AutoCompleteState.Comment;
                            this.StartOffset = current.Offset;
                            break;

                        case "BNode":
                            this.State = AutoCompleteState.BNode;
                            this.StartOffset = current.Offset;
                            break;

                        case "Variables":
                            this.State = AutoCompleteState.Variable;
                            this.StartOffset = current.Offset;
                            break;

                        default:
                            this.State = AutoCompleteState.None;
                            break;
                    }
                }
            }
        }

        protected virtual void DetectStateInternal(TextEditor editor)
        { }

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

        public abstract object Clone();
    }
}
