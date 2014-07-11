using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}