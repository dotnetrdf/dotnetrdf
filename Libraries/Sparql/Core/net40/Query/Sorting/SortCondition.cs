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
using VDS.RDF.Collections;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Sorting
{
    public class SortCondition
        : ISortCondition
    {
        public SortCondition(IExpression expression, bool isAscending)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            this.Expression = expression;
            this.IsAscending = isAscending;
        }

        public SortCondition(IExpression expression)
            : this(expression, true) {}

        public bool IsAscending { get; private set; }

        public IExpression Expression { get; private set; }

        public bool Equals(ISortCondition other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return this.IsAscending == other.IsAscending && this.Expression.Equals(other.Expression);
        }

        public IComparer<ISolution> CreateComparer(IExpressionContext context)
        {
            IComparer<ISolution> comparer = new ExpressionValueComparer(this.Expression, context, new SparqlOrderingComparer());
            return this.IsAscending ? comparer : new ReversedComparer<ISolution>(comparer);
        }

        public override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        public string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            return this.IsAscending ? this.Expression.ToString(formatter) : String.Format("DESC({0})", this.Expression.ToString(formatter));
        }

        public string ToPrefixString()
        {
            return ToPrefixString(new AlgebraFormatter());
        }

        public string ToPrefixString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            return this.IsAscending ? this.Expression.ToPrefixString(formatter) : String.Format("(desc {0})", this.Expression.ToPrefixString(formatter));
        }
    }
}