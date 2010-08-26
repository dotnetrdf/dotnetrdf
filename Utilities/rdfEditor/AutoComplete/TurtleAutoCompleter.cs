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

        #region Completion Window Management

        private void SetupCompletionWindow(TextArea area)
        {
            this._c = new CompletionWindow(area);
            this._c.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            this._c.CompletionList.InsertionRequested += new EventHandler(CompletionList_InsertionRequested);
        }

        private void AbortAutoComplete()
        {
            this.State = AutoCompleteState.None;
            this._temp = AutoCompleteState.None;
            if (this._c != null) this._c.Close();
        }

        private void FinishAutoComplete(bool saveCompletion, bool noAutoEnd)
        {
            if (saveCompletion) this._temp = this.State;
            if (noAutoEnd)
            {
                this.LastCompletion = this._temp;
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

        private void FinishAutoComplete()
        {
            this.LastCompletion = this._temp;
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

        #endregion

        public override void Initialise(TextEditor editor)
        {
            
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
                    //KeyConverter converter = new KeyConverter();
                    //if (e.Device is KeyboardDevice)
                    //{
                    //    this._c.CompletionList.HandleKey(new KeyEventArgs((KeyboardDevice)e.Device, e.Device.ActiveSource, e.Timestamp, (Key)converter.ConvertFromString(e.Text)));
                    //}
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
                else if (c == '#')
                {
                    this.State = AutoCompleteState.Comment;
                }
                else if (c == '"')
                {
                    if (this._temp == AutoCompleteState.Literal || this._temp == AutoCompleteState.LongLiteral)
                    {
                        this._temp = AutoCompleteState.None;
                        this.State = AutoCompleteState.None;
                    }
                    else
                    {
                        this.State = AutoCompleteState.Literal;
                    }
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

            if (e.Text.Length > 0)
            {
                this._buffer.Append(e.Text);
                switch (this.State)
                {
                    case AutoCompleteState.Prefix:
                        int testLength = Math.Min(PrefixDeclaration.Length, this._buffer.Length);
                        if (!PrefixDeclaration.Substring(0, testLength).Equals(this._buffer.ToString(0, testLength)))
                        {
                            //Not a prefix declaration so close the window
                            this.AbortAutoComplete();
                        }
                        break;

                    case AutoCompleteState.KeywordOrQName:
                        if (!this.IsValidPartialKeyword(this._buffer.ToString()) && !this.IsValidPartialQName(this._buffer.ToString()))
                        {
                            //Not a keyword/Qname so close the window
                            this.AbortAutoComplete();
                        }
                        else if (!this.IsValidPartialKeyword(this._buffer.ToString()))
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
                        if (!this.IsValidPartialQName(this._buffer.ToString()))
                        {
                            //Not a QName so close the window
                            this.State = AutoCompleteState.None;
                            this.AbortAutoComplete();
                        }
                        break;

                    case AutoCompleteState.BNode:
                        if (!this.IsValidPartialBlankNodeID(this._buffer.ToString()))
                        {
                            //Not a BNode ID so close the window
                            this.AbortAutoComplete();
                        }
                        break;

                    case AutoCompleteState.URI:
                        if (e.Text == ">")
                        {
                            if (!this._buffer.ToString(this._buffer.Length - 2, 2).Equals("\\>"))
                            {
                                //End of a URI so exit auto-complete
                                this.FinishAutoComplete(true, true);
                            }
                        }
                        break;

                    case AutoCompleteState.Literal:
                        if (e.Text == "\"")
                        {
                            if (this._buffer.Length == 2)
                            {
                                //Might be a long literal so have to wait and see
                            }
                            else if (this._buffer.Length == 3)
                            {
                                char last = this._buffer[this._buffer.Length - 1];
                                if (this._buffer.ToString().Equals("\"\"\""))
                                {
                                    //Switch to long literal mode
                                    this.State = AutoCompleteState.LongLiteral;
                                }
                                else if (Char.IsWhiteSpace(last) || Char.IsPunctuation(last))
                                {
                                    //White Space/Punctuation means we've left the empty literal
                                    this.FinishAutoComplete(true, true);
                                }
                                else if (!this._buffer.ToString(this._buffer.Length-2,2).Equals("\\\""))
                                {
                                    //Not an escape so ends the literal
                                    this.FinishAutoComplete(true, true);
                                }
                            }
                            else
                            {
                                //Is this an escaped "?
                                if (!this._buffer.ToString(this._buffer.Length - 2, 2).Equals("\\\""))
                                {
                                    //Not escaped so terminates the literal
                                    this.FinishAutoComplete(true, true);
                                }
                            }
                        }
                        break;

                    case AutoCompleteState.LongLiteral:
                        if (e.Text == "\"")
                        {
                            //Is this an escaped "?
                            if (!this._buffer.ToString(this._buffer.Length - 2, 2).Equals("\\\""))
                            {
                                //Not escaped so terminate the literal if the buffer ends in 3 "
                                if (this._buffer.ToString(this._buffer.Length - 3, 3).Equals("\"\"\""))
                                {
                                    this.FinishAutoComplete(true, true);
                                }
                            }
                        }
                        break;

                    default:
                        //Nothing to do as no other auto-completion is implemented yet
                        break;
                }
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
                case AutoCompleteState.Literal:
                case AutoCompleteState.LongLiteral:
                    editor.Document.Insert(offset, " ");
                    break;
            }

            this.LastCompletion = AutoCompleteState.None;
            //this._temp = AutoCompleteState.None;
            this._editor = null;
        }

        public virtual bool IsValidPartialKeyword(String value)
        {
            foreach (KeywordCompletionData keyword in this._keywords.OfType<KeywordCompletionData>())
            {
                int testLength = Math.Min(keyword.Text.Length, value.Length);
                if (keyword.Text.Substring(0, testLength).Equals(value.Substring(0, testLength))) return true;
            }
            return false;
        }

        public virtual bool IsValidPartialQName(String value)
        {
            String ns, localname;
            if (value.Contains(':'))
            {
                ns = value.Substring(0, value.IndexOf(':'));
                localname = value.Substring(value.IndexOf(':') + 1);
            }
            else
            {
                ns = value;
                localname = String.Empty;
            }

            //Namespace Validation
            if (!ns.Equals(String.Empty))
            {
                //Allowed empty Namespace
                if (ns.StartsWith("-"))
                {
                    //Can't start with a -
                    return false;
                }
                else
                {
                    char[] nchars = ns.ToCharArray();
                    if (XmlSpecsHelper.IsNameStartChar(nchars[0]) && nchars[0] != '_')
                    {
                        if (nchars.Length > 1)
                        {
                            for (int i = 1; i < nchars.Length; i++)
                            {
                                //Not a valid Name Char
                                if (!XmlSpecsHelper.IsNameChar(nchars[i])) return false;
                                if (nchars[i] == '.') return false;
                            }
                            //If we reach here the Namespace is OK
                        }
                        else
                        {
                            //Only 1 Character which was valid so OK
                        }
                    }
                    else
                    {
                        //Doesn't start with a valid Name Start Char
                        return false;
                    }
                }
            }

            //Local Name Validation
            if (!localname.Equals(String.Empty))
            {
                //Allowed empty Local Name
                char[] lchars = localname.ToCharArray();

                if (XmlSpecsHelper.IsNameStartChar(lchars[0]))
                {
                    if (lchars.Length > 1)
                    {
                        for (int i = 1; i < lchars.Length; i++)
                        {
                            //Not a valid Name Char
                            if (!XmlSpecsHelper.IsNameChar(lchars[i])) return false;
                            if (lchars[i] == '.') return false;
                        }
                        //If we reach here the Local Name is OK
                    }
                    else
                    {
                        //Only 1 Character which was valid so OK
                    }
                }
                else
                {
                    //Not a valid Name Start Char
                    return false;
                }
            }

            //If we reach here then it's all valid
            return true;
        }

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
    }
}
