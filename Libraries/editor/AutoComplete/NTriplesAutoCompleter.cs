/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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

namespace VDS.RDF.Utilities.Editor.AutoComplete
{
    /// <summary>
    /// Auto-completer for NTriples
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    public class NTriplesAutoCompleter<T>
        : BaseAutoCompleter<T>
    {
        protected String BlankNodePattern = @"_:\p{L}(\p{L}|\p{N}|-|_)*";

        private HashSet<ICompletionData> _bnodes = new HashSet<ICompletionData>();
        private BlankNodeMapper _bnodemap = new BlankNodeMapper();

        /// <summary>
        /// Creates a new auto-completer
        /// </summary>
        /// <param name="editor">Text Editor</param>
        public NTriplesAutoCompleter(ITextEditorAdaptor<T> editor)
            : base(editor) { }

        #region State Detection

        /// <summary>
        /// Detects the auto-complete state
        /// </summary>
        protected override void DetectStateInternal()
        {
            //Look for Blank Nodes
            this.DetectBlankNodes();
        }

        /// <summary>
        /// Detect declared blank nodes
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

        #endregion

        #region Start Auto-completion

        /// <summary>
        /// Start literal completion
        /// </summary>
        /// <param name="newText">New Text</param>
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
        /// <param name="newText">New Text</param>
        protected virtual void StartCommentCompletion(String newText)
        {
            this.State = AutoCompleteState.Comment;
        }

        /// <summary>
        /// Start URI completion
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void StartUriCompletion(String newText)
        {
            this.State = AutoCompleteState.Uri;
        }

        /// <summary>
        /// Start BNode completion
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void StartBNodeCompletion(String newText)
        {
            this.State = AutoCompleteState.BNode;
            this._editor.Suggest(new NewBlankNodeData(this._bnodemap.GetNextID()).AsEnumerable<ICompletionData>().Concat(this._bnodes));
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
                if (newText.Length == 1)
                {
                    char c = newText[0];
                    if (c == '_')
                    {
                        StartBNodeCompletion(newText);
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
                    if (this.Length < 0)
                    {
                        this._editor.EndSuggestion();
                        this.State = AutoCompleteState.None;
                        this.TryAutoComplete(newText);
                        return;
                    }

                    if (newText.Length > 0)
                    {
                        switch (this.State)
                        {
                            case AutoCompleteState.BNode:
                                TryBNodeCompletion(newText);
                                break;

                            case AutoCompleteState.Uri:
                                TryUriCompletion(newText);
                                break;

                            case AutoCompleteState.Literal:
                                TryLiteralCompletion(newText);
                                break;

                            case AutoCompleteState.Comment:
                                TryCommentCompletion(newText);
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
                    this.State = AutoCompleteState.None;
                    this._editor.EndSuggestion();
                }
            }
        }

        /// <summary>
        /// Try literal completion
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void TryLiteralCompletion(String newText)
        {
            if (this.IsNewLine(newText))
            {
                this.State = AutoCompleteState.None;
                this._editor.EndSuggestion();
            }

            if (newText == "\"")
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
                        this.LastCompletion = AutoCompleteState.Literal;
                        this._editor.EndSuggestion();
                    }
                }
            }
            else if (this.CurrentText.Length == 3)
            {
                char last = this.CurrentText[this.CurrentText.Length - 1];
                if (Char.IsWhiteSpace(last) || Char.IsPunctuation(last))
                {
                    this.LastCompletion = AutoCompleteState.Literal;
                    this._editor.EndSuggestion();
                }
            }
        }

        /// <summary>
        /// Try URI completion
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
        /// Try Blank Node completion
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void TryBNodeCompletion(String newText)
        {
            if (this.IsNewLine(newText))
            {
                this.State = AutoCompleteState.None;
                this._editor.EndSuggestion();
            }

            char c = newText[0];
            if (Char.IsWhiteSpace(c) || (Char.IsPunctuation(c) && c != '_' && c != '-' && c != ':'))
            {
                this.LastCompletion = AutoCompleteState.BNode;
                this.DetectBlankNodes();
                this._editor.EndSuggestion();
                return;
            }

            if (!this.IsValidPartialBlankNodeID(this.CurrentText.ToString()))
            {
                //Not a BNode ID so close the window
                this.State = AutoCompleteState.None;
                this._editor.EndSuggestion();
                this.DetectBlankNodes();
            }
        }

        /// <summary>
        /// Try comment completion
        /// </summary>
        /// <param name="newText">New Text</param>
        protected virtual void TryCommentCompletion(String newText)
        {
            if (this.IsNewLine(newText))
            {
                this.State = AutoCompleteState.None;
                this._editor.EndSuggestion();
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Is something a valid partial blank node ID?
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
        /// Is something a new line?
        /// </summary>
        /// <param name="text">Value</param>
        /// <returns></returns>
        protected bool IsNewLine(String text)
        {
            return text.Equals("\n") || text.Equals("\r") || text.Equals("\r\n") || text.Equals("\n\r");
        }

        #endregion
    }
}
