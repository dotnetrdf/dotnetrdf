using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using VDS.RDF.Parsing;

namespace VDS.RDF.Utilities.Editor.WinForms
{
    public class WinFormsEditorAdaptor : BaseTextEditorAdaptor<TextEditorControl>
    {
        private List<TextMarker> _markers = new List<TextMarker>();

        public WinFormsEditorAdaptor()
            : base(new TextEditorControl())
        {
            this.Control.Dock = DockStyle.Fill;
            this.Control.TextChanged += new EventHandler(this.HandleTextChanged);
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
                return this.Control.ActiveTextAreaControl.Caret.Offset;
            }
        }

        public override int SelectionStart
        {
            get
            {
                if (this.Control.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
                {
                    return this.Control.ActiveTextAreaControl.SelectionManager.SelectionCollection.First().Offset;
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                if (this.Control.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
                {
                    this.Control.ActiveTextAreaControl.SelectionManager.ClearSelection();
                }
                this.Control.ActiveTextAreaControl.SelectionManager.SetSelection(this.Control.Document.OffsetToPosition(value), this.Control.Document.OffsetToPosition(value + 1));
            }
        }

        public override int SelectionLength
        {
            get
            {
                if (this.Control.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
                {
                    return this.Control.ActiveTextAreaControl.SelectionManager.SelectionCollection.First().Length;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (this.Control.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
                {
                    TextLocation start = this.Control.ActiveTextAreaControl.SelectionManager.SelectionCollection.First().StartPosition;
                    int startOffset = this.Control.ActiveTextAreaControl.SelectionManager.SelectionCollection.First().Length;
                    this.Control.ActiveTextAreaControl.SelectionManager.SelectionCollection.Clear();
                    this.Control.ActiveTextAreaControl.SelectionManager.SetSelection(start, this.Control.Document.OffsetToPosition(startOffset + value));
                }
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
                return this.Control.ShowEOLMarkers;
            }
            set
            {
                this.Control.ShowEOLMarkers = value;
            }
        }

        public override bool ShowSpaces
        {
            get
            {
                return this.Control.ShowSpaces;
            }
            set
            {
                this.Control.ShowSpaces = value;
            }
        }

        public override bool ShowTabs
        {
            get
            {
                return this.Control.ShowTabs;
            }
            set
            {
                this.Control.ShowTabs = value;
            }
        }

        public override bool WordWrap
        {
            get
            {
                return false;
            }
            set
            {
                //Do Nothing
            }
        }

        #endregion

        #region Visual Manipulation

        public override void ScrollToLine(int line)
        {
            this.Control.ActiveTextAreaControl.ScrollTo(line);
        }

        #endregion

        #region Text Manipulation

        public override char GetCharAt(int offset)
        {
            return this.Control.Document.GetCharAt(offset);
        }

        public override int GetLineByOffset(int offset)
        {
            return this.Control.Document.GetLineSegmentForOffset(offset).LineNumber;
        }

        public override string GetText(int offset, int length)
        {
            return this.Control.Document.GetText(offset, length);
        }

        public override void Select(int offset, int length)
        {
            this.Control.ActiveTextAreaControl.SelectionManager.ClearSelection();
            this.Control.ActiveTextAreaControl.SelectionManager.SetSelection(this.Control.Document.OffsetToPosition(offset), this.Control.Document.OffsetToPosition(offset + length));
        }

        public override void Cut()
        {
            if (this.Control.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
            {
                Clipboard.SetText(this.Control.ActiveTextAreaControl.SelectionManager.SelectedText, TextDataFormat.UnicodeText);
                this.Control.ActiveTextAreaControl.SelectionManager.RemoveSelectedText();
            }
        }

        public override void Copy()
        {
            if (this.Control.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
            {
                Clipboard.SetText(this.Control.ActiveTextAreaControl.SelectionManager.SelectedText, TextDataFormat.UnicodeText);
            }
        }

        public override void Paste()
        {
            String text = Clipboard.GetText(TextDataFormat.UnicodeText);
            if (text != null)
            {
                this.Control.Document.Insert(this.Control.ActiveTextAreaControl.Caret.Offset, text);
            }
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
            this.Control.SetHighlighting(name);
        }

        public override void ClearErrorHighlights()
        {
            if (this._markers.Count > 0)
            {
                this._markers.ForEach(m => this.Control.Document.MarkerStrategy.RemoveMarker(m));
                this._markers.Clear();
            }
        }

        public override void AddErrorHighlight(Exception ex)
        {
            RdfParseException parseEx = ex as RdfParseException;
            if (parseEx != null)
            {
                if (parseEx.HasPositionInformation)
                {
                    if (parseEx.StartLine != parseEx.EndLine)
                    {
                        //Get Start and End Lines and use these to calculate the offsets
                        LineSegment startLine = this.Control.Document.GetLineSegment(parseEx.StartLine - 1);
                        LineSegment endLine = this.Control.Document.GetLineSegment(parseEx.EndLine - 1);
                        int startOffset = startLine.Offset + parseEx.StartPosition - 1;
                        int endOffset = endLine.Offset + parseEx.EndPosition - 1;
                        TextMarker m = new TextMarker(startOffset, endOffset - startOffset, TextMarkerType.WaveLine);
                        this._markers.Add(m);
                        this.Control.Document.MarkerStrategy.AddMarker(m);
                    }
                    else
                    {
                        //Get Start Line and use to calculate offset
                        LineSegment startLine = this.Control.Document.GetLineSegment(parseEx.StartLine - 1);
                        int startOffset = startLine.Offset + parseEx.StartPosition - 1;
                        TextMarker m = new TextMarker(startOffset, parseEx.EndPosition - parseEx.StartPosition, TextMarkerType.WaveLine);
                        this._markers.Add(m);
                        this.Control.Document.MarkerStrategy.AddMarker(m);
                    }
                }
            }
        }

        #endregion

        #region Event Handling

        private void HandleTextChanged(Object sender, EventArgs args)
        {
            this.RaiseTextChanged(sender);
        }

        #endregion
    }
}
