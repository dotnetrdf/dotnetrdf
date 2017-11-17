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
            Describe(new GraphHandler(g), context);
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

                // Apply Base URI and Namespaces to the Handler
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

                // Get the Nodes needing describing
                List<INode> nodes = GetNodes(handler, context);
                if (nodes.Count > 0)
                {
                    // If there is at least 1 Node then start describing
                    DescribeInternal(handler, context, nodes);
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

            // Build a list of INodes to describe
            List<INode> nodes = new List<INode>();
            foreach (IToken t in context.Query.DescribeVariables)
            {
                switch (t.TokenType)
                {
                    case Token.QNAME:
                    case Token.URI:
                        // Resolve Uri/QName
                        nodes.Add(factory.CreateUriNode(UriFactory.Create(Tools.ResolveUriOrQName(t, nsmap, baseUri))));
                        break;

                    case Token.VARIABLE:
                        // Get Variable Values
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

        // OPT: Replace with usage of MapTriple instead?

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
