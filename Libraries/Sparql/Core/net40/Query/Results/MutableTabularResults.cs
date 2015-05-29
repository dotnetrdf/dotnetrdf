/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Common.Tries;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Results
{
    public class MutableTabularResults
        : IMutableTabularResults
    {
        private readonly IList<IMutableResultRow> _rows;
        private readonly IList<String> _variables;

        public MutableTabularResults(IEnumerable<String> variables, IEnumerable<IMutableResultRow> rows)
        {
            this._variables = variables != null ? new List<string>(variables) : new List<string>();
            this._rows = rows != null ? new List<IMutableResultRow>(rows) : new List<IMutableResultRow>();
        }

        IEnumerator<IResultRow> IEnumerable<IResultRow>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IMutableResultRow> GetEnumerator()
        {
            return this._rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IMutableResultRow item)
        {
            if (item == null) throw new ArgumentNullException("item", "Null rows are not permitted");
            this._rows.Add(item);
        }

        public void Clear()
        {
            this._rows.Clear();
        }

        public bool Contains(IMutableResultRow item)
        {
            return this._rows.Contains(item);
        }

        public void CopyTo(IMutableResultRow[] array, int arrayIndex)
        {
            this._rows.CopyTo(array, arrayIndex);
        }

        public bool Remove(IMutableResultRow item)
        {
            return this._rows.Remove(item);
        }

        public int Count
        {
            get { return this._rows.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(IMutableResultRow item)
        {
            return this._rows.IndexOf(item);
        }

        public void Insert(int index, IMutableResultRow item)
        {
            if (item == null) throw new ArgumentNullException("item", "Null rows are not permitted");
            this._rows.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this._rows.RemoveAt(index);
        }

        public IMutableResultRow this[int index]
        {
            get { return this._rows[index]; }
            set
            {
                if (value == null) throw new ArgumentNullException("value", "Null rows are not permitted");
                this._rows[index] = value;
            }
        }

        public void Dispose()
        {
            // No dispose actions
        }

        public bool IsStreaming
        {
            get { return false; }
        }

        public IEnumerable<string> Variables
        {
            get { return this._variables; }
        }

        public void AddVariable(string var)
        {
            if (this._variables.Contains(var)) throw new RdfQueryException("Cannot add a variable that already exists");
            this._variables.Add(var);
        }

        public void AddVariable(string var, INode initialValue)
        {
            this.AddVariable(var);
            foreach (IMutableResultRow row in this._rows)
            {
                row.Set(var, initialValue);
            }
        }

        public bool Equals(IMutableTabularResults other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return true;
            if (this.Count != other.Count) return false;

            for (int i = 0; i < this.Count; i++)
            {
                if (!this[i].Equals(other[i])) return false;
            }
            return true;
        }
    }
}