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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Abstract base class for nullary expressions
    /// </summary>
    public abstract class BaseNullaryExpression
        : INullaryExpression
    {
        public abstract bool Equals(IExpression other);

        public abstract IValuedNode Evaluate(ISolution set, IExpressionContext context);

        public abstract IEnumerable<string> Variables { get; }

        public abstract string Functor { get; }

        public abstract bool CanParallelise { get; }

        public abstract bool IsDeterministic { get; }

        public abstract bool IsConstant { get; }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        public abstract string ToString(IAlgebraFormatter formatter);

        public string ToPrefixString()
        {
            return ToPrefixString(new AlgebraFormatter());
        }

        public abstract string ToPrefixString(IAlgebraFormatter formatter);

        public abstract IExpression Copy();

        public abstract override bool Equals(Object other);

        public abstract override int GetHashCode();
    }
}
