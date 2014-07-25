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

namespace VDS.RDF.Query.Expressions.Primary
{
    /// <summary>
    /// Class representing Variable value expressions
    /// </summary>
    public class VariableTerm
        : INullaryExpression
    {
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
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            INode value = solution[this.VariableName];
            return value.AsValuedNode();
        }

        public bool Equals(IExpression other)
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
        public override string ToString()
        {
            return "?" + this.VariableName;
        }

        /// <summary>
        /// Gets the enumeration containing the single variable that this expression term represents
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return this.VariableName.AsEnumerable();
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public String Functor
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return true;
            }
        }

        public bool IsDeterministic
        {
            get { return true; }
        }

        public bool IsConstant
        {
            get { return false; }
        }

        public IExpression Copy()
        {
            return new VariableTerm(this.VariableName);
        }
    }
}
