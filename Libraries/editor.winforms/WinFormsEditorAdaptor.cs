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
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.GUI.WinForms;

namespace VDS.RDF.Utilities.Editor.WinForms
{
    /// <summary>
    /// A Text Editor build with a standard <see cref="RichTextBox"/>
    /// </summary>
    public class WinFormsEditorAdaptor
        : BaseTextEditorAdaptor<DnrRichTextBox>
    {
        /// <summary>
        /// Creates a new text editor
        /// </summary>
        public WinFormsEditorAdaptor()
            : base(new DnrRichTextBox())
        {
            this.Control.TextChanged += this.HandleTextChanged;
            this.Control.DoubleClick += this.HandleDoubleClick;
            //this.Control.TextArea.TextEntered += this.HandleTextEntered;
            this.Control.Font = new Font("Consolas", 12f);
        }

        /// <summary>
        /// Applies visual options to the editor
        /// </summary>
        /// <typeparam name="TFont">Font Type</typeparam>
        /// <typeparam name="TColor">Colour Type</typeparam>
        /// <param name="options">Visual Options</param>
        public override void Apply<TFont, TColor>(VisualOptions<TFont, TColor> options)
        {
            try
            {
                this.Apply(options as VisualOptions<FontFamily, Color>);
            }
            catch
            {
                throw new ArgumentException("Type Arguments for the Visual Options for a WinFormsEditorAdaptor are invalid!");
            }
        }

        /// <summary>
        /// Applies visual options to the editor
        /// </summary>
        /// <param name="options">Visual Options</param>
        public void Apply(VisualOptions<FontFamily, Color> options)
        {
            if (options == null) return;

            //this.Control.Options.EnableEmailHyperlinks = options.EnableClickableUris;
            //this.Control.Options.EnableHyperlinks = options.EnableClickableUris;

            //if (options.FontFace != null)
            //{
            //    this.Control.FontFamily = options.FontFace;
            //}
            //this.Control.FontSize = options.FontSize;
            //this.Control.Foreground = new SolidColorBrush(options.Foreground);
            //this.Control.Background = new SolidColorBrush(options.Background);

            this.ShowLineNumbers = options.ShowLineNumbers;
            this.ShowSpaces = options.ShowSpaces;
            this.ShowTabs = options.ShowTabs;
            this.ShowEndOfLine = options.ShowEndOfLine;
            this.WordWrap = options.WordWrap;
        }

        #region State

        /// <summary>
        /// Gets/Sets the text
        /// </summary>
        public override string Text
        {
            get
            {
                return this.Control.Text;
            }
            set
            {
                this.Control.Text = value;
            }
        }

        /// <summary>
        /// Gets the text length
        /// </summary>
        public override int TextLength
        {
            get { return this.Control.TextLength; }
        }

        /// <summary>
        /// Gets the current caret position
        /// </summary>
        public override int CaretOffset
        {
            get { return this.Control.SelectionStart; }
            set
            {
                this.Control.SelectionStart = value;
                this.Control.SelectionLength = 0;
            }
        }

        /// <summary>
        /// Gets/Sets the current selection start (if any)
        /// </summary>
        public override int SelectionStart
        {
            get
            {
                return this.Control.SelectionStart;
            }
            set
            {
                this.Control.SelectionStart = value;
            }
        }

        /// <summary>
        /// Gets/Sets the current selection length (if any)
        /// </summary>
        public override int SelectionLength
        {
            get
            {
                return this.Control.SelectionLength;
            }
            set
            {
                this.Control.SelectionLength = value;
            }
        }

        /// <summary>
        /// Gets/Sets word wrapping
        /// </summary>
        public override bool WordWrap
        {
            get
            {
                return this.Control.WordWrap;
            }
            set
            {
                this.Control.WordWrap = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether to show line numbers
        /// </summary>
        public override bool ShowLineNumbers
        {
            get
            {
                //return this.Control.ShowLineNumbers;
                return false;
            }
            set
            {
                //this.Control.ShowLineNumbers = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether to show new line characters
        /// </summary>
        public override bool ShowEndOfLine
        {
            get
            {
                //return this.Control.Options.ShowEndOfLine;
                return false;
            }
            set
            {
                //this.Control.Options.ShowEndOfLine = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether to show spaces
        /// </summary>
        public override bool ShowSpaces
        {
            get
            {
                //return this.Control.Options.ShowSpaces;
                return false;
            }
            set
            {
                //this.Control.Options.ShowSpaces = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether to show tabs
        /// </summary>
        public override bool ShowTabs
        {
            get
            {
                //return this.Control.Options.ShowTabs;
                return false;
            }
            set
            {
                //this.Control.Options.ShowTabs = value;
            }
        }

        #endregion

        #region Visual Manipulation

        /// <summary>
        /// Scroll to a specific line
        /// </summary>
        /// <param name="line">Line</param>
        public override void ScrollToLine(int line)
        {
            this.Control.SelectionStart = this.Control.GetFirstCharIndexFromLine(line);
            this.Control.ScrollToCaret();
        }

        /// <summary>
        /// Refresh the editor
        /// </summary>
        public override void Refresh()
        {
            this.Control.Refresh();
        }

        /// <summary>
        /// Begins an update on the editor
        /// </summary>
        public override void BeginUpdate()
        {
            this.Control.BeginUpdate();
        }

        /// <summary>
        /// Ends an update on the editor
        /// </summary>
        public override void EndUpdate()
        {
            this.Control.EndUpdate();
        }

        #endregion

        #region Text Manipulation

        /// <summary>
        /// Gets the character at a given offset
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Character</returns>
        public override char GetCharAt(int offset)
        {
            return this.Control.Text[offset];
        }

        /// <summary>
        /// Gets the line number for an offset
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Line Number</returns>
        public override int GetLineByOffset(int offset)
        {
            return this.Control.GetLineFromCharIndex(offset);
        }

        /// <summary>
        /// Gets some text
        /// </summary>
        /// <param name="offset">Offset to start at</param>
        /// <param name="length">Length of the text to retrive</param>
        /// <returns>Text</returns>
        public override string GetText(int offset, int length)
        {
            return this.Control.Text.Substring(offset, length);
        }

        /// <summary>
        /// Selects some text
        /// </summary>
        /// <param name="offset">Offset to start selection at</param>
        /// <param name="length">Length of the text to select</param>
        public override void Select(int offset, int length)
        {
            this.Control.Select(offset, length);
        }

        /// <summary>
        /// Replaces some text
        /// </summary>
        /// <param name="offset">Offset to start replacement at</param>
        /// <param name="length">Length of the text to replace</param>
        /// <param name="text">Text to replace with</param>
        public override void Replace(int offset, int length, string text)
        {
            String currText = this.Control.Text;
            StringBuilder newText = new StringBuilder();
            if (offset > 0) newText.Append(currText.Substring(0, offset));
            newText.Append(text);
            if (length < currText.Length) newText.Append(currText.Substring(offset + length));
            this.Control.Text = newText.ToString();
        }

        /// <summary>
        /// Cut the current selection
        /// </summary>
        public override void Cut()
        {
            this.Control.Cut();
        }

        /// <summary>
        /// Copy the current selection
        /// </summary>
        public override void Copy()
        {
            this.Control.Copy();
        }

        /// <summary>
        /// Paste the current clipboard contents
        /// </summary>
        public override void Paste()
        {
            this.Control.Paste();
        }

        /// <summary>
        /// Undo the last operation
        /// </summary>
        public override void Undo()
        {
            this.Control.Undo();
        }

        /// <summary>
        /// Redo the last undone operation
        /// </summary>
        public override void Redo()
        {
            this.Control.Redo();
        }

        #endregion

        #region Highlighting

        /// <summary>
        /// Gets the error to higlight
        /// </summary>
        public Exception ErrorToHighlight { get; private set; }

        /// <summary>
        /// Sets the syntax highlighter to use
        /// </summary>
        /// <param name="name">Syntax Name</param>
        public override void SetHighlighter(string name)
        {
            //if (name == null || name.Equals(String.Empty) || name.Equals("None"))
            //{
            //    this.Control.SyntaxHighlighting = null;
            //}
            //else
            //{
            //    this.Control.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(name);
            //}
        }

        /// <summary>
        /// Adds error highlighting for the given error
        /// </summary>
        /// <param name="ex"></param>
        public override void AddErrorHighlight(Exception ex)
        {
            this.ErrorToHighlight = ex;
        }

        /// <summary>
        /// Clears error highlights
        /// </summary>
        public override void ClearErrorHighlights()
        {
            this.ErrorToHighlight = null;
        }

        #endregion

        #region Auto-Completion

        /// <summary>
        /// Gets that auto-completion is supported
        /// </summary>
        public override bool CanAutoComplete
        {
            get { return false; }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// Handles the text changed event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Event Arguments</param>
        private void HandleTextChanged(Object sender, EventArgs args)
        {
            this.RaiseTextChanged(sender);
        }

        /// <summary>
        /// Handles the double click event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="eventArgs">Event Arguments</param>
        private void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            this.RaiseDoubleClick(sender);
        }

        #endregion
    }
}
