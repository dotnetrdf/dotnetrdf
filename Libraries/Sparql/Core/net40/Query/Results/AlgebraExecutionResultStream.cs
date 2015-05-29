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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Engine;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// A result stream that allows presenting algebra execution results to the results API
    /// </summary>
    public class AlgebraExecutionResultStream
        : IResultStream
    {
        private IResultRow _current;

        public AlgebraExecutionResultStream(IAlgebra algebra, IEnumerable<ISolution> solutions)
        {
            if (algebra == null) throw new ArgumentNullException("algebra");
            if (solutions == null) throw new ArgumentNullException("solutions");
            this.Variables = algebra.ProjectedVariables.ToList().AsReadOnly();
            this.Solutions = solutions.GetEnumerator();
        }

        private bool Started { get; set; }

        private bool Finished { get; set; }

        private IEnumerator<ISolution> Solutions { get; set; }

        public void Dispose()
        {
            this.Solutions.Dispose();
        }

        public bool MoveNext()
        {
            if (!this.Started) this.Started = true;
            if (this.Finished) return false;

            if (!this.Solutions.MoveNext())
            {
                this.Finished = true;
                return false;
            }

            // Convert solution to row
            ISolution solution = this.Solutions.Current;
            IDictionary<String, INode> values = new Dictionary<string, INode>();
            foreach (String var in solution.Variables)
            {
                values.Add(var, solution[var]);
            }
            this.Current = new ResultRow(this.Variables, values);
            return true;
        }

        public void Reset()
        {
            throw new InvalidOperationException("Streamed results cannot be reset");
        }

        public IResultRow Current
        {
            get
            {
                if (!this.Started) throw new InvalidOperationException("Before the start of the enumerator, MoveNext() must be called at least once before this property is accessed");
                if (this.Finished) throw new InvalidOperationException("Past the end of the enumerator");
                return this._current;
            }
            set { this._current = value; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public IEnumerable<string> Variables { get; private set; }
    }
}
