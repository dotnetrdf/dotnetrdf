/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Utilities.Editor.AutoComplete.Data;
using VDS.RDF.Utilities.Editor.AutoComplete.Vocabularies;

namespace VDS.RDF.Utilities.Editor.AutoComplete
{
    /// <summary>
    /// Auto-complete implementation for Turtle
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    public class TurtleAutoCompleter<T>
        : BaseAutoCompleter<T>
    {
        private const String PrefixDeclaration = "prefix";
        private const String BaseDeclaration = "base";
        /// <summary>
        /// Regular expression pattern for prefix declarations
        /// </summary>
        protected String PrefixRegexPattern = @"@prefix\s+(\p{L}(\p{L}|\p{N}|-|_)*)?:\s+<((\\>|[^>])*)>\s*\.";
        /// <summary>
        /// Regular expression pattern for blank node IDs
        /// </summary>
        protected String BlankNodePattern = @"_:\p{L}(\p{L}|\p{N}|-|_)*";
        private LoadNamespaceTermsDelegate _namespaceLoader = new LoadNamespaceTermsDelegate(AutoCompleteManager.LoadNamespaceTerms);

        /// <summary>
        /// Keywords that the completer permits
        /// </summary>
        protected List<ICompletionData> _keywords = new List<ICompletionData>()
        {
            new KeywordData("a", "Shorthand for RDF type predicate - equivalent to the URI <" + RdfSpecsHelper.RdfType + ">"),
            new KeywordData("false", "Keyword representing false - equivalent to the literal \"false\"^^<" + XmlSpecsHelper.XmlSchemaDataTypeBoolean + ">"),
            new KeywordData("true", "Keyword representing true - equivalent to the literal \"true\"^^<" + XmlSpecsHelper.XmlSchemaDataTypeBoolean + ">")
        };

        private OffsetScopedNamespaceMapper _nsmap = new OffsetScopedNamespaceMapper(true);
        private Dictionary<String, List<NamespaceTerm>> _namespaceTerms = new Dictionary<string, List<NamespaceTerm>>();
        private HashSet<ICompletionData> _bnodes = new HashSet<ICompletionData>();
        private BlankNodeMapper _bnodemap = new BlankNodeMapper();

        /// <summary>
        /// Creates a new auto-completer
        /// </summary>
        /// <param name="editor">Text Editor</param>
        public TurtleAutoCompleter(ITextEditorAdaptor<T> editor)
            : base(editor) { }

        #region State Detection

        /// <summary>
        /// Detects the state of the auto-completer
        /// </summary>
        protected override void DetectStateInternal()
        {
            //Look for defined Prefixes - we have to clear the list of namespaces and prefixes since they might have been altered
            this.DetectNamespaces();
            this.DetectBlankNodes();
        }

        /// <summary>
        /// Detects declared namespaces
        /// </summary>
        protected virtual void DetectNamespaces()
        {
            this._nsmap.Clear();

            foreach (Match m in Regex.Matches(this._editor.Text, PrefixRegexPattern))
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

            //TODO: Add auto-detection of declared classes and properties
            if (this.CanDeclareNewTerms)
            {
                //TODO: Need to get the relevant parser and then invoke it on this text using TermDetectionHandler
            }
        }

        /// <summary>
        /// Gets whether new terms can be declared
        /// </summary>
        protected virtual bool CanDeclareNewTerms
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Detects declared blank nodes
        /// </summary>
        protected virtual void DetectBlankNodes()
        {
            this._bnodes.Clear();

            foreach (Match m in Regex.Matches(this._editor.Text, BlankNodePattern))
            {
                String id = m.Value;
                if (this._bnodes.Add(new BlankNodeData(id)))
                {
                    this._bnodemap.CheckID(ref id);
                }                
            }
        }

        /// <summary>
        /// Delegate for requesting namespace terms
        /// </summary>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns>Namespace Terms</returns>
        private delegate IEnumerable<NamespaceTerm> LoadNamespaceTermsDelegate(String namespaceUri);

        /// <summary>
        /// Callback to fire when namespace terms are received
        /// </summary>
        /// <param name="result">Async Result</param>
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

        /// <summary>
        /// Start literal completion
        /// </summary>
        /// <param name="newText">New text</param>
        protected virtual void StartLiteralCompletion(String newText)
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

        /// <summary>
        /// Start comment completion
        /// </summary>
        /// <param name="newText">New text</param>
        protected virtual void StartCommentCompletion(String newText)
        {
            this.State = AutoCompleteState.Comment;
        }

        /// <summary>
        /// Start URI completion
        /// </summary>
        /// <param name="newText">New text</param>
        protected virtual void StartUriCompletion(String newText)
        {
            this.State = AutoCompleteState.Uri;
        }

        /// <summary>
        /// Start QName completion
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void StartQNameCompletion(String newText)
        {
            this.State = AutoCompleteState.QName;

            this._editor.Suggest(this.GetQNameCompletionData());
        }

        /// <summary>
        /// Start Blank Node completion
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void StartBNodeCompletion(String newText)
        {
            this.State = AutoCompleteState.BNode;

            this._editor.Suggest(new NewBlankNodeData(this._bnodemap.GetNextID()).AsEnumerable<ICompletionData>().Concat(this._bnodes));
        }

        /// <summary>
        /// Start Keyword or QName completion
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void StartKeywordOrQNameCompletion(String newText)
        {
            //Backtrack start point
            BacktrackStartOffset(s => this.IsValidPartialKeyword(s) || this.IsValidPartialQName(s));

            if (this.IsValidPartialKeyword(this.CurrentText))
            {
                this.State = AutoCompleteState.KeywordOrQName;
                this._editor.Suggest(this._keywords.Concat(this.GetQNameCompletionData()));
            }
            else if (this.IsValidPartialQName(this.CurrentText))
            {
                this.State = AutoCompleteState.QName;
                this._editor.Suggest(this.GetQNameCompletionData());
            }
        }

        /// <summary>
        /// Start declaration completion
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void StartDeclarationCompletion(String newText)
        {
            this.State = AutoCompleteState.Declaration;

            //Build up the Possible Declarations
            List<ICompletionData> data = new List<ICompletionData>();

            //Add Base Declaration
            data.Add(new TurtleStyleBaseDeclarationData());

            //Add New Namespace Declarations
            data.Add(new TurtleStyleDefaultPrefixDeclarationData());
            String nextPrefix = "ns0";
            int nextPrefixID = 0;
            while (this._nsmap.HasNamespace(nextPrefix))
            {
                nextPrefixID++;
                nextPrefix = "ns" + nextPrefixID;
            }
            data.Add(new TurtleStylePrefixDeclarationData(nextPrefix, "Enter new Namespace URI here"));

            //Add Existing Namespace Declarations
            foreach (VocabularyDefinition vocab in AutoCompleteManager.Vocabularies)
            {
                data.Add(new TurtleStylePrefixDeclarationData(vocab.Prefix, vocab.NamespaceUri));
            }

            this._editor.Suggest(data);
        }

        #endregion

        #region Auto-completion

        /// <summary>
        /// Try to auto-complete
        /// </summary>
        /// <param name="newText">New Text</param>
        public override void TryAutoComplete(String newText)
        {
            //Don't do anything if auto-complete not currently active
            if (this.State == AutoCompleteState.Disabled || this.State == AutoCompleteState.Inserted) return;

            if (this.State == AutoCompleteState.None)
            {
                //If State is None then Start a New Completion

                //Actual Start Offset is always -1 from where we are as the user has typed a character
                this.StartOffset = this._editor.CaretOffset - 1;

                if (newText == "@")
                {
                    StartDeclarationCompletion(newText);
                }
                else if (newText.Length == 1)
                {
                    char c = newText[0];
                    if (Char.IsLetter(c))
                    {
                        StartKeywordOrQNameCompletion(newText);
                    }
                    else if (c == '_')
                    {
                        StartBNodeCompletion(newText);
                    }
                    else if (c == ':')
                    {
                        StartQNameCompletion(newText);
                    }
                    else if (c == '<')
                    {
                        StartUriCompletion(newText);
                    }
                    else if (c == '#')
                    {
                        StartCommentCompletion(newText);
                    }
                    else if (c == '"')
                    {
                        StartLiteralCompletion(newText);
                    }
                    else if (c == '.' || c == ',' || c == ';')
                    {
                        this.State = AutoCompleteState.None;
                    }
                }

                if (this.State == AutoCompleteState.None || this.State == AutoCompleteState.Disabled) return;
            }
            else
            {
                try
                {
                    //If Length is less than zero then user has moved the caret so we'll abort our completion and start a new one
                    if (this.Length < 0 || this.Length == 1)
                    {
                        this._editor.EndSuggestion();
                        this.TryAutoComplete(newText);
                        return;
                    }

                    if (newText.Length > 0)
                    {
                        switch (this.State)
                        {
                            case AutoCompleteState.Declaration:
                                TryDeclarationCompletion(newText);
                                break;

                            case AutoCompleteState.Base:
                                TryBaseCompletion(newText);
                                break;

                            case AutoCompleteState.Prefix:
                                TryPrefixCompletion(newText);
                                break;

                            case AutoCompleteState.KeywordOrQName:
                                TryKeywordOrQNameCompletion(newText);
                                break;

                            case AutoCompleteState.QName:
                                TryQNameCompletion(newText);
                                break;

                            case AutoCompleteState.BNode:
                                TryBNodeCompletion(newText);
                                break;

                            case AutoCompleteState.Uri:
                                TryUriCompletion(newText);
                                break;

                            case AutoCompleteState.Literal:
                                TryLiteralCompletion(newText);
                                break;

                            case AutoCompleteState.LongLiteral:
                                TryLongLiteralCompletion(newText);
                                break;

                            case AutoCompleteState.Comment:
                                TryCommentCompletion(newText);
                                break;

                            default:
                                //Nothing to do as no other auto-completion is implemented by this completer
                                //Derived implementations may call us first and then add in their extra cases if we don't hit anything
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
        }

        /// <summary>
        /// Try to auto-complete a long literal
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void TryLongLiteralCompletion(String newText)
        {
            if (newText == "\"")
            {
                //Is this an escaped "?
                if (!this.CurrentText.Substring(this.CurrentText.Length - 2, 2).Equals("\\\""))
                {
                    //Not escaped so terminate the literal if the buffer ends in 3 " and the length is >= 6
                    if (this.CurrentText.Length >= 6 && this.CurrentText.Substring(this.CurrentText.Length - 3, 3).Equals("\"\"\""))
                    {
                        this.LastCompletion = AutoCompleteState.LongLiteral;
                        this._editor.EndSuggestion();
                    }
                }
            }
        }

        /// <summary>
        /// Try to auto-complete a literal
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void TryLiteralCompletion(String newText)
        {
            if (this.IsNewLine(newText))
            {
                this.State = AutoCompleteState.None;
            }

            if (newText == "\"")
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
                        this.LastCompletion = AutoCompleteState.Literal;
                        this._editor.EndSuggestion();
                    }
                }
                else
                {
                    //Is this an escaped "?
                    if (!this.CurrentText.Substring(this.CurrentText.Length - 2, 2).Equals("\\\""))
                    {
                        //Not escaped so terminates the literal
                        this.LastCompletion = this.State;
                        this._editor.EndSuggestion();
                    }
                }
            }
            else if (this.CurrentText.Length == 3)
            {
                char last = this.CurrentText[this.CurrentText.Length - 1];
                if (Char.IsWhiteSpace(last) || Char.IsPunctuation(last))
                {
                    //White Space/Punctuation means we've left the empty literal
                    this.LastCompletion = AutoCompleteState.Literal;
                    this._editor.EndSuggestion();
                }
            }
        }

        /// <summary>
        /// Try to complete a URI
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void TryUriCompletion(String newText)
        {
            if (newText == ">")
            {
                if (!this.CurrentText.Substring(this.CurrentText.Length - 2, 2).Equals("\\>"))
                {
                    //End of a URI so exit auto-complete
                    this.LastCompletion = AutoCompleteState.Uri;
                    this._editor.EndSuggestion();
                }
            }
        }

        /// <summary>
        /// Try to complete a BNode
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void TryBNodeCompletion(String newText)
        {
            if (this.IsNewLine(newText))
            {
                this.LastCompletion = AutoCompleteState.BNode;
                this._editor.EndSuggestion();
            }

            char c = newText[0];
            if (Char.IsWhiteSpace(c) || (Char.IsPunctuation(c) && c != '_' && c != '-' && c != ':'))
            {
                this.LastCompletion = AutoCompleteState.BNode;
                this._editor.EndSuggestion();
                this.DetectBlankNodes();
                return;
            }

            if (!this.IsValidPartialBlankNodeID(this.CurrentText.ToString()))
            {
                //Not a BNode ID so close the window
                this._editor.EndSuggestion();
                this.DetectBlankNodes();
            }
        }

        /// <summary>
        /// Try to complete a QName
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void TryQNameCompletion(String newText)
        {
            if (this.IsValidPartialKeyword(this.CurrentText))
            {
                //Can switch back to Keyword/QName mode if user backtracks
                this.State = AutoCompleteState.KeywordOrQName;
                this._editor.Suggest(this._keywords.Where(d => d is KeywordData));
                this.TryKeywordOrQNameCompletion(newText);
                return;
            }

            if (this.IsNewLine(newText) || !this.IsValidPartialQName(this.CurrentText))
            {
                //Not a QName so close the window
                this.State = AutoCompleteState.None;
                this._editor.EndSuggestion();
            }
        }

        /// <summary>
        /// Try to complete a Keyword or QName
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void TryKeywordOrQNameCompletion(String newText)
        {
            if (this.IsNewLine(newText))
            {
                this.LastCompletion = this.State;
                this._editor.EndSuggestion();
            }

            char c = newText[0];
            if (Char.IsWhiteSpace(c) || (Char.IsPunctuation(c) && c != '_' && c != '-' && c != ':'))
            {
                this._editor.EndSuggestion();
                return;
            }

            if (!this.IsValidPartialKeyword(this.CurrentText.ToString()) && !this.IsValidPartialQName(this.CurrentText.ToString()))
            {
                //Not a keyword/Qname so close the window
                this._editor.EndSuggestion();
            }
            else if (!this.IsValidPartialKeyword(this.CurrentText.ToString()))
            {
                //No longer a possible keyword
                this.State = AutoCompleteState.QName;
            }
        }

        /// <summary>
        /// Try to complete a declaration
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void TryDeclarationCompletion(String newText)
        {
            if (this.IsNewLine(newText))
            {
                this.LastCompletion = AutoCompleteState.Declaration;
                this._editor.EndSuggestion();
            }

            char c = newText[0];
            if (Char.IsWhiteSpace(c) || Char.IsPunctuation(c))
            {
                this.LastCompletion = AutoCompleteState.Declaration;
                this._editor.EndSuggestion();
            }

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

        /// <summary>
        /// Try to complete a prefix declaration
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void TryPrefixCompletion(String newText)
        {
            if (this.IsNewLine(newText))
            {
                this.LastCompletion = AutoCompleteState.Prefix;
                this._editor.EndSuggestion();
                this.DetectNamespaces();
            }

            if (newText.Equals("."))
            {
                String testText = this._editor.GetText(this.StartOffset - 1, this.Length + 1);
                if (Regex.IsMatch(testText, PrefixRegexPattern))
                {
                    this.LastCompletion = AutoCompleteState.Declaration;
                    this._editor.EndSuggestion();
                    this.DetectNamespaces();
                    return;
                }
            }

            int testLength = Math.Min(PrefixDeclaration.Length, this.CurrentText.Length);
            if (!PrefixDeclaration.Substring(0, testLength).Equals(this.CurrentText.Substring(0, testLength)))
            {
                //Not a prefix declaration so close the window
                this._editor.EndSuggestion();
                this.DetectNamespaces();
            }
        }

        /// <summary>
        /// Try to complete a base declaration
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void TryBaseCompletion(String newText)
        {
            if (this.IsNewLine(newText))
            {
                this._editor.EndSuggestion();
            }

            char c = newText[0];
            if (Char.IsWhiteSpace(c) || Char.IsPunctuation(c))
            {
                this._editor.EndSuggestion();
            }

            int testLength = Math.Min(BaseDeclaration.Length, this.CurrentText.Length);
            if (!BaseDeclaration.Substring(0, testLength).Equals(this.CurrentText.Substring(0, testLength)))
            {
                //Not a base declaration so close the window
                this._editor.EndSuggestion();
            }
        }

        /// <summary>
        /// Try to complete a comment
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void TryCommentCompletion(String newText)
        {
            if (this.IsNewLine(newText))
            {
                this.LastCompletion = AutoCompleteState.Comment;
                this._editor.EndSuggestion();
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Is something a valid partial keyword?
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns></returns>
        protected virtual bool IsValidPartialKeyword(String value)
        {
            foreach (KeywordData keyword in this._keywords.OfType<KeywordData>())
            {
                int testLength = Math.Min(keyword.InsertionText.Length, value.Length);
                if (testLength > keyword.InsertionText.Length) continue;
                if (keyword.InsertionText.Substring(0, testLength).Equals(value.Substring(0, testLength))) return true;
            }
            return false;
        }

        /// <summary>
        /// Is something a valid partial QName?
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns></returns>
        protected virtual bool IsValidPartialQName(String value)
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

        /// <summary>
        /// Is something a valid partial Blank Node ID?
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns></returns>
        protected virtual bool IsValidPartialBlankNodeID(String value)
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

        /// <summary>
        /// Is something a new line
        /// </summary>
        /// <param name="text">Value</param>
        /// <returns></returns>
        protected bool IsNewLine(String text)
        {
            return text.Equals("\n") || text.Equals("\r") || text.Equals("\r\n") || text.Equals("\n\r");
        }

        /// <summary>
        /// Get QName completion data
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<ICompletionData> GetQNameCompletionData()
        {
            //Set Current Offset to scope the Namespace Mapper properly
            this._nsmap.CurrentOffset = this.StartOffset;

            //Generate all available QNames
            List<ICompletionData> qnames = new List<ICompletionData>();
            foreach (String prefix in this._nsmap.Prefixes)
            {
                String nsUri = this._nsmap.GetNamespaceUri(prefix).AbsoluteUri;
                if (this._namespaceTerms.ContainsKey(nsUri))
                {
                    foreach (NamespaceTerm term in this._namespaceTerms[nsUri])
                    {
                        if (term.Label.Equals(String.Empty))
                        {
                            qnames.Add(new QNameData(prefix + ":" + term.Term, "QName for the URI " + nsUri + term.Term));
                        }
                        else
                        {
                            qnames.Add(new QNameData(prefix + ":" + term.Term, term.Label));
                        }
                    }
                }
            }
            qnames.Sort();
            return qnames;
        }

        /// <summary>
        /// Backtrack the start offset while a function is true
        /// </summary>
        /// <param name="testFunc">Test function</param>
        protected void BacktrackStartOffset(Func<String, bool> testFunc)
        {
            int offset = this.StartOffset - 1;
            int backtracked = 1;
            if (offset >= 0)
            {
                String test = this._editor.GetText(offset, this.Length + backtracked);
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
                    test = this._editor.GetText(offset, this.Length + backtracked);
                }
                offset++;
                if (offset != this.StartOffset)
                {
                    this.StartOffset = offset;
                }
            }
        }

        #endregion
    }
}
