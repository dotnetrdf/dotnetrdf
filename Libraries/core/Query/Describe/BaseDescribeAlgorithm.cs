/*

Copyright Robert Vesse 2009-11
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
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Describe
{
    /// <summary>
    /// Abstract Base Class for SPARQL Describe Algorithms which provides BNode rewriting functionality
    /// </summary>
    public abstract class BaseDescribeAlgorithm
        : ISparqlDescribe
    {
        /// <summary>
        /// Gets the Description Graph based on the Query Results from the given Evaluation Context
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public IGraph Describe(SparqlEvaluationContext context)
        {
            Graph g = new Graph();
            this.Describe(new GraphHandler(g), context);
            return g;
        }

        /// <summary>
        /// Gets the Description Graph based on the Query Results from the given Evaluation Context passing the resulting Triples to the given RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public void Describe(IRdfHandler handler, SparqlEvaluationContext context)
        {
            try
            {
                handler.StartRdf();

                //Apply Base URI and Namespaces to the Handler
                if (context.Query != null)
                {
                    if (context.Query.BaseUri != null)
                    {
                        if (!handler.HandleBaseUri(context.Query.BaseUri)) ParserHelper.Stop();
                    }
                    foreach (String prefix in context.Query.NamespaceMap.Prefixes)
                    {
                        if (!handler.HandleNamespace(prefix, context.Query.NamespaceMap.GetNamespaceUri(prefix))) ParserHelper.Stop();
                    }
                }

                //Get the Nodes needing describing
                List<INode> nodes = this.GetNodes(handler, context);
                if (nodes.Count > 0)
                {
                    //If there is at least 1 Node then start describing
                    this.DescribeInternal(handler, context, nodes);
                }

                handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                handler.EndRdf(true);
            }
            catch
            {
                handler.EndRdf(false);
                throw;
            }
        }

        /// <summary>
        /// Generates the Description for each of the Nodes to be described
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="nodes">Nodes to be described</param>
        protected abstract void DescribeInternal(IRdfHandler handler, SparqlEvaluationContext context, IEnumerable<INode> nodes);

        /// <summary>
        /// Gets the Nodes that the algorithm should generate the descriptions for
        /// </summary>
        /// <param name="factory">Factory to create Nodes in</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        private List<INode> GetNodes(INodeFactory factory, SparqlEvaluationContext context)
        {
            INamespaceMapper nsmap = (context.Query != null ? context.Query.NamespaceMap : new NamespaceMapper(true));
            Uri baseUri = (context.Query != null ? context.Query.BaseUri : null);

            //Build a list of INodes to describe
            List<INode> nodes = new List<INode>();
            foreach (IToken t in context.Query.DescribeVariables)
            {
                switch (t.TokenType)
                {
                    case Token.QNAME:
                    case Token.URI:
                        //Resolve Uri/QName
                        nodes.Add(factory.CreateUriNode(new Uri(Tools.ResolveUriOrQName(t, nsmap, baseUri))));
                        break;

                    case Token.VARIABLE:
                        //Get Variable Values
                        String var = t.Value.Substring(1);
                        if (context.OutputMultiset.ContainsVariable(var))
                        {
                            foreach (ISet s in context.OutputMultiset.Sets)
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
            return nodes;
        }

        //OPT: Replace with usage of MapTriple instead?

        /// <summary>
        /// Helper method which rewrites Blank Node IDs for Describe Queries
        /// </summary>
        /// <param name="t">Triple</param>
        /// <param name="mapping">Mapping of IDs to new Blank Nodes</param>
        /// <param name="factory">Factory to create Nodes in</param>
        /// <returns></returns>
        protected Triple RewriteDescribeBNodes(Triple t, Dictionary<String, INode> mapping, INodeFactory factory)
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
                    s = factory.CreateBlankNode(id);
                    mapping.Add(id, s);
                }
            }
            else
            {
                s = Tools.CopyNode(t.Subject, factory);
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
                    p = factory.CreateBlankNode(id);
                    mapping.Add(id, p);
                }
            }
            else
            {
                p = Tools.CopyNode(t.Predicate, factory);
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
                    o = factory.CreateBlankNode(id);
                    mapping.Add(id, o);
                }
            }
            else
            {
                o = Tools.CopyNode(t.Object, factory);
            }

            return new Triple(s, p, o);
        }
    }
}
