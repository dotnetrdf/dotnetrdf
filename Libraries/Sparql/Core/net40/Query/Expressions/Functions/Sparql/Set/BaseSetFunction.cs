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
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Set
{
    /// <summary>
    /// Abstract base class for SPARQL Functions which operate on Sets
    /// </summary>
    public abstract class BaseSetFunction
        : BaseNAryExpression
    {
        /// <summary>
        /// Creates a new SPARQL Set function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="set">Set</param>
        protected BaseSetFunction(IExpression expr, IEnumerable<IExpression> set)
            : base(expr.AsEnumerable().Concat(set))
        {
            if (this.Arguments.Count == 0) throw new ArgumentException("Requires at least one argument");
        }

        public sealed override string ToString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.Arguments[0].ToString(formatter));
            builder.Append(' ');
            builder.Append(this.Functor);
            builder.Append(" (");
            for (int i = 1; i < this.Arguments.Count; i++)
            {
                if (i > 1) builder.Append(", ");
                builder.Append(this.Arguments[i].ToString(formatter));
            }
            builder.Append(')');
            return builder.ToString();
        }
    }
}
