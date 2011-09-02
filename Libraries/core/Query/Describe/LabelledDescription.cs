/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Describe
{
    /// <summary>
    /// Computes a Labelled Description for all the Values resulting from the Query
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Description returned is all the Triples for which a Value is a Subject and with any Blank Nodes expanded to include their rdfs:label property if present
    /// </para>
    /// </remarks>
    public class LabelledDescription
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
            //Rewrite Blank Node IDs for DESCRIBE Results
            Dictionary<String, INode> bnodeMapping = new Dictionary<string, INode>();

            //Get Triples for this Subject
            Queue<INode> bnodes = new Queue<INode>();
            HashSet<INode> expandedBNodes = new HashSet<INode>();
            INode rdfsLabel = handler.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            foreach (INode n in nodes)
            {
                //Get Triples where the Node is the Subject
                foreach (Triple t in context.Data.GetTriplesWithSubject(n))
                {
                    if (t.Object.NodeType == NodeType.Blank)
                    {
                        if (!expandedBNodes.Contains(t.Object)) bnodes.Enqueue(t.Object);
                    }
                    if (!handler.HandleTriple((this.RewriteDescribeBNodes(t, bnodeMapping, handler)))) ParserHelper.Stop();
                }

                //Compute the Blank Node Closure for this Subject
                while (bnodes.Count > 0)
                {
                    INode bsubj = bnodes.Dequeue();
                    if (expandedBNodes.Contains(bsubj)) continue;
                    expandedBNodes.Add(bsubj);

                    foreach (Triple t2 in context.Data.GetTriplesWithSubjectPredicate(bsubj, rdfsLabel))
                    {
                        if (!handler.HandleTriple((this.RewriteDescribeBNodes(t2, bnodeMapping, handler)))) ParserHelper.Stop();
                    }
                }
            }
        }
    }
}
