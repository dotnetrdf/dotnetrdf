using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using rdfEditor.AutoComplete.Data;

namespace rdfEditor.AutoComplete
{
    public class NTriplesAutoCompleter : BaseAutoCompleter
    {
        protected String BlankNodePattern = @"_:\p{L}(\p{L}|\p{N}|-|_)*";

        private HashSet<ICompletionData> _bnodes = new HashSet<ICompletionData>();
        private BlankNodeMapper _bnodemap = new BlankNodeMapper();

        public override void Initialise(TextEditor editor)
        {
            //Initialise States
            this.State = AutoCompleteState.None;
            this.LastCompletion = AutoCompleteState.None;
            this.TemporaryState = AutoCompleteState.None;

            //Try to detect the state
            this.DetectState(editor);
        }

        #region State Detection

        public override void DetectState(TextEditor editor)
        {
            //Don't do anything if currently disabled
            if (this.State == AutoCompleteState.Disabled) return;

            //Look for Blank Nodes
            this.DetectBlankNodes(editor);
        }

        protected virtual void DetectBlankNodes(TextEditor editor)
        {
            this._bnodes.Clear();

            foreach (Match m in Regex.Matches(editor.Text, BlankNodePattern))
            {
                String id = m.Value;
                if (this._bnodes.Add(new BlankNodeCompletionData(id)))
                {
                    this._bnodemap.CheckID(ref id);
                }                
            }
        }

        #endregion

        #region Start Auto-completion

        protected virtual void StartAutoComplete(TextEditor editor, TextCompositionEventArgs e)
        {
            //Only do something if auto-complete not active
            if (this.State != AutoCompleteState.None) return;

            this._editor = editor;

            if (e.Text.Length == 1)
            {
                char c = e.Text[0];
                if (c == '_')
                {
                    StartBNodeCompletion(editor, e);
                }
                else if (c == '<')
                {
                    StartUriCompletion(editor, e);
                }
                else if (c == '#')
                {
                    StartCommentCompletion(editor, e);
                }
                else if (c == '"')
                {
                    StartLiteralCompletion(editor, e);
                }
                else if (c == '.' || c == ',' || c == ';')
                {
                    this.State = AutoCompleteState.None;
                }
            }

            if (this.State == AutoCompleteState.None || this.State == AutoCompleteState.Disabled) return;

            //If no completion window in use have to manually set the Start Offset and Length
            if (this._c == null)
            {
                this.StartOffset = editor.CaretOffset - 1;
            }
        }

        protected virtual void StartLiteralCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (this.TemporaryState == AutoCompleteState.Literal || this.TemporaryState == AutoCompleteState.LongLiteral)
            {
                this.TemporaryState = AutoCompleteState.None;
                this.State = AutoCompleteState.None;
            }
            else
            {
                this.State = AutoCompleteState.Literal;
            }
        }

        protected virtual void StartCommentCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            this.State = AutoCompleteState.Comment;
        }

        protected virtual void StartUriCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            this.State = AutoCompleteState.Uri;
        }

        protected virtual void StartBNodeCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            this.State = AutoCompleteState.BNode;

            this.SetupCompletionWindow(editor.TextArea);
            this.AddCompletionData(new NewBlankNode(this._bnodemap.GetNextID()));
            this.AddCompletionData(this._bnodes);
            this._c.StartOffset--;
            this.StartOffset = this._c.StartOffset;

            this._c.CompletionList.SelectItem(this.CurrentText);

            this._c.Show();
        }

        #endregion

        #region Auto-completion

        public override void TryAutoComplete(TextEditor editor, TextCompositionEventArgs e)
        {
            //Don't do anything if auto-complete not currently active
            if (this.State == AutoCompleteState.Disabled || this.State == AutoCompleteState.Inserted) return;

            try
            {

                //If not currently auto-completing see if we can start a completion
                if (this.State == AutoCompleteState.None)
                {
                    this.StartAutoComplete(editor, e);
                    return;
                }

                //If Length is less than zero then user has moved the caret so we'll abort our completion and start a new one
                if (this.Length < 0)
                {
                    this.AbortAutoComplete();
                    //TODO: Should probably call DetectState() here once it's properly implemented
                    this.StartAutoComplete(editor, e);
                    return;
                }

                //Length should never be 1 when we get here
                if (this._c == null && this.Length == 1)
                {
                    this.State = AutoCompleteState.None;
                    this.StartAutoComplete(editor, e);
                    return;
                }

                if (e.Text.Length > 0)
                {
                    switch (this.State)
                    {
                        case AutoCompleteState.BNode:
                            TryBNodeCompletion(editor, e);
                            break;

                        case AutoCompleteState.Uri:
                            TryUriCompletion(editor, e);
                            break;

                        case AutoCompleteState.Literal:
                            TryLiteralCompletion(editor, e);
                            break;

                        case AutoCompleteState.Comment:
                            TryCommentCompletion(editor, e);
                            break;

                        default:
                            //Nothing to do as no other auto-completion is implemented yet
                            break;
                    }
                }
            }
            catch
            {
                //If any kind of error occurs abort auto-completion
                this.AbortAutoComplete();
            }
        }

        protected virtual void TryLiteralCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (this.IsNewLine(e.Text)) this.AbortAutoComplete();

            if (e.Text == "\"")
            {
                if (this.CurrentText.Length == 2)
                {
                    //Possibly end of the Literal, have to wait and see
                }
                else
                {
                    //Is this an escaped "?
                    if (!this.CurrentText.Substring(this.CurrentText.Length - 2, 2).Equals("\\\""))
                    {
                        //Not escaped so terminates the literal
                        this.FinishAutoComplete(true, false);
                    }
                }
            }
            else if (this.CurrentText.Length == 3)
            {
                char last = this.CurrentText[this.CurrentText.Length - 1];
                if (Char.IsWhiteSpace(last) || Char.IsPunctuation(last))
                {
                    this.FinishAutoComplete(true, true);
                }
            }
        }

        protected virtual void TryUriCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (e.Text == ">")
            {
                if (!this.CurrentText.Substring(this.CurrentText.Length - 2, 2).Equals("\\>"))
                {
                    //End of a URI so exit auto-complete
                    this.FinishAutoComplete(true, false);
                }
            }
        }

        protected virtual void TryBNodeCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (this.IsNewLine(e.Text)) this.FinishAutoComplete(true, false);

            char c = e.Text[0];
            if (Char.IsWhiteSpace(c) || (Char.IsPunctuation(c) && c != '_' && c != '-' && c != ':'))
            {
                this.AbortAutoComplete();
                this.DetectBlankNodes(editor);
                return;
            }

            if (!this.IsValidPartialBlankNodeID(this.CurrentText.ToString()))
            {
                //Not a BNode ID so close the window
                this.AbortAutoComplete();
                this.DetectBlankNodes(editor);
            }
        }

        protected virtual void TryCommentCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (this.IsNewLine(e.Text))
            {
                this.FinishAutoComplete();
            }
        }

        public override void EndAutoComplete(TextEditor editor)
        {
            if (this.State != AutoCompleteState.Inserted) return;
            this.State = AutoCompleteState.None;

            if (editor != null)
            {

                //Take State Specific Post insertion actions
                int offset = editor.SelectionStart + editor.SelectionLength;
                switch (this.LastCompletion)
                {
                    case AutoCompleteState.BNode:
                        this.DetectBlankNodes(editor);
                        break;
                }
            }

            this.LastCompletion = AutoCompleteState.None;
            //this._temp = AutoCompleteState.None;
        }

        #endregion

        #region Helper Functions

        public virtual bool IsValidPartialBlankNodeID(String value)
        {
            if (value.Equals(String.Empty))
            {
                //Can't be empty
                return false;
            }
            else if (value.Equals("_:"))
            {
                return true;
            }
            else if (value.Length > 2 && value.StartsWith("_:"))
            {
                value = value.Substring(2);
                char[] cs = value.ToCharArray();
                if (Char.IsDigit(cs[0]) || cs[0] == '-' || cs[0] == '_')
                {
                    //Can't start with a Digit, Hyphen or Underscore
                    return false;
                }
                else
                {
                    //Otherwise OK
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        protected bool IsNewLine(String text)
        {
            return text.Equals("\n") || text.Equals("\r") || text.Equals("\r\n") || text.Equals("\n\r");
        }

        #endregion

        public override object Clone()
        {
            return new NTriplesAutoCompleter();
        }
    }
}
