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
    public class Project
        : BaseUnaryAlgebra
    {

        public Project(IAlgebra innerAlgebra, IEnumerable<String> vars)
            : base(innerAlgebra)
        {
            this.Projections = vars.ToList().AsReadOnly();
            if (this.Projections.Count == 0) throw new ArgumentException("Number of variables to project must be >= 1", "vars");
        }

        public IList<String> Projections { get; private set; }

        public override IAlgebra Copy(IAlgebra innerAlgebra)
        {
            return new Project(innerAlgebra, this.Projections);
        }

        public override IEnumerable<string> ProjectedVariables { get { return this.Projections; } }

        public override IEnumerable<string> FixedVariables
        {
            get { return this.InnerAlgebra.FixedVariables.Where(v => this.Projections.Contains(v)); }
        }

        public override IEnumerable<string> FloatingVariables
        {
            get { return this.Projections.Except(this.FixedVariables); }
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
            builder.Append("(project (");
            for(int i = 0; i < this.Projections.Count; i++)
            {
                if (i > 0) builder.Append(' ');
                builder.Append(formatter.Format(new VariableNode(this.Projections[i])));
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
            if (!(other is Project)) return false;

            Project p = (Project) other;
            if (this.Projections.Count != p.Projections.Count) return false;
            for (int i = 0; i < this.Projections.Count; i++)
            {
                if (!this.Projections[i].Equals(p.Projections[i], StringComparison.Ordinal)) return false;
            }
            return this.InnerAlgebra.Equals(p.InnerAlgebra);
        }
    }
}
