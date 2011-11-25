using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    public abstract class BaseCompletionData : ICompletionData
    {
        private const double DefaultPriority = 1.0d;

        private String _display, _insert, _descrip;
        private double _priority = DefaultPriority;

        public BaseCompletionData(String displayText, String insertText)
            : this(displayText, insertText, String.Empty, DefaultPriority) { }

        public BaseCompletionData(String displayText, String insertText, String description)
            : this(displayText, insertText, description, DefaultPriority) { }

        public BaseCompletionData(String displayText, String insertText, String description, double priority)
        {
            this._display = displayText;
            this._insert = insertText;
            this._descrip = description;
            this._priority = priority;
        }

        public String Description
        {
            get
            {
                return this._descrip;
            }
        }

        public double Priority
        {
            get
            {
                return this._priority;
            }
        }

        public String DisplayText
        {
            get
            {
                return this._display;
            }
        }

        public String InsertionText
        {
            get
            {
                return this._insert;
            }
        }

        public int CompareTo(ICompletionData other)
        {
            int c = this.Priority.CompareTo(other.Priority);
            if (c == 0)
            {
                return this.InsertionText.CompareTo(other.InsertionText);
            }
            else
            {
                return c;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            if (obj is ICompletionData)
            {
                return this.CompareTo((ICompletionData)obj);
            }
            else
            {
                return -1;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is ICompletionData)
            {
                return this.Equals((ICompletionData)obj);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(ICompletionData other)
        {
            return this.GetHashCode().Equals(other.GetHashCode());
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override string ToString()
        {
            return this.GetType().Name + ": " + this.InsertionText;
        }
    }
}
