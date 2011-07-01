using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Describe
{
    /// <summary>
    /// Abstract Base Class for SPARQL Describe Algorithms which provides BNode rewriting functionality
    /// </summary>
    public abstract class BaseDescribeAlgorithm : ISparqlDescribe
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

        protected abstract void DescribeInternal(IRdfHandler handler, SparqlEvaluationContext context, IEnumerable<INode> nodes);

        /// <summary>
        /// Gets the Nodes that the algorithm should generate the descriptions for
        /// </summary>
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
        /// <param name="g">Graph of the Description</param>
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
