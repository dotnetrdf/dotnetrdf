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
    public class TurtleAutoCompleter : BaseAutoCompleter
    {
        private const String PrefixDeclaration = "prefix";
        private const String BaseDeclaration = "base";
        protected String PrefixRegexPattern = @"@prefix\s+(\p{L}(\p{L}|\p{N}|-|_)*)?:\s+<((\\>|[^>])*)>\s*\.";
        protected String BlankNodePattern = @"_:\p{L}(\p{L}|\p{N}|-|_)*";
        private LoadNamespaceTermsDelegate _namespaceLoader = new LoadNamespaceTermsDelegate(AutoCompleteManager.LoadNamespaceTerms);

        protected List<ICompletionData> _keywords = new List<ICompletionData>()
        {
            new KeywordCompletionData("a", "Shorthand for RDF type predicate - equivalent to the URI <" + RdfSpecsHelper.RdfType + ">"),
            new KeywordCompletionData("false", "Keyword representing false - equivalent to the literal \"false\"^^<" + XmlSpecsHelper.XmlSchemaDataTypeBoolean + ">"),
            new KeywordCompletionData("true", "Keyword representing true - equivalent to the literal \"true\"^^<" + XmlSpecsHelper.XmlSchemaDataTypeBoolean + ">")
        };

        private OffsetScopedNamespaceMapper _nsmap = new OffsetScopedNamespaceMapper(true);
        private Dictionary<String, List<NamespaceTerm>> _namespaceTerms = new Dictionary<string, List<NamespaceTerm>>();
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

            //Look for defined Prefixes - we have to clear the list of namespaces and prefixes since they might have been altered
            this.DetectNamespaces(editor);
            this.DetectBlankNodes(editor);
        }

        protected virtual void DetectNamespaces(TextEditor editor)
        {
            this._nsmap.Clear();

            foreach (Match m in Regex.Matches(editor.Text, PrefixRegexPattern))
            {
                //Set the Offset for this Namespace so it gets properly scoped later on
                this._nsmap.CurrentOffset = m.Index + m.Length;

                String prefix = m.Groups[1].Value;
                String nsUri = m.Groups[3].Value;
                try
                {
                    this._nsmap.AddNamespace(prefix, new Uri(nsUri));
                    if (!this._namespaceTerms.ContainsKey(nsUri))
                    {
                        this._namespaceLoader.BeginInvoke(nsUri, this.LoadNamespaceTermsCallback, null);
                    }
                }
                catch (UriFormatException)
                {
                    //Ignore
                }
            }
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

        private delegate IEnumerable<NamespaceTerm> LoadNamespaceTermsDelegate(String namespaceUri);

        private void LoadNamespaceTermsCallback(IAsyncResult result)
        {
            try
            {
                IEnumerable<NamespaceTerm> terms = (IEnumerable<NamespaceTerm>)this._namespaceLoader.EndInvoke(result);

                if (terms.Any())
                {
                    String nsUri = terms.First().NamespaceUri;

                    if (!this._namespaceTerms.ContainsKey(nsUri)) this._namespaceTerms.Add(nsUri, new List<NamespaceTerm>());

                    this._namespaceTerms[nsUri].AddRange(terms);
                }
            }
            catch (Exception ex)
            {
                //Ignore exceptions
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        #endregion

        #region Start Auto-completion

        protected virtual void StartAutoComplete(TextEditor editor, TextCompositionEventArgs e)
        {
            //Only do something if auto-complete not active
            if (this.State != AutoCompleteState.None) return;

            this._editor = editor;

            if (e.Text == "@")
            {
                StartDeclarationCompletion(editor, e);
            }
            else if (e.Text.Length == 1)
            {
                char c = e.Text[0];
                if (Char.IsLetter(c))
                {
                    StartKeywordOrQNameCompletion(editor, e);
                }
                else if (c == '_')
                {
                    StartBNodeCompletion(editor, e);
                }
                else if (c == ':')
                {
                    StartQNameCompletion(editor, e);
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

        protected virtual void StartQNameCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            this.State = AutoCompleteState.QName;

            this.SetupCompletionWindow(editor.TextArea);
            this._c.StartOffset--;
            this.StartOffset = this._c.StartOffset;
            this.AddQNameCompletionData();

            this._c.Show();
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

        protected virtual void StartKeywordOrQNameCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            this.SetupCompletionWindow(editor.TextArea);
            this._c.StartOffset--;
            this.StartOffset = this._c.StartOffset;

            //Backtrack start point
            BacktrackStartOffset(editor, s => this.IsValidPartialKeyword(s) || this.IsValidPartialQName(s));

            if (this.IsValidPartialKeyword(this.CurrentText))
            {
                this.State = AutoCompleteState.KeywordOrQName;
                this.AddCompletionData(this._keywords);
                this.AddQNameCompletionData();
            }
            else if (this.IsValidPartialQName(this.CurrentText))
            {
                this.State = AutoCompleteState.QName;
                this.AddQNameCompletionData();
            }
            this._c.CompletionList.SelectItem(this.CurrentText);

            this._c.Show();
        }

        protected virtual void StartDeclarationCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            this.State = AutoCompleteState.Declaration;
            this.SetupCompletionWindow(editor.TextArea);
            this.AddCompletionData(new NewBaseDeclaration());
            String nextPrefix = "ns0";
            int nextPrefixID = 0;
            while (this._nsmap.HasNamespace(nextPrefix))
            {
                nextPrefixID++;
                nextPrefix = "ns" + nextPrefixID;
            }
            this.AddCompletionData(new NewPrefixDeclaration(nextPrefix));
            this.AddCompletionData(new NewDefaultPrefixDeclaration());
            this.AddCompletionData(AutoCompleteManager.PrefixData);
            this._c.CloseWhenCaretAtBeginning = false;
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
                        case AutoCompleteState.Declaration:
                            TryDeclarationCompletion(editor, e);
                            break;

                        case AutoCompleteState.Base:
                            TryBaseCompletion(editor, e);
                            break;

                        case AutoCompleteState.Prefix:
                            TryPrefixCompletion(editor, e);
                            break;

                        case AutoCompleteState.KeywordOrQName:
                            TryKeywordOrQNameCompletion(editor, e);
                            break;

                        case AutoCompleteState.QName:
                            TryQNameCompletion(editor, e);
                            break;

                        case AutoCompleteState.BNode:
                            TryBNodeCompletion(editor, e);
                            break;

                        case AutoCompleteState.Uri:
                            TryUriCompletion(editor, e);
                            break;

                        case AutoCompleteState.Literal:
                            TryLiteralCompletion(editor, e);
                            break;

                        case AutoCompleteState.LongLiteral:
                            TryLongLiteralCompletion(editor, e);
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
                //If any kind of error occurs just abort auto-completion
                this.AbortAutoComplete();
            }
        }

        protected virtual void TryLongLiteralCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (e.Text == "\"")
            {
                //Is this an escaped "?
                if (!this.CurrentText.Substring(this.CurrentText.Length - 2, 2).Equals("\\\""))
                {
                    //Not escaped so terminate the literal if the buffer ends in 3 " and the length is >= 6
                    if (this.CurrentText.Length >= 6 && this.CurrentText.Substring(this.CurrentText.Length - 3, 3).Equals("\"\"\""))
                    {
                        this.FinishAutoComplete(true, false);
                    }
                }
            }
        }

        protected virtual void TryLiteralCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (this.IsNewLine(e.Text)) this.AbortAutoComplete();

            if (e.Text == "\"")
            {
                if (this.CurrentText.Length == 2)
                {
                    //Might be a long literal so have to wait and see
                }
                else if (this.CurrentText.Length == 3)
                {
                    if (this.CurrentText.ToString().Equals("\"\"\""))
                    {
                        //Switch to long literal mode
                        this.State = AutoCompleteState.LongLiteral;
                    }
                    else if (!this.CurrentText.Substring(this.CurrentText.Length - 2, 2).Equals("\\\""))
                    {
                        //Not an escape so ends the literal
                        this.FinishAutoComplete(true, false);
                    }
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
                    //White Space/Punctuation means we've left the empty literal
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

        protected virtual void TryQNameCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (this.IsValidPartialKeyword(this.CurrentText))
            {
                //Can switch back to Keyword/QName mode if user backtracks
                this.State = AutoCompleteState.KeywordOrQName;
                this.AddCompletionData(this._keywords.Where(k => k is KeywordCompletionData));
                this.TryKeywordOrQNameCompletion(editor, e);
                this._c.CompletionList.SelectItem(this.CurrentText);
                return;
            }

            if (this.IsNewLine(e.Text) || !this.IsValidPartialQName(this.CurrentText))
            {
                //Not a QName so close the window
                this.State = AutoCompleteState.None;
                this.AbortAutoComplete();
            }
        }

        protected virtual void TryKeywordOrQNameCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (this.IsNewLine(e.Text)) this.FinishAutoComplete(true, true);

            char c = e.Text[0];
            if (Char.IsWhiteSpace(c) || (Char.IsPunctuation(c) && c != '_' && c != '-' && c != ':'))
            {
                this.AbortAutoComplete();
                return;
            }

            if (!this.IsValidPartialKeyword(this.CurrentText.ToString()) && !this.IsValidPartialQName(this.CurrentText.ToString()))
            {
                //Not a keyword/Qname so close the window
                this.AbortAutoComplete();
            }
            else if (!this.IsValidPartialKeyword(this.CurrentText.ToString()))
            {
                //No longer a possible keyword
                this.State = AutoCompleteState.QName;

                //Strip keywords from the auto-complete list
                this.RemoveCompletionData(data => data is KeywordCompletionData);
            }
        }

        protected virtual void TryDeclarationCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (this.IsNewLine(e.Text)) this.AbortAutoComplete();

            char c = e.Text[0];
            if (Char.IsWhiteSpace(c) || Char.IsPunctuation(c)) this.AbortAutoComplete();

            int testLength = Math.Min(PrefixDeclaration.Length, this.CurrentText.Length);
            if (PrefixDeclaration.Substring(0, testLength).Equals(this.CurrentText.Substring(0, testLength)))
            {
                this.State = AutoCompleteState.Prefix;
                return;
            }
            testLength = Math.Min(BaseDeclaration.Length, this.CurrentText.Length);
            if (BaseDeclaration.Substring(0, testLength).Equals(this.CurrentText.Substring(0, testLength)))
            {
                this.State = AutoCompleteState.Base;
            }
        }

        protected virtual void TryPrefixCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (this.IsNewLine(e.Text))
            {
                this.AbortAutoComplete();
                this.DetectNamespaces(editor);
            }

            if (e.Text.Equals("."))
            {
                String testText = editor.Document.GetText(this.StartOffset - 1, this.Length + 1);
                if (Regex.IsMatch(testText, PrefixRegexPattern))
                {
                    this.FinishAutoComplete(false, true);
                    this.DetectNamespaces(editor);
                    return;
                }
            }

            int testLength = Math.Min(PrefixDeclaration.Length, this.CurrentText.Length);
            if (!PrefixDeclaration.Substring(0, testLength).Equals(this.CurrentText.Substring(0, testLength)))
            {
                //Not a prefix declaration so close the window
                this.AbortAutoComplete();
                this.DetectNamespaces(editor);
            }
        }

        protected virtual void TryBaseCompletion(TextEditor editor, TextCompositionEventArgs e)
        {
            if (this.IsNewLine(e.Text))
            {
                this.AbortAutoComplete();
            }

            char c = e.Text[0];
            if (Char.IsWhiteSpace(c) || Char.IsPunctuation(c)) this.AbortAutoComplete();

            int testLength = Math.Min(BaseDeclaration.Length, this.CurrentText.Length);
            if (!BaseDeclaration.Substring(0, testLength).Equals(this.CurrentText.Substring(0, testLength)))
            {
                //Not a base declaration so close the window
                this.AbortAutoComplete();
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
                    case AutoCompleteState.Prefix:
                    case AutoCompleteState.Base:
                    case AutoCompleteState.Declaration:
                        editor.Document.Insert(offset, ".");
                        this.DetectNamespaces(editor);
                        break;

                    case AutoCompleteState.KeywordOrQName:
                    case AutoCompleteState.Keyword:
                    case AutoCompleteState.QName:
                        if (!editor.Document.GetText(offset - 1, 1).Equals(" "))
                        {
                            editor.Document.Insert(offset, " ");
                        }
                        break;

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

        public virtual bool IsValidPartialKeyword(String value)
        {
            foreach (KeywordCompletionData keyword in this._keywords.OfType<KeywordCompletionData>())
            {
                int testLength = Math.Min(keyword.Text.Length, value.Length);
                if (testLength > keyword.Text.Length) continue;
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

        protected bool IsNewLine(String text)
        {
            return text.Equals("\n") || text.Equals("\r") || text.Equals("\r\n") || text.Equals("\n\r");
        }

        protected virtual void AddQNameCompletionData()
        {
            //Set Current Offset to scope the Namespace Mapper properly
            this._nsmap.CurrentOffset = this.StartOffset;

            //Generate all available QNames
            List<ICompletionData> qnames = new List<ICompletionData>();
            foreach (String prefix in this._nsmap.Prefixes)
            {
                String nsUri = this._nsmap.GetNamespaceUri(prefix).ToString();
                if (this._namespaceTerms.ContainsKey(nsUri))
                {
                    foreach (NamespaceTerm term in this._namespaceTerms[nsUri])
                    {
                        if (term.Label.Equals(String.Empty))
                        {
                            qnames.Add(new QNameCompletionData(prefix + ":" + term.Term, "QName for the URI " + nsUri + term.Term));
                        }
                        else
                        {
                            qnames.Add(new QNameCompletionData(prefix + ":" + term.Term, term.Label));
                        }
                    }
                }
            }
            qnames.Sort();
            this.AddCompletionData(qnames);
        }

        protected void BacktrackStartOffset(TextEditor editor, Func<String, bool> testFunc)
        {
            int offset = this.StartOffset - 1;
            int backtracked = 1;
            if (offset >= 0)
            {
                String test = editor.Document.GetText(offset, this.Length + backtracked);
                while (testFunc(test))
                {
                    if (offset > 0)
                    {
                        offset--;
                        backtracked++;
                    }
                    else
                    {
                        break;
                    }
                    test = editor.Document.GetText(offset, this.Length + backtracked);
                }
                offset++;
                if (offset != this.StartOffset)
                {
                    this._c.StartOffset = offset;
                    this.StartOffset = offset;
                }
            }
        }

        #endregion

        public override object Clone()
        {
            return new TurtleAutoCompleter();
        }
    }
}
