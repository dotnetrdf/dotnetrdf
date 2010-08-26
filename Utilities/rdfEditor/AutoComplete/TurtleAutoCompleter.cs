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
    public class TurtleAutoCompleter : BaseAutoCompleter
    {
        private const String PrefixDeclaration = "@prefix";
        private CompletionWindow _c;
        private StringBuilder _buffer = new StringBuilder();
        private AutoCompleteState _temp = AutoCompleteState.None;
        private TextEditor _editor;

        private List<ICompletionData> _keywords = new List<ICompletionData>()
        {
            new KeywordCompletionData("true", "Keyword representing true - equivalent to the literal \"true\"^^<" + XmlSpecsHelper.XmlSchemaDataTypeBoolean + ">"),
            new KeywordCompletionData("false", "Keyword representing false - equivalent to the literal \"false\"^^<" + XmlSpecsHelper.XmlSchemaDataTypeBoolean + ">"),
            new KeywordCompletionData("a", "Shorthand for RDF type predicate - equivalent to the URI <" + RdfSpecsHelper.RdfType + ">")
        };

        private void SetupCompletionWindow(TextArea area)
        {
            this._c = new CompletionWindow(area);
            this._c.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            this._c.CompletionList.InsertionRequested += new EventHandler(CompletionList_InsertionRequested);
            //this._c.CompletionList.IsFiltering = false;
        }

        private void CompletionList_InsertionRequested(object sender, EventArgs e)
        {
            this.LastCompletion = this._temp;
            if (this.LastCompletion != AutoCompleteState.None)
            {
                this.State = AutoCompleteState.Inserted;
                this.EndAutoComplete(this._editor);
            }
        }

        private void SetupCompletionWindow(TextArea area, IEnumerable<ICompletionData> data)
        {
            this.SetupCompletionWindow(area);
            foreach (ICompletionData dataItem in data)
            {
                this._c.CompletionList.CompletionData.Add(dataItem);
            }
        }

        private void CompletionWindowClosed(Object sender, EventArgs e)
        {
            //Reset State
            this._temp = this.State;
            this._c = null;
            this._buffer.Remove(0, this._buffer.Length); 
        }

        public override void StartAutoComplete(TextEditor editor, TextCompositionEventArgs e)
        {
            //Only do something if auto-complete not active
            if (this.State != AutoCompleteState.None) return;

            //Force the buffer to always be clear when starting an auto-completion
            if (this._buffer.Length > 0) this._buffer.Remove(0, this._buffer.Length);

            this._editor = editor;

            if (e.Text == "@")
            {
                //Prefix Completion
                this._buffer.Append(e.Text);
                this.State = AutoCompleteState.Prefix;
                this.SetupCompletionWindow(editor.TextArea, AutoCompleteManager.PrefixData);
                this._c.Show();
                this._c.Closed += new EventHandler(this.CompletionWindowClosed);
            }
            else if (e.Text.Length == 1)
            {
                char c = e.Text[0];
                if (Char.IsLetter(c))
                {
                    this.State = AutoCompleteState.KeywordOrQName;
                    this.SetupCompletionWindow(editor.TextArea, this._keywords);
                    this._c.Show();
                }
                else if (c == '_')
                {
                    this.State = AutoCompleteState.BNode;
                }
                else if (c == ':')
                {
                    this.State = AutoCompleteState.QName;
                    //this.SetupCompletionWindow(editor.TextArea);
                    //this._c.Show();
                }
                else if (c == '<')
                {
                    this.State = AutoCompleteState.URI;
                }

                //Add to Buffer if we've entered an auto-completion state
                if (this.State != AutoCompleteState.None)
                {
                    this._buffer.Append(e.Text);
                }
            }
        }

        public override void TryAutoComplete(TextCompositionEventArgs e)
        {
            //Don't do anything if auto-complete not currently active
            if (this.State == AutoCompleteState.None || this.State == AutoCompleteState.Disabled || this.State == AutoCompleteState.Inserted) return;

            if (e.Text.Length > 0 && this._c != null)
            {
                this._buffer.Append(e.Text);
                switch (this.State)
                {
                    case AutoCompleteState.Prefix:
                        int testLength = Math.Min(PrefixDeclaration.Length, this._buffer.Length);
                        if (!PrefixDeclaration.Substring(0, testLength).Equals(this._buffer.ToString(0, testLength)))
                        {
                            //Not a prefix declaration so close the window
                            this.State = AutoCompleteState.None;
                            this._c.Close();
                        }
                        break;

                    case AutoCompleteState.KeywordOrQName:
                        if (!TurtleSpecsHelper.IsValidPlainLiteral(this._buffer.ToString()) && !TurtleSpecsHelper.IsValidQName(this._buffer.ToString()))
                        {
                            //Not a keyword/Qname so close the window
                            this.State = AutoCompleteState.None;
                            this._c.Close();
                        }
                        else if (!TurtleSpecsHelper.IsValidPlainLiteral(this._buffer.ToString()))
                        {
                            //No longer a possible keyword
                            this.State = AutoCompleteState.QName;

                            //Strip keywords from the auto-complete list
                            for (int i = 0; i < this._c.CompletionList.CompletionData.Count; i++)
                            {
                                if (this._c.CompletionList.CompletionData[i] is KeywordCompletionData)
                                {
                                    this._c.CompletionList.CompletionData.RemoveAt(i);
                                }
                            }
                        }
                        break;

                    case AutoCompleteState.QName:
                        if (!TurtleSpecsHelper.IsValidQName(this._buffer.ToString()))
                        {
                            //Not a QName so close the window
                            this.State = AutoCompleteState.None;
                            this._c.Close();
                        }
                        break;

                    case AutoCompleteState.BNode:
                        if (!WriterHelper.IsValidBlankNodeID(this._buffer.ToString()))
                        {
                            //Not a BNode ID so close the window
                            this.State = AutoCompleteState.None;
                            this._c.Close();
                        }
                        break;

                    case AutoCompleteState.URI:
                        if (e.Text == ">")
                        {
                            if (!this._buffer.ToString(this._buffer.Length - 2, 2).Equals("\\>"))
                            {
                                //End of a URI so exit auto-complete
                                this.State = AutoCompleteState.None;
                            }
                        }
                        break;

                    default:
                        //Nothing to do as no other auto-completion is implemented yet
                        break;
                }
            }
            else
            {
                //If we get here then some kind of control character was hit?
                this.State = AutoCompleteState.None;
            }
        }

        public override void EndAutoComplete(TextEditor editor)
        {
            if (this.State != AutoCompleteState.Inserted) return;
            this.State = AutoCompleteState.None;

            //Take State Specific Post insertion actions
            int offset = editor.SelectionStart + editor.SelectionLength;
            switch (this.LastCompletion)
            {
                case AutoCompleteState.Prefix:
                    editor.Document.Insert(offset, ".");
                    break;

                case AutoCompleteState.KeywordOrQName:
                case AutoCompleteState.Keyword:
                case AutoCompleteState.QName:
                    editor.Document.Insert(offset, " ");
                    break;
            }

            this.LastCompletion = AutoCompleteState.None;
            this._temp = AutoCompleteState.None;
            this._editor = null;
        }
    }
}
