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
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Describe
{
    /// <summary>
    /// Computes a Concise Bounded Description for all the Subjects resulting
    /// </summary>
    public class ConciseBoundedDescription : ISparqlDescribe
    {
        /// <summary>
        /// Returns the Graph which is the Result of the Describe Query by computing the Concise Bounded Description for all Results
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public Graph Describe(SparqlEvaluationContext context)
        {
            //Get a new empty Graph and import the Base Uri and Namespace Map of the Query
            Graph g = new Graph();
            g.BaseUri = context.Query.BaseUri;
            g.NamespaceMap.Import(context.Query.NamespaceMap);

            //Build a list of INodes to describe
            List<INode> nodes = new List<INode>();
            foreach (IToken t in context.Query.DescribeVariables)
            {
                switch (t.TokenType)
                {
                    case Token.QNAME:
                    case Token.URI:
                        //Resolve Uri/QName
                        nodes.Add(new UriNode(g, new Uri(Tools.ResolveUriOrQName(t, g.NamespaceMap, g.BaseUri))));
                        break;

                    case Token.VARIABLE:
                        //Get Variable Values
                        String var = t.Value.Substring(1);
                        if (context.OutputMultiset.ContainsVariable(var))
                        {
                            foreach (Set s in context.OutputMultiset.Sets)
                            {
                                INode temp = s[var];
                                if (temp != null) nodes.Add(temp);
                            }
                        }
                        break;

                    default:
                        throw new RdfQueryException("Unexpected Token '" + t.GetType().ToString() + "' in DESCRIBE Variables list");
                }
            }

            //Rewrite Blank Node IDs for DESCRIBE Results
            Dictionary<String, INode> bnodeMapping = new Dictionary<string, INode>();

            //Get Triples for this Subject
            Queue<INode> bnodes = new Queue<INode>();
            foreach (INode subj in nodes)
            {
                //Get Triples where the Node is the Subject
                foreach (Triple t in context.Data.GetTriplesWithSubject(subj))
                {
                    if (t.Object.NodeType == NodeType.Blank)
                    {
                        bnodes.Enqueue(t.Object);
                    }
                    g.Assert(this.RewriteDescribeBNodes(t, bnodeMapping, g));
                }

                //Compute the Blank Node Closure for this Subject
                while (bnodes.Count > 0)
                {
                    INode bsubj = bnodes.Dequeue();

                    foreach (Triple t2 in context.Data.GetTriplesWithSubject(bsubj))
                    {
                        if (t2.Object.NodeType == NodeType.Blank)
                        {
                            bnodes.Enqueue(t2.Object);
                        }
                        g.Assert(this.RewriteDescribeBNodes(t2, bnodeMapping, g));
                    }
                }
            }

            //Return the Graph
            g.BaseUri = null;
            return g;
        }

        //OPT: Replace with usage of MapTriple instead?

        /// <summary>
        /// Helper method which rewrites Blank Node IDs for Describe Queries
        /// </summary>
        /// <param name="t">Triple</param>
        /// <param name="mapping">Mapping of IDs to new Blank Nodes</param>
        /// <param name="g">Graph of the Description</param>
        /// <returns></returns>
        private Triple RewriteDescribeBNodes(Triple t, Dictionary<String, INode> mapping, Graph g)
        {
            INode s, p, o;
            String id;

            if (t.Subject.NodeType == NodeType.Blank)
            {
                id = t.Subject.GetHashCode() + "-" + t.Graph.GetHashCode();
                if (mapping.ContainsKey(id))
                {
                    s = mapping[id];
                }
                else
                {
                    s = g.CreateBlankNode(id);
                    mapping.Add(id, s);
                }
            }
            else
            {
                s = Tools.CopyNode(t.Subject, g);
            }

            if (t.Predicate.NodeType == NodeType.Blank)
            {
                id = t.Predicate.GetHashCode() + "-" + t.Graph.GetHashCode();
                if (mapping.ContainsKey(id))
                {
                    p = mapping[id];
                }
                else
                {
                    p = g.CreateBlankNode(id);
                    mapping.Add(id, p);
                }
            }
            else
            {
                p = Tools.CopyNode(t.Predicate, g);
            }

            if (t.Object.NodeType == NodeType.Blank)
            {
                id = t.Object.GetHashCode() + "-" + t.Graph.GetHashCode();
                if (mapping.ContainsKey(id))
                {
                    o = mapping[id];
                }
                else
                {
                    o = g.CreateBlankNode(id);
                    mapping.Add(id, o);
                }
            }
            else
            {
                o = Tools.CopyNode(t.Object, g);
            }

            return new Triple(s, p, o);
        }
    }
}
