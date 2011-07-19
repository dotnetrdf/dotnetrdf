using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    public interface ITextEditorAdaptor<T>
    {
        T Control
        {
            get;
        }

        String Text
        {
            get;
            set;
        }

        int TextLength
        {
            get;
        }

        int CaretOffset
        {
            get;
        }

        int SelectionStart
        {
            get;
            set;
        }

        int SelectionLength
        {
            get;
            set;
        }

        bool WordWrap
        {
            get;
            set;
        }

        bool ShowLineNumbers
        {
            get;
            set;
        }

        bool ShowEndOfLine
        {
            get;
            set;
        }

        bool ShowSpaces
        {
            get;
            set;
        }

        bool ShowTabs
        {
            get;
            set;
        }

        char GetCharAt(int offset);

        String GetText(int offset, int length);

        void Select(int offset, int length);

        int GetLineByOffset(int offset);

        void ScrollToLine(int line);

        void Cut();

        void Copy();

        void Paste();

        void Undo();

        void Redo();

        void SetHighlighter(String name);

        void ClearErrorHighlights();

        void AddErrorHighlight(Exception ex);

        event TextEditorChangedHandler<T> TextChanged;
    }
}
