using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace rdfEditor.AutoComplete
{
    public class KeywordCompletionData : ICompletionData
    {
        private String _keyword, _description = String.Empty;
        private double _priority = 0.0d;

        public KeywordCompletionData(String keyword)
        {
            this._keyword = keyword;
        }

        public KeywordCompletionData(String keyword, String description)
            : this(keyword)
        {
            this._description = description;
        }

        public KeywordCompletionData(String keyword, double priority)
            : this(keyword)
        {
            this._priority = priority;
        }

        public KeywordCompletionData(String keyword, String description, double priority)
            : this(keyword, description)
        {
            this._priority = priority;
        }

        #region ICompletionData Members

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }

        public object Content
        {
            get 
            {
                return this.Text;
            }
        }

        public object Description
        {
            get 
            {
                return this._description; 
            }
        }

        public System.Windows.Media.ImageSource Image
        {
            get 
            {
                return null; 
            }
        }

        public double Priority
        {
            get 
            {
                return this._priority;
            }
        }

        public string Text
        {
            get 
            {
                return this._keyword; 
            }
        }

        #endregion
    }
}
