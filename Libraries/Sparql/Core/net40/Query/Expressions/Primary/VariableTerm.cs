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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions.Primary
{
    /// <summary>
    /// Class representing Variable value expressions
    /// </summary>
    public class VariableTerm
        : BaseNullaryExpression
    {
        /// <summary>
        /// Gets the Variable Name
        /// </summary>
        public string VariableName { get; private set; }

        /// <summary>
        /// Creates a new Variable Expression
        /// </summary>
        /// <param name="name">Variable Name</param>
        public VariableTerm(String name)
        {
            this.VariableName = name;

            //Strip leading ?/$ if present
            if (this.VariableName.StartsWith("?") || this.VariableName.StartsWith("$"))
            {
                this.VariableName = this.VariableName.Substring(1);
            }
        }
        
        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            INode value = solution[this.VariableName];
            return value.AsValuedNode();
        }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is VariableTerm)) return false;

            return this.VariableName.Equals(((VariableTerm) other).VariableName);
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString(IAlgebraFormatter formatter)
        {
            return formatter.Format(new VariableNode(this.VariableName));
        }

        /// <summary>
        /// Gets the enumeration containing the single variable that this expression term represents
        /// </summary>
        public override IEnumerable<String> Variables
        {
            get
            {
                return this.VariableName.AsEnumerable();
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        /// <returns>An operator symbol, function keyword or URI if an operator/function.  Null otherwise</returns>
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
            get { return false; }
        }

        public override string ToPrefixString(IAlgebraFormatter formatter)
        {
            return formatter.Format(new VariableNode(this.VariableName));
        }

        public override IExpression Copy()
        {
            return new VariableTerm(this.VariableName);
        }

        public override bool Equals(object other)
        {
            if (other is VariableTerm) return Equals((IExpression) other);
            return false;
        }

        public override int GetHashCode()
        {
            return this.VariableName.GetHashCode();
        }
    }
}
