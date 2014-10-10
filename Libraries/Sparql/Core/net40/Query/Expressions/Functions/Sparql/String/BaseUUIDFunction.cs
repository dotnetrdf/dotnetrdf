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
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Abstract Base Class for functions that generate UUIDs
    /// </summary>
    public abstract class BaseUuidFunction
        : BaseNullaryExpression
    {
        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            Guid uuid = Guid.NewGuid();
            return this.EvaluateInternal(uuid);
        }

        /// <summary>
        /// Method to be implemented by derived classes to implement the actual logic of turning the generated UUID into a RDF term
        /// </summary>
        /// <param name="uuid">UUID</param>
        /// <returns></returns>
        protected abstract IValuedNode EvaluateInternal(Guid uuid);

        /// <summary>
        /// Gets the variables used in the expression
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get { return Enumerable.Empty<System.String>(); }
        }

        /// <summary>
        /// Gets the arguments of the expression
        /// </summary>
        public virtual IEnumerable<IExpression> Arguments
        {
            get { return Enumerable.Empty<IExpression>(); }
        }

        /// <summary>
        /// Returns whether the function can be parallelised
        /// </summary>
        public override sealed bool CanParallelise
        {
            get { return true; }
        }

        public override sealed bool IsConstant
        {
            get { return false; }
        }

        public override bool IsDeterministic
        {
            get { return false; }
        }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is BaseUuidFunction)) return false;

            BaseUuidFunction func = (BaseUuidFunction) other;
            return this.Functor.Equals(func.Functor);
        }

        public override int GetHashCode()
        {
            return this.Functor.GetHashCode();
        }

        public override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is INullaryExpression)) return false;

            return this.Equals((INullaryExpression) other);
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException();
            return System.String.Format("{0}()", this.Functor);
        }

        public override string ToPrefixString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException();
            return System.String.Format("({0})", this.Functor.ToLowerInvariant());
        }
    }
}