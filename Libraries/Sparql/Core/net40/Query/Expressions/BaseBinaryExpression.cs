/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using VDS.RDF.Specifications;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Abstract base class for Binary Expressions
    /// </summary>
    public abstract class BaseBinaryExpression
        : IBinaryExpression
    {
        /// <summary>
        /// Creates a new Base Binary Expression
        /// </summary>
        /// <param name="leftExpr">Left Expression</param>
        /// <param name="rightExpr">Right Expression</param>
        protected BaseBinaryExpression(IExpression leftExpr, IExpression rightExpr)
        {
            this.FirstArgument = leftExpr;
            this.SecondArgument = rightExpr;
        }

        /// <summary>
        /// The sub-expressions of this Expression
        /// </summary>
        public IExpression FirstArgument { get; private set; }

        /// <summary>
        /// The sub-expressions of this Expression
        /// </summary>
        public IExpression SecondArgument { get; private set; }

        public virtual IExpression Copy()
        {
            return Copy(this.FirstArgument.Copy(), this.SecondArgument.Copy());
        }

        public abstract IExpression Copy(IExpression arg1, IExpression arg2);

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public abstract IValuedNode Evaluate(ISolution solution, IExpressionContext context);

        public abstract bool Equals(IExpression other);

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public sealed override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        public string ToString(IAlgebraFormatter formatter)
        {
            String f = SparqlSpecsHelper.IsFunctionKeyword11(this.Functor) ? this.Functor.ToLowerInvariant() : formatter.FormatUri(this.Functor);
            return String.Format("{0}({1}, {2})", f, this.FirstArgument.ToString(formatter), this.SecondArgument.ToString(formatter));
        }

        public string ToPrefixString()
        {
            return ToPrefixString(new AlgebraFormatter());
        }

        public string ToPrefixString(IAlgebraFormatter formatter)
        {
            String f = SparqlSpecsHelper.IsFunctionKeyword11(this.Functor) ? this.Functor.ToLowerInvariant() : formatter.FormatUri(this.Functor);
            return String.Format("({0} {1} {2})", f, this.FirstArgument.ToPrefixString(formatter), this.SecondArgument.ToPrefixString(formatter));
        }

        /// <summary>
        /// Gets an enumeration of all the Variables used in this expression
        /// </summary>
        public virtual IEnumerable<String> Variables
        {
            get
            {
                return this.FirstArgument.Variables.Concat(this.SecondArgument.Variables).Distinct();
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public abstract String Functor
        {
            get;
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                // Assume we can parallelise if both arguments can
                return this.FirstArgument.CanParallelise && this.SecondArgument.CanParallelise;
            }
        }

        public virtual bool IsDeterministic
        {
            get
            {
                // Assume we are deterministic if both arguments are
                return this.FirstArgument.IsDeterministic && this.SecondArgument.IsDeterministic;
            }
        }

        public virtual bool IsConstant
        {
            get
            {
                // If we are deterministic and both arguments are constant then assume we are constant
                return this.IsDeterministic && this.FirstArgument.IsConstant && this.SecondArgument.IsConstant;
            }
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(Object other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is BaseBinaryExpression)) return false;

            return this.Equals((BaseBinaryExpression) other);
        }

        public override int GetHashCode()
        {
            return Tools.CombineHashCodes(this.Functor, this.FirstArgument, this.SecondArgument);
        }
    }
}