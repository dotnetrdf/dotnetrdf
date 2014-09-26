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

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Numeric
{
    /// <summary>
    /// Represents the SPARQL RAND() Function
    /// </summary>
    public class RandFunction
        : BaseNullaryExpression
    {
        private static readonly Random _rnd = new Random();

        /// <summary>
        /// Creates a new SPARQL RAND() Function
        /// </summary>
        public RandFunction() { }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            return other is RandFunction;
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            return new DoubleNode(_rnd.NextDouble());
        }

        /// <summary>
        /// Gets the Variables used in this Expression
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public override bool CanParallelise
        {
            get { return false; }
        }

        public override bool IsDeterministic
        {
            get { return false; }
        }

        public override bool IsConstant
        {
            get { return false; }
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            return System.String.Format("{0}()", this.Functor);
        }

        public override string ToPrefixString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            return System.String.Format("({0})", this.Functor);
        }

        public override IExpression Copy()
        {
            return new RandFunction();
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            return other is RandFunction;
        }

        public override int GetHashCode()
        {
            return this.Functor.GetHashCode();
        }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordRand;
            }
        }
    }
}
