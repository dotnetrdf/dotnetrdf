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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public class Table
        : IAlgebra
    {
        public Table(IEnumerable<ISolution> sets)
        {
            if (sets == null) throw new ArgumentNullException("sets");
            this.Data = sets.ToList().AsReadOnly();
            this.IsEmpty = this.Data.Count == 0;
            this.IsUnit = this.Data.Count == 1 && this.Data[0].IsEmpty;
        }

        public static Table CreateUnit()
        {
            return new Table(new Solution().AsEnumerable());
        }

        public static Table CreateEmpty()
        {
            return new Table(Enumerable.Empty<ISolution>());
        }

        public IList<ISolution> Data { get; private set; }

        public bool IsEmpty { get; private set; }

        public bool IsUnit { get; private set; }

        public bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is Table)) return false;

            Table t = (Table) other;
            if (this.IsEmpty) return t.IsEmpty;
            if (this.IsUnit) return t.IsUnit;
            if (t.IsEmpty || t.IsUnit) return false;

            for (int i = 0; i < this.Data.Count; i++)
            {
                if (!this.Data[i].Equals(t.Data[i])) return false;
            }
            return true;
        }

        public IEnumerable<string> ProjectedVariables
        {
            get { return this.Data.SelectMany(s => s.Variables).Distinct(); }
        }

        public IEnumerable<string> FixedVariables
        {
            get { return this.ProjectedVariables.Where(v => this.Data.All(s => s[v] != null)); }
        }

        public IEnumerable<string> FloatingVariables
        {
            get { return this.ProjectedVariables.Where(v => this.Data.Any(s => s[v] == null)); }
        }

        public void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }

        public override String ToString()
        {
            return this.ToString(new AlgebraFormatter());
        }

        public string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            if (this.IsUnit) return "(table unit)";
            if (this.IsEmpty) return "(table empty)";

            StringBuilder builder = new StringBuilder();
            builder.Append("(table (vars");
            foreach (String var in this.ProjectedVariables)
            {
                builder.Append(' ');
                builder.Append(formatter.Format(new VariableNode(var)));
            }
            builder.AppendLine(")");
            foreach (ISolution solution in this.Data)
            {
                builder.Append("  (row");
                foreach (String var in solution.Variables)
                {
                    INode n = solution[var];
                    if (n == null) continue;
                    builder.Append(" [");
                    builder.Append(formatter.Format(new VariableNode(var)));
                    builder.Append(' ');
                    builder.Append(formatter.Format(n));
                    builder.Append("]");
                }
                builder.AppendLine(")");
            }
            builder.AppendLine(")");
            return builder.ToString();
        }

        public IAlgebra Copy()
        {
            return this.Copy(this.Data);
        }

        public IAlgebra Copy(IEnumerable<ISolution> data)
        {
            return new Table(data);
        }
    }
}