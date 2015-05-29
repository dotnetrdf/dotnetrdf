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
using VDS.RDF.Query.Expressions;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public class Filter
        : BaseUnaryAlgebra
    {
        private Filter(IAlgebra innerAlgebra, IEnumerable<IExpression> expressions)
            : base(innerAlgebra)
        {
            if (expressions == null) throw new ArgumentNullException("expressions");
            this.Expressions = expressions.ToList().AsReadOnly();
        }

        public static Filter Create(IAlgebra innerAlgebra, IEnumerable<IExpression> expressions)
        {
            if (!(innerAlgebra is Filter)) return Wrap(innerAlgebra, expressions);

            Filter f = (Filter) innerAlgebra;
            return new Filter(f.InnerAlgebra, f.Expressions.Concat(expressions));
        }

        public static Filter Wrap(IAlgebra innerAlgebra, IEnumerable<IExpression> expressions)
        {
            return new Filter(innerAlgebra, expressions);
        }

        public IList<IExpression> Expressions { get; private set; }

        public override IAlgebra Copy(IAlgebra innerAlgebra)
        {
            return Create(innerAlgebra, this.Expressions);
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
            if (!(other is Filter)) return false;

            Filter f = (Filter) other;
            if (this.Expressions.Count != f.Expressions.Count) return false;

            for (int i = 0; i < this.Expressions.Count; i++)
            {
                if (!this.Expressions[i].Equals(f.Expressions[i])) return false;
            }
            return true;
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("(filter (exprlist ");
            for (int i = 0; i < this.Expressions.Count; i++)
            {
                if (i > 0) builder.Append(' ');
                builder.Append("(");
                builder.Append(this.Expressions[i].ToPrefixString(formatter));
                builder.Append(")");
            }
            builder.AppendLine(")");
            builder.AppendLineIndented(this.InnerAlgebra.ToString(), 2);
            builder.AppendLine(")");
            return builder.ToString();
        }
    }
}
