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
using VDS.RDF.Query.Expressions;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public class Extend
        : BaseUnaryAlgebra
    {
        private Extend(IAlgebra innerAlgebra, IEnumerable<KeyValuePair<String, IExpression>> assignments)
            : base(innerAlgebra)
        {
            this.Assignments = assignments.ToList().AsReadOnly();
            if (this.Assignments.Count == 0) throw new ArgumentException("Number of assignments must be >= 1", "assignments");
        }

        public static Extend Create(IAlgebra innerAlgebra, IEnumerable<KeyValuePair<String, IExpression>> assignments)
        {
            if (!(innerAlgebra is Extend)) return Wrap(innerAlgebra, assignments);

            Extend e = (Extend) innerAlgebra;
            return new Extend(e.InnerAlgebra, e.Assignments.Concat(assignments));
        }

        public static Extend Wrap(IAlgebra innerAlgebra, IEnumerable<KeyValuePair<String, IExpression>> assignments)
        {
            return new Extend(innerAlgebra, assignments);
        }

        public IList<KeyValuePair<String, IExpression>> Assignments { get; private set; }

        public override IAlgebra Copy(IAlgebra innerAlgebra)
        {
            return Create(innerAlgebra, this.Assignments);
        }

        public override void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            StringBuilder builder = new StringBuilder();
            builder.Append("(extend (");
            for (int i = 0; i < this.Assignments.Count; i++)
            {
                if (i > 0) builder.Append(' ');
                builder.Append('(');
                builder.Append(formatter.Format(new VariableNode(this.Assignments[i].Key)));
                builder.Append(' ');
                builder.Append(this.Assignments[i].Value.ToPrefixString(formatter));
                builder.Append(")");
            }
            builder.AppendLine(")");
            builder.AppendLineIndented(this.InnerAlgebra.ToString(formatter), 2);
            builder.AppendLine(")");
            return builder.ToString();
        }

        public override bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is Extend)) return false;

            Extend e = (Extend) other;
            if (this.Assignments.Count != e.Assignments.Count) return false;
            for (int i = 0; i < this.Assignments.Count; i++)
            {
                if (!this.Assignments[i].Equals(e.Assignments[i])) return false;
            }
            return this.InnerAlgebra.Equals(e.InnerAlgebra);
        }
    }
}
