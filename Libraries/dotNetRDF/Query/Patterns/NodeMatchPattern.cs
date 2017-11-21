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

using VDS.RDF.Query.Construct;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Pattern which matches specific Nodes
    /// </summary>
    public class NodeMatchPattern
        : PatternItem
    {
        private INode _node;

        /// <summary>
        /// Creates a new Node Match Pattern
        /// </summary>
        /// <param name="n">Exact Node to match</param>
        public NodeMatchPattern(INode n)
        {
            _node = n;
        }

        /// <summary>
        /// Creates a new Node Match Pattern
        /// </summary>
        /// <param name="n">Exact Node to match</param>
        /// <param name="rigorousEvaluation">Whether to force rigorous evaluation regardless of the global setting</param>
        public NodeMatchPattern(INode n, bool rigorousEvaluation)
            : this(n)
        {
            RigorousEvaluation = rigorousEvaluation;
        }

        /// <summary>
        /// Checks whether the given Node matches the Node this pattern was instantiated with
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="obj">Node to test</param>
        /// <returns></returns>
        protected internal override bool Accepts(SparqlEvaluationContext context, INode obj)
        {
            if (Options.RigorousEvaluation || RigorousEvaluation)
            {
                return _node.Equals(obj);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Constructs a Node based on the given Set
        /// </summary>
        /// <param name="context">Construct Context</param>
        protected internal override INode Construct(ConstructContext context)
        {
            return context.GetNode(_node);
        }

        /// <summary>
        /// Gets a String representation of the Node
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.Formatter.Format(_node);
        }

        /// <summary>
        /// Gets the Node that this Pattern matches
        /// </summary>
        public INode Node
        {
            get
            {
                return _node;
            }
        }
    }
}
