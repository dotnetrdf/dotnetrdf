using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public abstract Graph Describe(SparqlEvaluationContext context);

        //OPT: Replace with usage of MapTriple instead?

        /// <summary>
        /// Helper method which rewrites Blank Node IDs for Describe Queries
        /// </summary>
        /// <param name="t">Triple</param>
        /// <param name="mapping">Mapping of IDs to new Blank Nodes</param>
        /// <param name="g">Graph of the Description</param>
        /// <returns></returns>
        protected Triple RewriteDescribeBNodes(Triple t, Dictionary<String, INode> mapping, Graph g)
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
