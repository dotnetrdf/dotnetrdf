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
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Query.Paths;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public class PropertyPath
        : BaseUnaryAlgebra
    {
        public PropertyPath(IAlgebra innerAlgebra, TriplePath triplePath)
            : base(innerAlgebra)
        {
            if (triplePath == null) throw new ArgumentNullException("triplePath");
            this.TriplePath = triplePath;
        }

        public TriplePath TriplePath { get; private set; }

        public override IAlgebra Copy(IAlgebra innerAlgebra)
        {
            return new PropertyPath(innerAlgebra, this.TriplePath);
        }

        public override IEnumerable<string> ProjectedVariables
        {
            get { return this.InnerAlgebra.ProjectedVariables.Concat(this.TriplePath.Variables).Distinct(); }
        }

        public override IEnumerable<string> FixedVariables
        {
            get { return this.InnerAlgebra.FixedVariables.Concat(this.TriplePath.Variables).Distinct(); }
        }

        public override IEnumerable<string> FloatingVariables
        {
            get { return this.InnerAlgebra.FloatingVariables.Except(this.FixedVariables); }
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
            builder.Append("(path ");
            builder.Append(formatter.Format(this.TriplePath.Subject));
            builder.Append(" (");
            builder.Append(this.TriplePath.Path.ToString(formatter));
            builder.Append(") ");
            builder.AppendLine(formatter.Format(this.TriplePath.Object));
            builder.AppendLineIndented(this.InnerAlgebra.ToString(formatter), 2);
            builder.AppendLine(")");
            return builder.ToString();
        }

        public override bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is PropertyPath)) return false;

            PropertyPath pp = (PropertyPath) other;

            return this.TriplePath.Equals(pp.TriplePath);
        }
    }
}
