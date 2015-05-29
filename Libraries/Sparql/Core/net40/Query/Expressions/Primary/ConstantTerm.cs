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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions.Primary
{
    /// <summary>
    /// Class for representing constant terms
    /// </summary>
    public class ConstantTerm
        : BaseNullaryExpression
    {
        /// <summary>
        /// Creates a new Constant
        /// </summary>
        /// <param name="n">Valued Node</param>
        public ConstantTerm(IValuedNode n)
        {
            this.Node = n;
        }

        /// <summary>
        /// Creates a new Constant
        /// </summary>
        /// <param name="n">Node</param>
        public ConstantTerm(INode n)
            : this(n.AsValuedNode()) { }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            return this.Node;
        }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is ConstantTerm)) return false;

            return EqualityHelper.AreNodesEqual(this.Node, ((ConstantTerm) other).Node);
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString(IAlgebraFormatter formatter)
        {
            return this.Node.ToString(formatter);
        }

        /// <summary>
        /// Gets an Empty Enumerable since a Node Term does not use variables
        /// </summary>
        public override IEnumerable<String> Variables
        {
            get
            {
                return Enumerable.Empty<String>();
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get { return null; }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public override bool CanParallelise
        {
            get
            {
                return true;
            }
        }

        public override bool IsDeterministic
        {
            get { return true; }
        }

        public override bool IsConstant
        {
            get { return true; }
        }

        public override string ToPrefixString(IAlgebraFormatter formatter)
        {
            return this.Node.ToString(formatter);
        }

        public override IExpression Copy()
        {
            return new ConstantTerm(this.Node);
        }

        public override bool Equals(object other)
        {
            if (other is ConstantTerm) return Equals((IExpression) other);
            return false;
        }

        public override int GetHashCode()
        {
            return this.Node.GetHashCode();
        }

        /// <summary>
        /// Node this Term represents
        /// </summary>
        public IValuedNode Node { get; protected set; }
    }
}
