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
    /// Pattern which matches the Blank Node with the given Internal ID regardless of the Graph the nodes come from
    /// </summary>
    public class FixedBlankNodePattern : PatternItem
    {
        private String _id;

        /// <summary>
        /// Creates a new Fixed Blank Node Pattern
        /// </summary>
        /// <param name="id">ID</param>
        public FixedBlankNodePattern(String id)
        {
            if (id.StartsWith("_:"))
            {
                _id = id.Substring(2);
            }
            else
            {
                _id = id;
            }
        }

        /// <summary>
        /// Gets the Blank Node ID
        /// </summary>
        public String InternalID
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Checks whether the pattern accepts the given Node
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="obj">Node to test</param>
        /// <returns></returns>
        protected internal override bool Accepts(SparqlEvaluationContext context, INode obj)
        {
            if (obj.NodeType == NodeType.Blank)
            {
                return ((IBlankNode)obj).InternalID.Equals(_id);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a Blank Node with a fixed ID scoped to whichever graph is provided
        /// </summary>
        /// <param name="context">Construct Context</param>
        protected internal override INode Construct(ConstructContext context)
        {
            if (context.Graph != null)
            {
                IBlankNode b = context.Graph.GetBlankNode(_id);
                if (b != null)
                {
                    return b;
                }
                else
                {
                    return context.Graph.CreateBlankNode(_id);
                }
            }
            else
            {
                return new BlankNode(context.Graph, _id);
            }
        }

        /// <summary>
        /// Gets the String representation of the Pattern Item
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<_:" + _id + ">";
        }
    }
}
