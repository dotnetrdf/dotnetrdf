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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VDS.RDF.Utilities.Editor
{
    /// <summary>
    /// Modes for find and replace
    /// </summary>
    public enum FindReplaceMode
    {
        /// <summary>
        /// Find
        /// </summary>
        Find,

        /// <summary>
        /// Find and Replace
        /// </summary>
        FindAndReplace
    }

    /// <summary>
    /// Find and Replace Scopes
    /// </summary>
    public enum FindAndReplaceScope
    {
        /// <summary>
        /// Current Document
        /// </summary>
        CurrentDocument,

        /// <summary>
        /// Selection
        /// </summary>
        Selection
    }

    /// <summary>
    /// Abstract implementation of find and replace for text editors
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    public abstract class FindAndReplace<T>
    {
        /// <summary>
        /// Creates a new find and replace
        /// </summary>
        public FindAndReplace()
        {
            this.UseRegex = false;
            this.MatchCase = false;
            this.MatchWholeWord = false;
            this.SearchUp = false;
            this.Scope = FindAndReplaceScope.CurrentDocument;

            this.FindText = String.Empty;
            this.ReplaceText = String.Empty;

            this.RecentFindTexts = new BindingList<string>();
            this.RecentReplaceTexts = new BindingList<string>();
        }

        /// <summary>
        /// Indicates whether search should restart from start
        /// </summary>
        /// <returns>True if search should restart from start when end of document is reached, false otherwise</returns>
        protected abstract bool ShouldRestartSearchFromStart();

        /// <summary>
        /// Shows a message
        /// </summary>
        /// <param name="message">Message</param>
        protected abstract void ShowMessage(String message);

        /// <summary>
        /// Gets/Sets the scope
        /// </summary>
        public FindAndReplaceScope Scope { get; set; }

        /// <summary>
        /// Gets/Sets the find text
        /// </summary>
        public String FindText { get; set; }

        /// <summary>
        /// Gets/Sets the replace text
        /// </summary>
        public String ReplaceText { get; set; }

        /// <summary>
        /// Gets/Sets whether to match case
        /// </summary>
        public bool MatchCase { get; set; }

        /// <summary>
        /// Gets/Sets whether to match the whole word
        /// </summary>
        public bool MatchWholeWord { get; set; }

        /// <summary>
        /// Gets/Sets whether to use regex
        /// </summary>
        public bool UseRegex { get; set; }

        /// <summary>
        /// Gets/Sets whether to search upwards instead of downwards in a document
        /// </summary>
        public bool SearchUp { get; set; }

        /// <summary>
        /// Gets/Sets recent find text
        /// </summary>
        public BindingList<String> RecentFindTexts { get; set; }

        /// <summary>
        /// Gets/Sets recent replace text
        /// </summary>
        public BindingList<String> RecentReplaceTexts { get; set; }

        /// <summary>
        /// Finds the first occurrence of the currently configured search in the given text editor
        /// </summary>
        /// <param name="editor">Text Editor</param>
        public void Find(ITextEditorAdaptor<T> editor)
        {
            bool fromStart = (editor.CaretOffset == 0);
            if (!this.FindNext(editor) && !fromStart)
            {
                if (this.ShouldRestartSearchFromStart())
                {
                    editor.CaretOffset = 0;
                    editor.SelectionStart = 0;
                    editor.SelectionLength = 0;
                    this.FindNext(editor);
                }
            }
        }

        /// <summary>
        /// Finds the next occurrence of the currently configured search in the given text editor
        /// </summary>
        /// <param name="editor">Text Editor</param>
        /// <returns>True if a match if found, false otherwise</returns>
        public bool FindNext(ITextEditorAdaptor<T> editor)
        {
            if (this.Scope == FindAndReplaceScope.CurrentDocument || editor.SelectionLength == 0)
            {
                return this.FindNext(editor, -1, editor.Text.Length);
            }
            else
            {
                return this.FindNext(editor, Math.Max(0, editor.SelectionStart - 1), editor.SelectionStart + editor.SelectionLength);
            }
        }

        /// <summary>
        /// Finds the next occurrence of the currently configured search in the given text editor within the specified bounds
        /// </summary>
        /// <param name="editor">Text Editor</param>
        /// <param name="minPos">Minimum Position</param>
        /// <param name="maxPos">Maximum Position</param>
        /// <returns></returns>
        public bool FindNext(ITextEditorAdaptor<T> editor, int minPos, int maxPos)
        {
            if (String.IsNullOrEmpty(this.FindText))
            {
                this.ShowMessage("No Find Text specified");
                return false;
            }
            String find = this.FindText;

            //Validate Regex
            Regex regex = null;
            RegexOptions regexOps = this.MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
            regexOps |= RegexOptions.Multiline;
            regexOps |= RegexOptions.CultureInvariant;
            if (this.UseRegex)
            {
                try
                {
                    regex = new Regex(find, regexOps);
                }
                catch (Exception ex)
                {
                    this.ShowMessage("Regular Expression is malformed - " + ex.Message);
                    return false;
                }
            }

            //Add Search Text to Combo Box for later reuse
            if (!this.RecentFindTexts.Contains(find))
            {
                this.RecentFindTexts.Add(find);
            }

            int start = editor.CaretOffset;
            int pos;
            int length = find.Length;
            StringComparison compareMode = this.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            if (this.SearchUp)
            {
                //Search portion of Document prior to current position
                if (this.UseRegex)
                {
                    MatchCollection ms = regex.Matches(editor.Text.Substring(0, start));
                    if (ms.Count == 0)
                    {
                        pos = -1;
                    }
                    else
                    {
                        pos = ms[ms.Count - 1].Index;
                        length = ms[ms.Count - 1].Length;
                    }
                }
                else
                {
                    pos = editor.Text.Substring(0, start).LastIndexOf(find, compareMode);
                }
            }
            else
            {
                //Search position of Document subsequent to current position (incl. any selected text)
                start += editor.SelectionLength;
                if (this.UseRegex)
                {
                    Match m = regex.Match(editor.Text.Substring(start));
                    if (!m.Success)
                    {
                        pos = -1;
                    }
                    else
                    {
                        pos = start + m.Index;
                        length = m.Length;
                    }
                }
                else
                {
                    pos = editor.Text.IndexOf(find, start, compareMode);
                }
            }

            //If we've found the position of the next highlight it and return true otherwise return false
            if (pos <= -1) return false;

            //Check we meet any document range restrictions
            if (pos < minPos || pos > maxPos)
            {
                return false;
            }

            //If Matching on whole word ensure that their are boundaries before and after the match
            if (this.MatchWholeWord)
            {
                //Check boundary before
                if (pos > 0)
                {
                    char c = editor.Text[pos - 1];
                    if (Char.IsLetterOrDigit(c))
                    {
                        //Not a boundary so adjust start position and recurse
                        editor.CaretOffset = pos + length;
                        if (!this.SearchUp) editor.CaretOffset -= editor.SelectionLength;
                        return this.FindNext(editor);
                    }
                }
                //Check boundary after
                if (pos + length < editor.Text.Length - 1)
                {
                    char c = editor.Text[pos + length];
                    if (Char.IsLetterOrDigit(c))
                    {
                        //Not a boundary so adjust start position and recurse
                        editor.CaretOffset = pos + length - 1;
                        if (!this.SearchUp) editor.CaretOffset -= editor.SelectionLength;
                        return this.FindNext(editor);
                    }
                }
            }

            editor.Select(pos, length);
            editor.CaretOffset = pos;
            editor.ScrollToLine(editor.GetLineByOffset(pos));
            return true;
        }

        /// <summary>
        /// Replace the next occurrence of the currently configured search in the given text editor
        /// </summary>
        /// <param name="editor">Text Editor</param>
        public void Replace(ITextEditorAdaptor<T> editor)
        {
            if (String.IsNullOrEmpty(this.ReplaceText))
            {
                this.ShowMessage("No Replace Text specified");
            }

            //Check whether the relevant Text is already selected
            if (!String.IsNullOrEmpty(this.FindText))
            {
                if (this.FindText.Equals(editor.GetText(editor.SelectionStart, editor.SelectionLength)))
                {
                    //If it is remove the selection so the FindNext() call will simply find the currently highlighted text
                    editor.SelectionStart = 0;
                    editor.SelectionLength = 0;
                }
            }


            bool fromStart = (editor.CaretOffset == 0);
            if (this.FindNext(editor))
            {
                editor.Replace(editor.SelectionStart, editor.SelectionLength, this.ReplaceText);
                editor.SelectionLength = this.ReplaceText.Length;
                editor.CaretOffset = editor.SelectionStart;
            }
            else if (!fromStart)
            {
                if (this.ShouldRestartSearchFromStart())
                {
                    editor.CaretOffset = 0;
                    editor.SelectionStart = 0;
                    editor.SelectionLength = 0;
                    if (this.FindNext(editor))
                    {
                        editor.Replace(editor.SelectionStart, editor.SelectionLength, this.ReplaceText);
                        editor.SelectionLength = this.ReplaceText.Length;
                    }
                }
            }
        }

        /// <summary>
        /// Replace all occurrences of the currently configured search in the given text editor
        /// </summary>
        /// <param name="editor">Text Editor</param>
        public void ReplaceAll(ITextEditorAdaptor<T> editor)
        {
            if (String.IsNullOrEmpty(this.ReplaceText))
            {
                this.ShowMessage("No Replace Text specified");
            }

            int origPos = editor.CaretOffset;

            if (this.Scope != FindAndReplaceScope.Selection)
            {
                //Check whether the relevant Text is already selected
                if (!String.IsNullOrEmpty(this.FindText))
                {
                    if (this.FindText.Equals(editor.GetText(editor.SelectionStart, editor.SelectionLength)))
                    {
                        //If it is remove the selection so the FindNext() call will simply find the currently highlighted text
                        editor.SelectionStart = 0;
                        editor.SelectionLength = 0;
                    }
                }
            }

            //Replace All works over the entire document unless there was already a selection present
            int minPos, maxPos;
            bool restoreSelection = false;
            if (editor.SelectionLength > 0 && this.Scope == FindAndReplaceScope.Selection)
            {
                restoreSelection = true;
                minPos = editor.SelectionStart - 1;
                maxPos = editor.SelectionStart + editor.SelectionLength;
                editor.CaretOffset = Math.Max(minPos, 0);
            }
            else
            {
                minPos = -1;
                maxPos = editor.Text.Length;
                editor.CaretOffset = 0;
            }
            editor.SelectionStart = 0;
            editor.SelectionLength = 0;

            try
            {
                editor.BeginUpdate();
                String replace = this.ReplaceText;
                while (this.FindNext(editor, minPos, maxPos))
                {
                    int diff = replace.Length - editor.SelectionLength;

                    editor.Replace(editor.SelectionStart, editor.SelectionLength, replace);
                    editor.SelectionLength = replace.Length;
                    editor.CaretOffset = editor.SelectionStart;

                    minPos = editor.SelectionStart + editor.SelectionLength + 1;
                    maxPos += diff;
                }
            }
            finally
            {
                editor.EndUpdate();
            }

            editor.CaretOffset = origPos;
            editor.ScrollToLine(editor.GetLineByOffset(origPos));

            if (restoreSelection)
            {
                editor.Select(minPos + 1, maxPos - minPos - 1);
            }
        }
    }
}