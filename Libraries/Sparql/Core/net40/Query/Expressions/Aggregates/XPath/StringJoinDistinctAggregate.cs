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
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Aggregation.XPath;
using VDS.RDF.Query.Expressions.Factories;

namespace VDS.RDF.Query.Expressions.Aggregates.XPath
{
    public class StringJoinDistinctAggregate
        : BaseDistinctAggregate
    {
        public StringJoinDistinctAggregate(IExpression expr)
            : this(expr, null) {}

        public StringJoinDistinctAggregate(IExpression expr, IExpression separatorExpr)
            : base(MakeArguments(expr, separatorExpr)) {}

        private static IEnumerable<IExpression> MakeArguments(IExpression expr, IExpression separatorExpr)
        {
            return separatorExpr != null ? new IExpression[] {expr, separatorExpr} : expr.AsEnumerable();
        }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            List<IExpression> exprs = args.ToList();
            return exprs.Count == 1 ? new StringJoinAggregate(exprs[0], null) : new StringJoinAggregate(exprs[0], exprs[1]);
        }

        public override string Functor
        {
            get { return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.StringJoin; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return new DistinctShortCircuitAccumulator(this.Arguments.Count > 1 ? new StringJoinAccumulator(this.Arguments[0], this.Arguments[1]) : new StringJoinAccumulator(this.Arguments[0]));
        }
    }
}