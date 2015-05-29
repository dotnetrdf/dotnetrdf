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
using System.Text;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public class Slice
        : BaseUnaryAlgebra
    {
        public Slice(IAlgebra innerAlgebra, long limit, long offset)
            : base(innerAlgebra)
        {
            this.Limit = limit >= 0 ? limit : -1;
            this.Offset = offset > 0 ? offset : 0;
        }

        public long Limit { get; private set; }

        public long Offset { get; private set; }

        public override bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is Slice)) return false;

            Slice s = (Slice) other;
            return this.Limit == s.Limit && this.Offset == s.Offset;
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
            builder.Append("(slice ");
            if (this.Offset > 0)
            {
                builder.Append(this.Offset);
            }
            else
            {
                builder.Append('_');
            }
            builder.Append(' ');
            if (this.Limit > 0)
            {
                builder.Append(this.Limit);
            }
            else
            {
                builder.Append('_');
            }
            builder.AppendLine();
            builder.AppendLineIndented(this.InnerAlgebra.ToString(formatter), 2);
            builder.AppendLine(")");
            return builder.ToString();
        }

        public override IAlgebra Copy(IAlgebra innerAlgebra)
        {
            return new Slice(innerAlgebra, this.Limit, this.Offset);
        }
    }
}