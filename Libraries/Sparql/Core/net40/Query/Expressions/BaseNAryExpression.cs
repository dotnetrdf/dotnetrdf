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
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Abstract base class for Binary Expressions
    /// </summary>
    public abstract class BaseNAryExpression
        : INAryExpression
    {
        /// <summary>
        /// Creates a new Base Binary Expression
        /// </summary>
        /// <param name="args">Arguments</param>
        protected BaseNAryExpression(IEnumerable<IExpression> args)
        {
            this.Arguments = (args != null ? args.ToList() : new List<IExpression>()).AsReadOnly();
        }

        /// <summary>
        /// The first argument of this expression
        /// </summary>
        public IList<IExpression> Arguments { get; private set; }

        public virtual IExpression Copy()
        {
            return Copy(this.Arguments.Select(a => a.Copy()));
        }

        public abstract IExpression Copy(IEnumerable<IExpression> args);

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

        public virtual string ToString(IAlgebraFormatter formatter)
        {
            String f = SparqlSpecsHelper.IsFunctionKeyword11(this.Functor) ? this.Functor.ToLowerInvariant() : formatter.FormatUri(this.Functor);
            StringBuilder builder = new StringBuilder();
            builder.Append(f);
            builder.Append('(');
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(this.Arguments[i].ToString(formatter));
            }
            builder.Append(')');
            return builder.ToString();
        }

        public string ToPrefixString()
        {
            return ToPrefixString(new AlgebraFormatter());
        }

        public virtual string ToPrefixString(IAlgebraFormatter formatter)
        {
            String f = SparqlSpecsHelper.IsFunctionKeyword11(this.Functor) ? this.Functor.ToLowerInvariant() : formatter.FormatUri(this.Functor);
            StringBuilder builder = new StringBuilder();
            builder.Append('(');
            builder.Append(f);
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                if (i > 0) builder.Append(" ");
                builder.Append(this.Arguments[i].ToPrefixString(formatter));
            }
            builder.Append(')');
            return builder.ToString();
        }

        /// <summary>
        /// Gets an enumeration of all the Variables used in this expression
        /// </summary>
        public virtual IEnumerable<String> Variables
        {
            get { return this.Arguments.SelectMany(a => a.Variables).Distinct(); }
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
                // Assume we can parallelise if all arguments can
                return this.Arguments.All(a => a.CanParallelise);
            }
        }

        public virtual bool IsDeterministic
        {
            get
            {
                // Assume we are deterministic if all arguments are
                return this.Arguments.All(a => a.IsDeterministic);
            }
        }

        public virtual bool IsConstant
        {
            get
            {
                // If we are deterministic and all arguments are constant then assume we are constant
                return this.IsDeterministic && this.Arguments.All(a => a.IsConstant);
            }
        }

        public virtual void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(Object other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is BaseTernaryExpression)) return false;

            return this.Equals((BaseTernaryExpression) other);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            return Tools.CombineHashCodes(this.Functor.AsEnumerable().Concat<object>(this.Arguments));
        }
    }
}