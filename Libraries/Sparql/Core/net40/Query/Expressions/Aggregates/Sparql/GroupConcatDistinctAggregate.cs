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
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Aggregation.Sparql;
using VDS.RDF.Specifications;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions.Aggregates.Sparql
{
    public class GroupConcatDistinctAggregate
        : BaseDistinctAggregate
    {
        public GroupConcatDistinctAggregate(IExpression expr)
            : this(expr, null) {}

        public GroupConcatDistinctAggregate(IExpression expr, IExpression separatorExpr)
            : base(MakeArguments(expr, separatorExpr)) {}

        private static IEnumerable<IExpression> MakeArguments(IExpression expr, IExpression separatorExpr)
        {
            return separatorExpr != null ? new IExpression[] {expr, separatorExpr} : expr.AsEnumerable();
        }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            List<IExpression> exprs = args.ToList();
            return exprs.Count == 1 ? new GroupConcatDistinctAggregate(exprs[0], null) : new GroupConcatDistinctAggregate(exprs[0], exprs[1]);
        }

        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordGroupConcat; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return new DistinctShortCircuitAccumulator(this.Arguments.Count > 1 ? new GroupConcatAccumulator(this.Arguments[0], this.Arguments[1]) : new GroupConcatAccumulator(this.Arguments[0]));
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.Functor.ToLowerInvariant());
            builder.Append("(DISTINCT ");
            for (int i = 0; i < this.Arguments.Count - 1; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(this.Arguments[i].ToString(formatter));
            }
            if (this.Arguments.Count > 1)
            {
                builder.Append("; SEPARATOR = ");
                builder.Append(this.Arguments[this.Arguments.Count - 1].ToString(formatter));
            }
            builder.Append(")");
            return builder.ToString();
        }

        public override string ToPrefixString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('(');
            builder.Append(this.Functor.ToLowerInvariant());
            builder.Append(" distinct");
            if (this.Arguments.Count > 1)
            {
                builder.Append(" (separator ");
                builder.Append(this.Arguments[this.Arguments.Count - 1].ToPrefixString(formatter));
                builder.Append(')');
            }
            for (int i = 0; i < this.Arguments.Count - 1; i++)
            {
                builder.Append(' ');
                builder.Append(this.Arguments[i].ToPrefixString(formatter));
            }
            builder.Append(')');
            return builder.ToString();
        }
    }
}