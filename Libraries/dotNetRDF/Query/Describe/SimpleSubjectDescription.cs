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
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Describe
{
    /// <summary>
    /// Computes a Simple Subject Description for all Values resulting from the Query
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Description returned is all the Triples for which a Value is the Subject - this description does not expand any Blank Nodes
    /// </para>
    /// </remarks>
    public class SimpleSubjectDescription 
        : BaseDescribeAlgorithm
    {
        /// <summary>
        /// Generates the Description for each of the Nodes to be described
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="nodes">Nodes to be described</param>
        protected override void DescribeInternal(IRdfHandler handler, SparqlEvaluationContext context, IEnumerable<INode> nodes)
        {
            // Rewrite Blank Node IDs for DESCRIBE Results
            Dictionary<String, INode> bnodeMapping = new Dictionary<string, INode>();

            // Get Triples for this Subject
            foreach (INode subj in nodes)
            {
                // Get Triples where the Node is the Subject
                foreach (Triple t in context.Data.GetTriplesWithSubject(subj).ToList())
                {
                    if (!handler.HandleTriple((RewriteDescribeBNodes(t, bnodeMapping, handler)))) ParserHelper.Stop();
                }
            }
        }
    }
}
