using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Engine.Medusa
{
    public class QuadStoreBgpExecutor
        : IBgpExecutor
    {
        public QuadStoreBgpExecutor(IQuadStore quadStore)
        {
            if (quadStore == null) throw new ArgumentNullException("quadStore");
            this.QuadStore = quadStore;
        }

        public IQuadStore QuadStore { get; private set; }

        public IEnumerable<ISet> Match(INode graphName, Triple t)
        {
            if (t.IsGroundTriple)
            {
                // Return an empty row for each quad that matches
                return this.QuadStore.Find(graphName, t.Subject, t.Predicate, t.Object).Select(q => new Set());
            }
            INode s = ToFindItem(t.Subject);
            INode p = ToFindItem(t.Predicate);
            INode o = ToFindItem(t.Object);
            return this.QuadStore.Find(graphName, s, p, o).Select(q => QuadToSet(q, t));
        }

        public IEnumerable<ISet> Match(INode graphName, Triple t, ISet input)
        {
            if (t.IsGroundTriple)
            {
                // Return an empty row for each quad that matches
                return this.QuadStore.Find(graphName, t.Subject, t.Predicate, t.Object).Select(q => input);
            }
            INode s = ToFindItem(t.Subject, input);
            INode p = ToFindItem(t.Predicate, input);
            INode o = ToFindItem(t.Object, input);
            return this.QuadStore.Find(graphName, s, p, o).Select(q => QuadToSet(q, t, input));
        }

        private ISet QuadToSet(Quad matched, Triple pattern)
        {
            return QuadToSet(matched, pattern, null);
        }

        private ISet QuadToSet(Quad matched, Triple pattern, ISet set)
        {
            ISet output = set != null ? new Set(set) : new Set();
            String sVar = ToVarName(pattern.Subject);
            if (sVar != null && output[sVar] == null) output.Add(sVar, matched.Subject);
            String pVar = ToVarName(pattern.Predicate);
            if (pVar != null && output[pVar] == null) output.Add(pVar, matched.Predicate);
            String oVar = ToVarName(pattern.Object);
            if (oVar != null && output[oVar] == null) output.Add(oVar, matched.Object);

            return output;
        }

        private static INode ToFindItem(INode n)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.Variable:
                    return null;
                default:
                    return n;
            }
        }

        private static INode ToFindItem(INode n, ISet set)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    return set[n.AnonID.ToString()];
                case NodeType.Variable:
                    return set[n.VariableName];
                default:
                    return n;
            }
        }

        private static String ToVarName(INode n)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    return n.AnonID.ToString();
                case NodeType.Variable:
                    return n.VariableName;
                default:
                    return null;
            }
        }
    }
}