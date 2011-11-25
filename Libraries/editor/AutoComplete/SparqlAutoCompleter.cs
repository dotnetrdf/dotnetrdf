using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using VDS.RDF.Utilities.Editor.AutoComplete.Data;
using VDS.RDF.Utilities.Editor.AutoComplete.Vocabularies;

namespace VDS.RDF.Utilities.Editor.AutoComplete
{
    public abstract class SparqlAutoCompleter<T>
        : TurtleAutoCompleter<T>
    {
        private SparqlQuerySyntax _syntax;
        private HashSet<ICompletionData> _vars = new HashSet<ICompletionData>();
        protected String VariableRegexPattern = @"[?$](_|\p{L}|\d)(_|-|\p{L}|\p{N})*";

        public SparqlAutoCompleter(ITextEditorAdaptor<T> editor)
            : this(editor, SparqlQuerySyntax.Sparql_1_1) { }

        public SparqlAutoCompleter(ITextEditorAdaptor<T> editor, SparqlQuerySyntax? syntax)
            : base(editor)
        {
            //Alter the Regex patterns
            this.PrefixRegexPattern = this.PrefixRegexPattern.Substring(1, this.PrefixRegexPattern.Length-6);
            this.BlankNodePattern = @"_:(\p{L}|\d)(\p{L}|\p{N}|-|_)*";

            //Add Prefix Definitions to Keywords
            this._keywords.Add(new SparqlStyleBaseDeclarationData());
            this._keywords.Add(new SparqlStyleDefaultPrefixDeclarationData());
            foreach (VocabularyDefinition vocab in AutoCompleteManager.Vocabularies)
            {
                this._keywords.Add(new SparqlStylePrefixDeclarationData(vocab.Prefix, vocab.NamespaceUri));
            }

            //If not Query Syntax don't add any Query Keywords
            if (syntax == null) return;

            //Add Keywords relevant to the Syntax
            this._syntax = (SparqlQuerySyntax)syntax;
            foreach (String keyword in SparqlSpecsHelper.SparqlQuery10Keywords)
            {
                this._keywords.Add(new KeywordData(keyword));
                this._keywords.Add(new KeywordData(keyword.ToLower()));
            }

            if (syntax != SparqlQuerySyntax.Sparql_1_0)
            {
                foreach (String keyword in SparqlSpecsHelper.SparqlQuery11Keywords)
                {
                    this._keywords.Add(new KeywordData(keyword));
                    this._keywords.Add(new KeywordData(keyword.ToLower()));
                }
            }

            //Sort the Keywords
            this._keywords.Sort();
        }

        protected override bool CanDeclareNewTerms
        {
            get
            {
                return false;
            }
        }

        protected override void DetectStateInternal()
        {
            base.DetectStateInternal();
            this.DetectVariables();
        }

        protected virtual void DetectVariables()
        {
            this._vars.Clear();
            foreach (Match m in Regex.Matches(this._editor.Text, VariableRegexPattern))
            {
                this._vars.Add(new VariableData(m.Value));
            }
        }

        protected override void StartDeclarationCompletion(String newText)
        {
            //We don't start declarations like Turtle does so don't do anything here
            return;
        }

        protected virtual void StartAlternateLiteralCompletion(String newText)
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

        protected virtual void StartVariableCompletion(String newText)
        {
            this.State = AutoCompleteState.Variable;
            this._editor.Suggest(this._vars);
        }

        public override void TryAutoComplete(String newText)
        {
            base.TryAutoComplete(newText);

            //Don't do anything if auto-complete not currently active
            if (this.State == AutoCompleteState.Disabled || this.State == AutoCompleteState.Inserted) return;

            if (this.State == AutoCompleteState.None)
            {
                if (newText.Length == 1)
                {
                    char c = newText[0];
                    if (Char.IsLetter(c))
                    {
                        StartKeywordOrQNameCompletion(newText);
                    }
                    else
                    {
                        switch (c)
                        {
                            case '_':
                                StartBNodeCompletion(newText);
                                break;
                            case ':':
                                StartQNameCompletion(newText);
                                break;
                            case '<':
                                StartUriCompletion(newText);
                                break;
                            case '#':
                                StartCommentCompletion(newText);
                                break;
                            case '"':
                                StartLiteralCompletion(newText);
                                break;
                            case '\'':
                                StartAlternateLiteralCompletion(newText);
                                break;
                            case '?':
                            case '$':
                                StartVariableCompletion(newText);
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
            } 

            //If not currently auto-completing then do nothing - if the call to base.TryAutoComplete() couldn't
            //do anything and our start code didn't do anything we aren't doing auto-completion
            if (this.State == AutoCompleteState.None) return;

            try
            {

                //If Length is less than zero then user has moved the caret so we'll abort our completion and start a new one
                if (this.Length < 0)
                {
                    this._editor.EndSuggestion();
                    this.TryAutoComplete(newText);
                    return;
                }

                if (newText.Length > 0)
                {
                    switch (this.State)
                    {
                        case AutoCompleteState.AlternateLiteral:
                            this.TryAlternateLiteralCompletion(newText);
                            break;

                        case AutoCompleteState.AlternateLongLiteral:
                            this.TryAlternateLongLiteralCompletion(newText);
                            break;

                        case AutoCompleteState.Variable:
                            this.TryVariableCompletion(newText);
                            break;

                        default:
                            //No other auto-completion supported
                            break;
                    }
                }
            }
            catch
            {
                //If any kind of error occurs just abort auto-completion
                this.State = AutoCompleteState.None;
                this._editor.EndSuggestion();
            }
        }

        protected virtual void TryAlternateLongLiteralCompletion(String newText)
        {
            if (newText == "'")
            {
                //Is this an escaped '?
                if (!this.CurrentText.Substring(this.CurrentText.Length - 2, 2).Equals("\\'"))
                {
                    //Not escaped so terminate the literal if the buffer ends in 3 ' and the length is >= 6
                    if (this.CurrentText.Length >= 6 && this.CurrentText.Substring(this.CurrentText.Length - 3, 3).Equals("'''"))
                    {
                        this.LastCompletion = AutoCompleteState.LongLiteral;
                        this._editor.EndSuggestion();
                    }
                }
            }
        }

        protected virtual void TryAlternateLiteralCompletion(String newText)
        {
            if (this.IsNewLine(newText))
            {
                this.State = AutoCompleteState.None;
                this._editor.EndSuggestion();
            }

            if (newText == "'")
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
                        this.LastCompletion = AutoCompleteState.AlternateLiteral;
                        this._editor.EndSuggestion();
                    }
                    else if (!this.CurrentText.Substring(this.CurrentText.Length - 2, 2).Equals("\\'"))
                    {
                        //Not an escape so ends the literal
                        this.LastCompletion = AutoCompleteState.AlternateLiteral;
                        this._editor.EndSuggestion();
                    }
                }
                else
                {
                    //Is this an escaped '?
                    if (!this.CurrentText.Substring(this.CurrentText.Length - 2, 2).Equals("\\'"))
                    {
                        //Not escaped so terminates the literal
                        this.LastCompletion = AutoCompleteState.AlternateLiteral;
                        this._editor.EndSuggestion();
                    }
                }
            }
        }

        protected virtual void TryVariableCompletion(String newText)
        {
            if (this.IsNewLine(newText))
            {
                this.State = AutoCompleteState.None;
                this._editor.EndSuggestion();
            }

            char c = newText[0];
            if (this.Length > 1)
            {
                if (Char.IsWhiteSpace(c) || (Char.IsPunctuation(c) && c != '_' && c != '-'))
                {
                    this.LastCompletion = AutoCompleteState.Variable;
                    this._editor.EndSuggestion();
                    this.DetectVariables();
                    return;
                }
            }

            if (!this.IsValidPartialVariableName(this.CurrentText.ToString()))
            {
                //Not a Variable so close the window
                this.State = AutoCompleteState.None;
                this._editor.EndSuggestion();
                this.DetectVariables();
            }
        }

        protected override void TryDeclarationCompletion(String newText)
        {
            //We don't do declarations like Turtle does so don't do anything here
            return;
        }

        protected override void TryPrefixCompletion(String newText)
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
    }

    public class Sparql11AutoCompleter<T>
        : SparqlAutoCompleter<T>
    {
        public Sparql11AutoCompleter(ITextEditorAdaptor<T> editor)
            : base(editor, SparqlQuerySyntax.Sparql_1_1) { }
    }

    public class Sparql10AutoCompleter<T>
        : SparqlAutoCompleter<T>
    {
        public Sparql10AutoCompleter(ITextEditorAdaptor<T> editor)
            : base(editor, SparqlQuerySyntax.Sparql_1_0) { }
    }


}
