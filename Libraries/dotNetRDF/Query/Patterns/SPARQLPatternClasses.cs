/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using VDS.RDF.Query.Construct;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Class for representing Node Patterns in Sparql Queries
    /// </summary>
    public abstract class PatternItem
    {
        /// <summary>
        /// Binding Context for Pattern Item
        /// </summary>
        protected SparqlResultBinder _context = null;

        private bool _repeated = false;
        private bool _rigorousEvaluation = false;

        /// <summary>
        /// Checks whether the Pattern Item accepts the given Node in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="obj">Node to test</param>
        /// <returns></returns>
        protected internal abstract bool Accepts(SparqlEvaluationContext context, INode obj);

        /// <summary>
        /// Constructs a Node based on this Pattern for the given Set
        /// </summary>
        /// <param name="context">Construct Context</param>
        /// <returns></returns>
        protected internal abstract INode Construct(ConstructContext context);

        /// <summary>
        /// Sets the Binding Context for the Pattern Item
        /// </summary>
        public SparqlResultBinder BindingContext
        {
            set
            {
                _context = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether rigorous evaluation is used, note that this setting may be overridden by the global <see cref="Options.RigorousEvaluation" /> option
        /// </summary>
        public bool RigorousEvaluation
        {
            get { return _rigorousEvaluation; }
            set { _rigorousEvaluation = value; }

        }

        /// <summary>
        /// Gets the String representation of the Pattern
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets the Variable Name if this is a Variable Pattern or null otherwise
        /// </summary>
        public virtual String VariableName
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Variable is repeated in the Pattern
        /// </summary>
        public virtual bool Repeated
        {
            get
            {
                return _repeated;
            }
            set
            {
                _repeated = value;
            }
        }

    }
}