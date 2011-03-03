using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    public class KeywordCompletionData : BaseCompletionData
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

        public override object Description
        {
            get 
            {
                if (this._description.Equals(String.Empty))
                {
                    return "The " + this._keyword + " Keyword";
                }
                else
                {
                    return this._description;
                }
            }
        }

        public override double Priority
        {
            get 
            {
                return this._priority;
            }
            set
            {
                this._priority = value;
            }
        }

        public override string Text
        {
            get 
            {
                return this._keyword; 
            }
        }
    }
}
