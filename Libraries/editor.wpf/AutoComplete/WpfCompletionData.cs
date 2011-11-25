using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using AvComplete = ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using VDS.RDF.Utilities.Editor.AutoComplete;
using VDS.RDF.Utilities.Editor.AutoComplete.Data;

namespace VDS.RDF.Utilities.Editor.Wpf.AutoComplete
{
    public class WpfCompletionData
        : AvComplete.ICompletionData
    {
        private ICompletionData _data;

        public WpfCompletionData(ICompletionData data)
        {
            this._data = data;
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }

        public object Content
        {
            get 
            {
                return this._data.DisplayText; 
            }
        }

        public object Description
        {
            get 
            {
                return this._data.Description; 
            }
        }

        public ImageSource Image
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
                return this._data.Priority; 
            }
        }

        public string Text
        {
            get 
            {
                return this._data.InsertionText; 
            }
        }
    }
}
