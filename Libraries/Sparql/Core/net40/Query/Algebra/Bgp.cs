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
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public class Bgp
        : IAlgebra
    {
        public Bgp()
            : this(null) {}

        public Bgp(IEnumerable<Triple> patterns)
        {
            this.TriplePatterns = patterns != null ? patterns.ToList().AsReadOnly() : new List<Triple>().AsReadOnly();
        }

        /// <summary>
        /// Gets the Triple Patterns in the BGP
        /// </summary>
        public IList<Triple> TriplePatterns { get; private set; }

        public bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is Bgp)) return false;

            Bgp bgp = (Bgp) other;

            if (this.TriplePatterns.Count != bgp.TriplePatterns.Count) return false;

            for (int i = 0; i < this.TriplePatterns.Count; i++)
            {
                if (!this.TriplePatterns[i].Equals(bgp.TriplePatterns[i])) return false;
            }
            return true;
        }

        public IEnumerable<string> ProjectedVariables
        {
            get { return this.TriplePatterns.SelectMany(t => t.Nodes).Where(n => n.NodeType == NodeType.Variable).Select(n => n.VariableName).Distinct(); }
        }

        public IEnumerable<string> FixedVariables
        {
            get { return this.ProjectedVariables; }
        }

        public IEnumerable<string> FloatingVariables
        {
            get { return Enumerable.Empty<String>(); }
        }

        public void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }

        public override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        public string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            if (this.TriplePatterns.Count == 0) return "(bgp)";

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("(bgp");
            foreach (Triple t in this.TriplePatterns)
            {
                builder.Append("  (triple ");
                builder.Append(t.Subject.ToString(formatter));
                builder.Append(' ');
                builder.Append(t.Predicate.ToString(formatter));
                builder.Append(' ');
                builder.Append(t.Object.ToString(formatter));
                builder.AppendLine(")");
            }
            builder.Append(")");
            return builder.ToString();
        }

        public IAlgebra Copy()
        {
            return this.Copy(this.TriplePatterns);
        }

        public IAlgebra Copy(IEnumerable<Triple> triplePatterns)
        {
            return new Bgp(triplePatterns);
        }
    }
}