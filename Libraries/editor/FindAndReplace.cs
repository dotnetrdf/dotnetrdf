using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VDS.RDF.Utilities.Editor
{
    public enum FindReplaceMode
    {
        Find,
        FindAndReplace
    }

    public enum FindAndReplaceScope
    {
        CurrentDocument,
        Selection
    }

    public abstract class FindAndReplace<T>
    {
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

        protected abstract bool ShouldRestartSearchFromStart();

        protected abstract void ShowMessage(String message);

        public FindAndReplaceScope Scope
        {
            get;
            set;
        }

        public String FindText
        {
            get;
            set;
        }

        public String ReplaceText
        {
            get;
            set;
        }

        public bool MatchCase
        {
            get;
            set;
        }

        public bool MatchWholeWord
        {
            get;
            set;
        }

        public bool UseRegex
        {
            get;
            set;
        }

        public bool SearchUp
        {
            get;
            set;
        }

        public BindingList<String> RecentFindTexts
        {
            get;
            set;
        }

        public BindingList<String> RecentReplaceTexts
        {
            get;
            set;
        }

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

        public bool FindNext(ITextEditorAdaptor<T> editor, int minPos, int maxPos)
        {
            if (String.IsNullOrEmpty(this.FindText))
            {
                this.ShowMessage("No Find Text specified");
                return false;
            }
            String find = this.FindText;

            //Validate Regex
            if (this.UseRegex)
            {
                try
                {
                    Regex.IsMatch(String.Empty, find);
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
            RegexOptions regexOps = this.MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase;

            if (this.SearchUp)
            {
                //Search portion of Document prior to current position
                if (this.UseRegex)
                {
                    MatchCollection ms = Regex.Matches(editor.Text.Substring(0, start), find, regexOps);
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
                    Match m = Regex.Match(editor.Text.Substring(start), find, regexOps);
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
            if (pos > -1)
            {
                //Check we meet any document range restrictions
                if (pos < minPos || pos > maxPos)
                {
                    editor.CaretOffset = pos;
                    editor.SelectionStart = pos;
                    editor.SelectionLength = length;
                    return this.FindNext(editor, minPos, maxPos);
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
            else
            {
                return false;
            }
        }

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
