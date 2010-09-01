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
using VDS.RDF.Query;
using VDS.RDF.Writing;
using rdfEditor.AutoComplete.Data;

namespace rdfEditor.AutoComplete
{
    public class SparqlAutoCompleter : TurtleAutoCompleter
    {
        private SparqlQuerySyntax _syntax;
        private HashSet<ICompletionData> _vars = new HashSet<ICompletionData>();
        protected String VariableRegexPattern = @"[?$](_|\p{L}|\d)(_|-|\p{L}|\p{N})*";

        public SparqlAutoCompleter()
            : this(SparqlQuerySyntax.Sparql_1_1) { }

        public SparqlAutoCompleter(SparqlQuerySyntax? syntax)
        {
            //Alter the Regex patterns
            this.PrefixRegexPattern = this.PrefixRegexPattern.Substring(1, this.PrefixRegexPattern.Length-6);
            this.BlankNodePattern = @"_:(\p{L}|\d)(\p{L}|\p{N}|-|_)*";

            //Add Prefix Definitions to Keywords
            this._keywords.AddRange(AutoCompleteManager.PrefixData);

            //If not Query Syntax don't add any Query Keywords
            if (syntax == null) return;

            //Add Keywords relevant to the Syntax
            this._syntax = (SparqlQuerySyntax)syntax;
            foreach (String keyword in SparqlSpecsHelper.SparqlQuery10Keywords)
            {
                this._keywords.Add(new KeywordCompletionData(keyword));
                this._keywords.Add(new KeywordCompletionData(keyword.ToLower()));
            }

            if (syntax != SparqlQuerySyntax.Sparql_1_0)
            {
                foreach (String keyword in SparqlSpecsHelper.SparqlQuery11Keywords)
                {
                    this._keywords.Add(new KeywordCompletionData(keyword));
                    this._keywords.Add(new KeywordCompletionData(keyword.ToLower()));
                }
            }

            //Sort the Keywords
            this._keywords.Sort();
        }

        public override void DetectState(TextEditor editor)
        {
            base.DetectState(editor);

            this.DetectVariables(editor);
        }

        protected virtual void DetectVariables(TextEditor editor)
        {
            this._vars.Clear();
            foreach (Match m in Regex.Matches(editor.Text, VariableRegexPattern))
            {
                this._vars.Add(new VariableCompletionData(m.Value));
            }
        }

        protected override void CompletionWindowKeyDown(object sender, KeyEventArgs e)
        {
            base.CompletionWindowKeyDown(sender, e);
            if (e.Handled) return;

            if (this.State == AutoCompleteState.Keyword || this.State == AutoCompleteState.KeywordOrQName)
            {
                if (e.Key == Key.D9 && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
                {
                    this._c.CompletionList.RequestInsertion(e);
                }
                else if (e.Key == Key.OemOpenBrackets && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
                {
                    this._c.CompletionList.RequestInsertion(e);
                }
            }
            else if (this.State == AutoCompleteState.Variable || this.State == AutoCompleteState.BNode)
            {
                if (e.Key == Key.D0 && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
                {
                    this._c.CompletionList.RequestInsertion(e);
                }
            }
        }

        protected override void StartAutoComplete(TextEditor editor, TextCompositionEventArgs e)
        {
            //Only do something if auto-complete not active
            if (this.State != AutoCompleteState.None) return;

            this._editor = editor;

            if (e.Text.Length == 1)
            {
                char c = e.Text[0];
                if (Char.IsLetter(c))
                {
                    StartKeywordOrQNameCompletion(editor, e);
                }
                else 
                {
                    switch (c)
                    {
                        case '_':
                            StartBNodeCompletion(editor, e);
                            break;
                        case ':':
                            StartQNameCompletion(editor, e);
                            break;
                        case '<':
                            StartUriCompletion(editor, e);
                            break;
                        case '#':
                            StartCommentCompletion(editor, e);
                            break;
                        case '"':
                            StartLiteralCompletion(editor, e);
                            break;
                        case '\'':
                            StartAlternateLiteralCompletion(editor, e);
                            break;
                        case '?':
                        case '$':
                            StartVariableCompletion(editor, e);
                            break;
                        case '.':
                        case ',':
                        case ';':
                            this.State = AutoCompleteState.None;
                            break;
                    }
                }
            }

            if (this.State == AutoCompleteState.None || this.State == AutoCompleteState.Disabled) return;

            //If no completion window in use have to manually set the Start Offset and Length
            if (this._c == null)
            {
                this.StartOffset = editor.CaretOffset - 1;
            }
        }

        protected override void StartDeclarationCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            //We don't start declarations like Turtle does so don't do anything here
            return;
        }

        protected virtual void StartAlternateLiteralCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (this.TemporaryState == AutoCompleteState.AlternateLiteral || this.TemporaryState == AutoCompleteState.AlternateLongLiteral)
            {
                this.TemporaryState = AutoCompleteState.None;
                this.State = AutoCompleteState.None;
            }
            else
            {
                this.State = AutoCompleteState.AlternateLiteral;
            }
        }

        protected virtual void StartVariableCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            this.State = AutoCompleteState.Variable;

            this.SetupCompletionWindow(editor.TextArea, this._vars);
            this._c.StartOffset--;
            this.StartOffset = this._c.StartOffset;

            this._c.CompletionList.SelectItem(this.CurrentText);

            this._c.Show();
        }

        public override void TryAutoComplete(TextEditor editor, TextCompositionEventArgs e)
        {
            base.TryAutoComplete(editor, e);

            //Don't do anything if auto-complete not currently active
            if (this.State == AutoCompleteState.Disabled || this.State == AutoCompleteState.Inserted) return;

            //If not currently auto-completing then do nothing - note that the call to base.TryAutoComplete() may
            //already have caused us to try a StartAutoComplete() call which will have called our override so
            //if we aren't in a completion State then we can't do anything
            if (this.State == AutoCompleteState.None) return;

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
                    case AutoCompleteState.AlternateLiteral:
                        this.TryAlternateLiteralCompletion(editor, e);
                        break;

                    case AutoCompleteState.AlternateLongLiteral:
                        this.TryAlternateLongLiteralCompletion(editor, e);
                        break;

                    case AutoCompleteState.Variable:
                        this.TryVariableCompletion(editor, e);
                        break;

                    default:
                        //No other auto-completion supported
                        break;
                }
            }
        }

        protected virtual void TryAlternateLongLiteralCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (e.Text == "'")
            {
                //Is this an escaped '?
                if (!this.CurrentText.Substring(this.CurrentText.Length - 2, 2).Equals("\\'"))
                {
                    //Not escaped so terminate the literal if the buffer ends in 3 ' and the length is >= 6
                    if (this.CurrentText.Length >= 6 && this.CurrentText.Substring(this.CurrentText.Length - 3, 3).Equals("'''"))
                    {
                        this.FinishAutoComplete(true, false);
                    }
                }
            }
        }

        protected virtual void TryAlternateLiteralCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (this.IsNewLine(e.Text)) this.AbortAutoComplete();

            if (e.Text == "'")
            {
                if (this.CurrentText.Length == 2)
                {
                    //Might be a long literal so have to wait and see
                }
                else if (this.CurrentText.Length == 3)
                {
                    char last = this.CurrentText[this.CurrentText.Length - 1];
                    if (this.CurrentText.ToString().Equals("'''"))
                    {
                        //Switch to long literal mode
                        this.State = AutoCompleteState.AlternateLongLiteral;
                    }
                    else if (Char.IsWhiteSpace(last) || Char.IsPunctuation(last))
                    {
                        //White Space/Punctuation means we've left the empty literal
                        this.FinishAutoComplete(true, true);
                    }
                    else if (!this.CurrentText.Substring(this.CurrentText.Length - 2, 2).Equals("\\'"))
                    {
                        //Not an escape so ends the literal
                        this.FinishAutoComplete(true, false);
                    }
                }
                else
                {
                    //Is this an escaped '?
                    if (!this.CurrentText.Substring(this.CurrentText.Length - 2, 2).Equals("\\'"))
                    {
                        //Not escaped so terminates the literal
                        this.FinishAutoComplete(true, false);
                    }
                }
            }
        }

        protected virtual void TryVariableCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (this.IsNewLine(e.Text)) this.FinishAutoComplete(true, false);

            char c = e.Text[0];
            if (this.Length > 1)
            {
                if (Char.IsWhiteSpace(c) || (Char.IsPunctuation(c) && c != '_' && c != '-'))
                {
                    this.AbortAutoComplete();
                    this.DetectVariables(editor);
                    return;
                }
            }

            if (!this.IsValidPartialVariableName(this.CurrentText.ToString()))
            {
                //Not a Variable so close the window
                this.AbortAutoComplete();
                this.DetectVariables(editor);
            }
        }

        protected override void TryDeclarationCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            //We don't do declarations like Turtle does so don't do anything here
            return;
        }

        protected override void TryPrefixCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            //We don't do Prefix declarations like Turtle does so don't do anything here
            return;
        }

        public override bool IsValidPartialQName(string value)
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

                if (XmlSpecsHelper.IsNameStartChar(lchars[0]) || Char.IsNumber(lchars[0]))
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

        public virtual bool IsValidPartialVariableName(String value)
        {
            if (value.Length == 0) return false;

            if (value[0] != '$' && value[0] != '?') return false;

            if (value.Length == 1) return true;

            char[] cs = value.ToCharArray(1, value.Length - 1);

            //First Character must be from PN_CHARS_U or a digit
            char first = cs[0];
            if (Char.IsDigit(first) || SparqlSpecsHelper.IsPNCharU(first))
            {
                if (cs.Length > 1)
                {
                    for (int i = 1; i < cs.Length; i++)
                    {
                        //Middle Chars must be from PN_CHARS or a '.'
                        if (!(cs[i] == '.' || SparqlSpecsHelper.IsPNChar(cs[i])))
                        {
                            return false;
                        }
                        //Can't do the last character specific test because this is only a partial test
                    }

                    return true;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public override void EndAutoComplete(TextEditor editor)
        {
            if (this.State != AutoCompleteState.Inserted) return;

            if (this.LastCompletion == AutoCompleteState.Keyword || this.LastCompletion == AutoCompleteState.KeywordOrQName)
            {
                this.DetectNamespaces(editor);
            }

            base.EndAutoComplete(editor);
        }

        public override object Clone()
        {
            return new SparqlAutoCompleter(this._syntax);
        }
    }
}
