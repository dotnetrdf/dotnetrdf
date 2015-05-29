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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public class Join
        : BaseBinaryAlgebra
    {
        private Join(IAlgebra lhs, IAlgebra rhs) 
            : base(lhs, rhs) { }

        /// <summary>
        /// Creates a join only if necessary i.e. if one side is a join identity returns only the other side
        /// </summary>
        /// <param name="lhs">LHS algebra</param>
        /// <param name="rhs">RHS algebra</param>
        /// <returns>Algebra</returns>
        public static IAlgebra Create(IAlgebra lhs, IAlgebra rhs)
        {
            if (IsTableUnit(lhs)) return rhs;
            return IsTableUnit(rhs) ? lhs : new Join(lhs, rhs);
        }

        /// <summary>
        /// Creates a join, unlike <see cref="Create"/> does not simplify when one side is the join identity
        /// </summary>
        /// <param name="lhs">LHS algebra</param>
        /// <param name="rhs">RHS algebra</param>
        /// <returns>Join</returns>
        public static IAlgebra CreateDirect(IAlgebra lhs, IAlgebra rhs)
        {
            return new Join(lhs, rhs);
        }

        private static bool IsTableUnit(IAlgebra algebra)
        {
            if (algebra is Table)
            {
                return ((Table) algebra).IsUnit;
            }
            if (algebra is Bgp)
            {
                return ((Bgp) algebra).TriplePatterns.Count == 0;
            }
            return false;
        }

        public override IAlgebra Copy(IAlgebra lhs, IAlgebra rhs)
        {
            return Create(lhs, rhs);
        }

        public override IEnumerable<string> ProjectedVariables
        {
            get { return this.Lhs.ProjectedVariables.Concat(this.Rhs.ProjectedVariables).Distinct(); }
        }

        public override IEnumerable<string> FixedVariables
        {
            get { return this.Lhs.FixedVariables.Concat(this.Rhs.FixedVariables).Distinct(); }
        }

        public override IEnumerable<string> FloatingVariables
        {
            get { return this.Lhs.FloatingVariables.Concat(this.Rhs.FloatingVariables).Distinct().Except(this.FixedVariables); }
        }

        public override void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }

        public override bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is Join)) return false;

            Join j = (Join) other;
            return this.Lhs.Equals(j.Lhs) && this.Rhs.Equals(j.Rhs);
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("(join");
            builder.AppendLineIndented(this.Lhs.ToString(formatter), 2);
            builder.AppendLineIndented(this.Rhs.ToString(formatter), 2);
            builder.Append(")");
            return builder.ToString();
        }
    }
}
