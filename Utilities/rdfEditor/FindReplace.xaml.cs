using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit;

namespace rdfEditor
{
    public enum FindReplaceMode
    {
        Find,
        FindAndReplace
    }

    /// <summary>
    /// Interaction logic for FindReplace.xaml
    /// </summary>
    public partial class FindReplace : Window
    {
        private FindReplaceMode _mode = FindReplaceMode.Find;
        private TextEditor _editor;

        public FindReplace()
        {
            InitializeComponent();
            this.GotFocus += new RoutedEventHandler(FindReplace_GotFocus);
        }

        void FindReplace_GotFocus(object sender, RoutedEventArgs e)
        {
            Window_GotFocus(sender, e);
        }

        public FindReplaceMode Mode
        {
            get
            {
                return this._mode;
            }
            set
            {
                this._mode = value;
                if (this._mode == FindReplaceMode.Find)
                {
                    this.ToggleReplaceVisibility(Visibility.Collapsed);
                }
                else
                {
                    this.ToggleReplaceVisibility(Visibility.Visible);
                }
            }
        }

        public TextEditor Editor
        {
            set
            {
                this._editor = value;
            }
        }

        public bool IsScopeCurrentDocument
        {
            get
            {
                return ((ComboBoxItem)this.cboLookIn.SelectedItem).Tag.Equals("Current Document");
            }
        }

        public bool IsScopeSelection
        {
            get
            {
                return ((ComboBoxItem)this.cboLookIn.SelectedItem).Tag.Equals("Selection");
            }
        }

        private void ToggleReplaceVisibility(Visibility v)
        {
            this.lblReplace.Visibility = v;
            this.cboReplace.Visibility = v;
            this.btnReplace.Visibility = v;
            this.btnReplaceAll.Visibility = v;

            switch (v)
            {
                case Visibility.Collapsed:
                case Visibility.Hidden:
                    this.Title = "Find";
                    break;
                default:
                    this.Title = "Find and Replace";
                    break;
            }
            this.stkDialog.UpdateLayout();
        }

        public void Find(TextEditor editor)
        {
            bool fromStart = (this._editor.CaretOffset == 0);
            if (!this.FindNext(this._editor) && !fromStart)
            {
                if (MessageBox.Show("No further instances of the Find Text were found.  Would you like to restart the search from beginning of document?", "Text Not Found", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    this._editor.CaretOffset = 0;
                    this._editor.SelectionStart = 0;
                    this._editor.SelectionLength = 0;
                    this.FindNext(this._editor);
                }
            }
        }

        public bool FindNext(TextEditor editor)
        {
            if (this.IsScopeCurrentDocument || editor.SelectionLength == 0)
            {
                return this.FindNext(editor, -1, editor.Text.Length);
            }
            else
            {
                return this.FindNext(editor, Math.Max(0, editor.SelectionStart - 1), editor.SelectionStart + editor.SelectionLength);
            }
        }

        public bool FindNext(TextEditor editor, int minPos, int maxPos)
        {
            if (editor == null)
            {
                MessageBox.Show("No Text Editor is associated with this Find and Replace Dialog");
                return false;
            }
            if (this.cboFind.Text == null || this.cboFind.Text.Equals(String.Empty))
            {
                MessageBox.Show("No Find Text specified");
                return false;
            }
            String find = this.cboFind.Text;

            //Validate Regex
            if (this.chkRegex.IsChecked == true)
            {
                try
                {
                    Regex.IsMatch(String.Empty, find);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Regular Expression is malformed - " + ex.Message);
                    return false;
                }
            }

            //Add Search Text to Combo Box for later reuse
            if (!this.cboFind.Items.Contains(find))
            {
                this.cboFind.Items.Add(find);
            }

            int start = editor.CaretOffset;
            int pos;
            int length = find.Length;
            StringComparison compareMode = (this.chkMatchCase.IsChecked == true) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            RegexOptions regexOps = (this.chkMatchCase.IsChecked == true) ? RegexOptions.None : RegexOptions.IgnoreCase;

            if (this.chkSearchUp.IsChecked == true)
            {
                //Search portion of Document prior to current position
                if (this.chkRegex.IsChecked == true)
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
                if (this.chkRegex.IsChecked == true)
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
                if (this.chkMatchWholeWord.IsChecked == true)
                {
                    //Check boundary before
                    if (pos > 0)
                    {
                        char c = editor.Text[pos - 1];
                        if (Char.IsLetterOrDigit(c))
                        {
                            //Not a boundary so adjust start position and recurse
                            editor.CaretOffset = pos + length;
                            if (this.chkSearchUp.IsChecked == false) editor.CaretOffset -= editor.SelectionLength;
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
                            if (this.chkSearchUp.IsChecked == false) editor.CaretOffset -= editor.SelectionLength;
                            return this.FindNext(editor);
                        }
                    }
                }

                editor.Select(pos, length);
                editor.CaretOffset = pos;
                editor.ScrollTo(editor.Document.GetLineByOffset(pos).LineNumber, 0);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Replace(TextEditor editor)
        {
            if (this.cboReplace.Text == null)
            {
                MessageBox.Show("No Replace Text specified");
            }

            //Check whether the relevant Text is already selected
            if (this.cboFind.Text != null && !this.cboFind.Text.Equals(String.Empty))
            {
                if (this.cboFind.Text.Equals(editor.SelectedText))
                {
                    //If it is remove the selection so the FindNext() call will simply find the currently highlighted text
                    editor.SelectionStart = 0;
                    editor.SelectionLength = 0;
                }
            }


            bool fromStart = (editor.CaretOffset == 0);
            if (this.FindNext(editor))
            {
                editor.Document.Replace(editor.SelectionStart, editor.SelectionLength, this.cboReplace.Text);
                editor.SelectionLength = this.cboReplace.Text.Length;
                editor.CaretOffset = editor.SelectionStart;
            }
            else if (!fromStart)
            {
                if (MessageBox.Show("No further instances of the Find Text were found.  Would you like to restart the search from beginning of document?", "Text Not Found", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    editor.CaretOffset = 0;
                    editor.SelectionStart = 0;
                    editor.SelectionLength = 0;
                    if (this.FindNext(editor))
                    {
                        editor.Document.Replace(editor.SelectionStart, editor.SelectionLength, this.cboReplace.Text);
                        editor.SelectionLength = this.cboReplace.Text.Length;
                    }
                }
            }
        }

        private void ReplaceAll(TextEditor editor)
        {
            if (this.cboReplace.Text == null)
            {
                MessageBox.Show("No Replace Text specified");
            }

            int origPos = editor.CaretOffset;

            if (!this.cboLookIn.SelectedValue.ToString().Equals("Selection"))
            {
                //Check whether the relevant Text is already selected
                if (this.cboFind.Text != null && !this.cboFind.Text.Equals(String.Empty))
                {
                    if (this.cboFind.Text.Equals(editor.SelectedText))
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
            if (editor.SelectionLength > 0 && this.IsScopeSelection)
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

            editor.Document.BeginUpdate();
            String replace = this.cboReplace.Text;
            while (this.FindNext(editor, minPos, maxPos))
            {
                int diff = replace.Length - editor.SelectionLength;

                editor.Document.Replace(editor.SelectionStart, editor.SelectionLength, replace);
                editor.SelectionLength = replace.Length;
                editor.CaretOffset = editor.SelectionStart;

                maxPos += diff;
            }
            editor.Document.EndUpdate();

            editor.CaretOffset = origPos;
            editor.ScrollToLine(editor.Document.GetLineByOffset(origPos).LineNumber);

            if (restoreSelection)
            {
                editor.Select(minPos + 1, maxPos - minPos - 1);
            }
        }

        private void btnFindNext_Click(object sender, RoutedEventArgs e)
        {
            this.Find(this._editor);
        }

        private void btnReplace_Click(object sender, RoutedEventArgs e)
        {
            if (this._mode == FindReplaceMode.Find) return;
            this.Replace(this._editor);
        }

        private void btnReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            if (this._mode == FindReplaceMode.Find) return;
            this.ReplaceAll(this._editor);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            e.Cancel = true;
        }

        private void btnReplace_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.btnReplace.Visibility != Visibility.Visible)
            {
                this.cboFind.Focus();
            }
        }

        private void btnReplaceAll_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.btnReplaceAll.Visibility != Visibility.Visible)
            {
                this.cboFind.Focus();
            }
        }

        private void cboReplace_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.cboReplace.Visibility != Visibility.Visible)
            {
                this.chkMatchCase.Focus();
            }
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is EditorWindow)
            {
                this.cboFind.Focus();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.cboFind.Focus();
        }
    }
}
