/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
