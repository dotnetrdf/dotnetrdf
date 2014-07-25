using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VDS.Common.Collections;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// Basic implementation of random access tabular results
    /// </summary>
    public class RandomAccessTabularResults
        : IRandomAccessTabularResults
    {
        private readonly IList<IResultRow> _rows;
        private readonly MaterializedImmutableView<String> _variables;

        /// <summary>
        /// Creates a new random access tabular result that has zero variables and zero result rows
        /// </summary>
        public RandomAccessTabularResults()
            : this(null, null) { }

        /// <summary>
        /// Creates a new random access tabular result that has the given variables but zero result rows
        /// </summary>
        /// <param name="variables"></param>
        public RandomAccessTabularResults(IEnumerable<String> variables)
            : this(variables, null) { }

        /// <summary>
        /// Creates a new random access tabular result that consists of zero variables and the given number of empty result rows
        /// </summary>
        /// <param name="rows">Number of empty rows</param>
        public RandomAccessTabularResults(int rows)
            : this(null, Enumerable.Repeat(new ResultRow(), rows)) { }

        /// <summary>
        /// Creates a new random access tabular result that consists of the given variables and rows
        /// </summary>
        /// <param name="variables"></param>
        /// <param name="rows"></param>
        public RandomAccessTabularResults(IEnumerable<String> variables, IEnumerable<IResultRow> rows)
        {
            this._variables = variables != null ? new MaterializedImmutableView<string>(variables) : new MaterializedImmutableView<string>();
            this._rows = rows != null ? new List<IResultRow>(rows) : new List<IResultRow>();
        }

        public RandomAccessTabularResults(ITabularResults results)
        {
            this._variables = new MaterializedImmutableView<string>(results.Variables);
            this._rows = new List<IResultRow>(results);
        }

        public IEnumerator<IResultRow> GetEnumerator()
        {
            return this._rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsStreaming
        {
            get { return false; }
        }

        public IEnumerable<string> Variables
        {
            get { return this._variables; }
        }

        public int Count
        {
            get { return this._rows.Count; }
        }

        public IResultRow this[int index]
        {
            get { return this._rows[index]; }
        }

        public void Dispose()
        {
            // No dispose actions needed
        }
    }
}