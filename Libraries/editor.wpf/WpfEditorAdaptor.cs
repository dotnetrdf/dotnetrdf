using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using VDS.RDF.Utilities.Editor.Syntax;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    class WpfEditorAdaptor
        : BaseTextEditorAdaptor<TextEditor>
    {
        private Exception _currError;

        public WpfEditorAdaptor()
            : base(new TextEditor())
        {
            this.Control.TextChanged += new EventHandler(this.HandleTextChanged);
            this.Control.FontFamily = new FontFamily("Consolas");
        }

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

        public override int TextLength
        {
            get 
            {
                return this.Control.Document.TextLength; 
            }
        }

        public override int CaretOffset
        {
            get
            {
                return this.Control.CaretOffset;
            }
        }

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

        #endregion

        #region Event Handling

        private void HandleTextChanged(Object sender, EventArgs args)
        {
            this.RaiseTextChanged(sender);
        }

        #endregion
    }
}
