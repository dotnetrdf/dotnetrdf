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
