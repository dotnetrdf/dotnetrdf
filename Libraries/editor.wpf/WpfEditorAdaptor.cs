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
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using AvComplete = ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using VDS.RDF.Utilities.Editor.AutoComplete;
using VDS.RDF.Utilities.Editor.AutoComplete.Data;
using VDS.RDF.Utilities.Editor.Syntax;
using VDS.RDF.Utilities.Editor.Selection;
using VDS.RDF.Utilities.Editor.Wpf.AutoComplete;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    /// <summary>
    /// A Text Editor build with AvalonEdit
    /// </summary>
    public class WpfEditorAdaptor
        : BaseTextEditorAdaptor<TextEditor>
    {
        private Exception _currError;
        private AvComplete.CompletionWindow _c;

        /// <summary>
        /// Creates a new text editor
        /// </summary>
        public WpfEditorAdaptor()
            : base(new TextEditor())
        {
            this.Control.TextChanged += new EventHandler(this.HandleTextChanged);
            this.Control.TextArea.MouseDoubleClick += this.HandleDoubleClick;
            this.Control.TextArea.TextEntered += this.HandleTextEntered;
            this.Control.FontFamily = new FontFamily("Consolas");
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
                throw new ArgumentException("Type Arguments for the Visual Options for a WpfEditorAdaptor are invalid!");
            }
        }

        /// <summary>
        /// Applies visual options to the editor
        /// </summary>
        /// <param name="options">Visual Options</param>
        public void Apply(VisualOptions<FontFamily, Color> options)
        {
            if (options == null) return;

            this.Control.Options.EnableEmailHyperlinks = options.EnableClickableUris;
            this.Control.Options.EnableHyperlinks = options.EnableClickableUris;

            if (options.FontFace != null)
            {
                this.Control.FontFamily = options.FontFace;
            }
            this.Control.FontSize = options.FontSize;
            this.Control.Foreground = new SolidColorBrush(options.Foreground);
            this.Control.Background = new SolidColorBrush(options.Background);

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
            get 
            {
                return this.Control.Document.TextLength; 
            }
        }

        /// <summary>
        /// Gets the current caret position
        /// </summary>
        public override int CaretOffset
        {
            get
            {
                return this.Control.CaretOffset;
            }
            set
            {
                this.Control.CaretOffset = value;
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
                return this.Control.ShowLineNumbers;
            }
            set
            {
                this.Control.ShowLineNumbers = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether to show new line characters
        /// </summary>
        public override bool ShowEndOfLine
        {
            get
            {
                return this.Control.Options.ShowEndOfLine;
            }
            set
            {
                this.Control.Options.ShowEndOfLine = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether to show spaces
        /// </summary>
        public override bool ShowSpaces
        {
            get
            {
                return this.Control.Options.ShowSpaces;
            }
            set
            {
                this.Control.Options.ShowSpaces = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether to show tabs
        /// </summary>
        public override bool ShowTabs
        {
            get
            {
                return this.Control.Options.ShowTabs;
            }
            set
            {
                this.Control.Options.ShowTabs = value;
            }
        }

        #endregion

        #region Visual Manipulation

        public override void ScrollToLine(int line)
        {
            this.Control.ScrollToLine(line);
        }

        public override void Refresh()
        {
            this.Control.TextArea.InvalidateVisual();
        }

        public override void BeginUpdate()
        {
            this.Control.Document.BeginUpdate();
        }

        public override void EndUpdate()
        {
            this.Control.Document.EndUpdate();
        }

        #endregion

        #region Text Manipulation

        public override char GetCharAt(int offset)
        {
            return this.Control.Document.GetCharAt(offset);
        }

        public override int GetLineByOffset(int offset)
        {
            return this.Control.Document.GetLineByOffset(offset).LineNumber;
        }

        public override string GetText(int offset, int length)
        {
            return this.Control.Document.GetText(offset, length);
        }

        public override void Select(int offset, int length)
        {
            this.Control.Select(offset, length);
        }

        public override void Replace(int offset, int length, string text)
        {
            this.Control.Document.Replace(offset, length, text);
        }

        public override void Cut()
        {
            this.Control.Cut();
        }

        public override void Copy()
        {
            this.Control.Copy();
        }

        public override void Paste()
        {
            this.Control.Paste();
        }

        public override void Undo()
        {
            this.Control.Undo();
        }

        public override void Redo()
        {
            this.Control.Redo();
        }

        #endregion

        #region Highlighting

        public Exception ErrorToHighlight
        {
            get
            {
                return this._currError;
            }
        }

        public override void SetHighlighter(string name)
        {
            if (name == null || name.Equals(String.Empty) || name.Equals("None"))
            {
                this.Control.SyntaxHighlighting = null;
            }
            else
            {
                this.Control.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(name);
            }
        }

        public override void AddErrorHighlight(Exception ex)
        {
            this._currError = ex;
        }

        public override void ClearErrorHighlights()
        {
            this._currError = null;
        }

        #endregion

        #region Auto-Completion

        //TODO: Refactor auto-completion appropriately

        public override bool CanAutoComplete
        {
            get
            {
                return true;
            }
        }

        public override void Suggest(IEnumerable<ICompletionData> suggestions)
        {
            bool mustShow = false;
            if (this._c == null)
            {
                this._c = new AvComplete.CompletionWindow(this.Control.TextArea);
                this._c.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
                this._c.StartOffset = this.CaretOffset - 1;
                this._c.CloseAutomatically = true;
                this._c.CloseWhenCaretAtBeginning = true;
                this._c.Closed += (sender, args) =>
                    {
                        this.EndSuggestion();
                        this.AutoCompleter.DetectState();
                    };
                this._c.KeyDown += (sender, args) =>
                    {
                        if (args.Key == Key.Space && args.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        {
                            this._c.CompletionList.RequestInsertion(args);
                            this.Control.Document.Insert(this.CaretOffset, " ");
                            args.Handled = true;
                        }
                        else if (this.AutoCompleter.State == AutoCompleteState.Keyword || this.AutoCompleter.State == AutoCompleteState.KeywordOrQName)
                        {
                            if (args.Key == Key.D9 && args.KeyboardDevice.Modifiers == ModifierKeys.Shift)
                            {
                                this._c.CompletionList.RequestInsertion(args);
                            }
                            else if (args.Key == Key.OemOpenBrackets && args.KeyboardDevice.Modifiers == ModifierKeys.Shift)
                            {
                                this._c.CompletionList.RequestInsertion(args);
                                this.Control.Document.Insert(this.CaretOffset, " ");
                            }
                        }
                        else if (this.AutoCompleter.State == AutoCompleteState.Variable || this.AutoCompleter.State == AutoCompleteState.BNode)
                        {
                            if (args.Key == Key.D0 && args.KeyboardDevice.Modifiers == ModifierKeys.Shift)
                            {
                                this._c.CompletionList.RequestInsertion(args);
                            }
                        }
                    };
                mustShow = true;
            }
            foreach (ICompletionData data in suggestions)
            {
                this._c.CompletionList.CompletionData.Add(new WpfCompletionData(data));
            }
            if (mustShow) this._c.Show();
        }

        public override void EndSuggestion()
        {
            if (this._c != null)
            {
                this._c.Close();
                this._c = null;
            }
            if (this.AutoCompleter != null)
            {
                this.AutoCompleter.State = AutoCompleteState.None;
            }
        }

        #endregion

        #region Event Handling

        private void HandleTextChanged(Object sender, EventArgs args)
        {
            this.RaiseTextChanged(sender);
        }

        private void HandleTextEntered(Object sender, TextCompositionEventArgs args)
        {
            if (this.CanAutoComplete && this.AutoCompleter != null)
            {
                this.AutoCompleter.TryAutoComplete(args.Text);
            }
        }

        private void HandleDoubleClick(Object sender, MouseButtonEventArgs args)
        {
            this.RaiseDoubleClick(sender);
            args.Handled = (this.SymbolSelector != null);
        }

        #endregion
    }
}
