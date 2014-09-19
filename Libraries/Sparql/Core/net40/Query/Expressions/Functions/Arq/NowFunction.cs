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
using VDS.RDF.Query.Expressions.Factories;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions.Functions.Arq
{
    /// <summary>
    /// Represents the ARQ afn:now() function
    /// </summary>
    public class NowFunction 
        : BaseNullaryExpression
    {
        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            return other is NowFunction;
        }

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns>
        /// Returns a constant Literal Node which is a Date Time typed Literal
        /// </returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            return new DateTimeNode(context.ParentContext.EffectiveNow);
        }

        public override IEnumerable<string> Variables
        {
            get { return Enumerable.Empty<String>(); }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Now;
            }
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
            get { return false; }
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            return String.Format("{0}()", formatter.FormatUri(this.Functor));
        }

        public override string ToPrefixString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            return String.Format("({0})", formatter.FormatUri(this.Functor));
        }

        public override IExpression Copy()
        {
            return new NowFunction();
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            return other is NowFunction;
        }

        public override int GetHashCode()
        {
            return this.Functor.GetHashCode();
        }
    }
}
