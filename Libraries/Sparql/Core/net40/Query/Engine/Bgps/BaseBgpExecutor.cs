/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Engine.Bgps
{
    /// <summary>
    /// Base implementation of a BGP executor which handles determining which graphs the matches should apply to
    /// </summary>
    public abstract class BaseBgpExecutor 
        : IBgpExecutor
    {
        public IEnumerable<ISolution> Match(Triple t, IExecutionContext context)
        {
            INode graphName = context.ActiveGraph;
            // If active graph not the special Default Graph Node then just matching a single specific graph
            if (!Quad.DefaultGraphNode.Equals(graphName)) return Match(graphName, t);

            // Possibly default graph is actually empty or the merge of multiple graphs
            switch (context.DefaultGraphs.Count)
            {
                case 0:
                    // Empty default graph
                    return Enumerable.Empty<ISolution>();
                case 1:
                    // The default graph is a single specific graph
                    return Match(graphName, t);
                default:
                    // Default graph is merge of multiple graphs
                    // Therefore the match means matching on all relevant graphs and applying distinct
                    // since identical matches could occur across graphs
                    return context.DefaultGraphs.SelectMany(n => Match(n, t)).Distinct();
            }
        }

        public IEnumerable<ISolution> Match(Triple t, ISolution input, IExecutionContext context)
        {
            INode graphName = context.ActiveGraph;
            // If active graph not the special Default Graph Node then just matching a single specific graph
            if (!Quad.DefaultGraphNode.Equals(graphName)) return Match(graphName, t, input);

            // Possibly default graph is actually empty or the merge of multiple graphs
            List<INode> graphNames = context.DefaultGraphs.ToList();
            switch (graphNames.Count)
            {
                case 0:
                    // Empty default graph
                    return Enumerable.Empty<ISolution>();
                case 1:
                    // The default graph is a single specific graph
                    return Match(graphName, t, input);
                default:
                    // Default graph is merge of multiple graphs
                    // Therefore the match means matching on all relevant graphs and applying distinct
                    // since identical matches could occur across graphs
                    return graphNames.SelectMany(n => Match(n, t, input)).Distinct();
            }
        }

        protected virtual IEnumerable<ISolution> Match(INode graphName, Triple t)
        {
            if (t.IsGround)
            {
                // Return an empty row for each quad that matches
                return this.Find(graphName, t.Subject, t.Predicate, t.Object).Select(q => new Solution());
            }
            INode s = ToFindItem(t.Subject);
            INode p = ToFindItem(t.Predicate);
            INode o = ToFindItem(t.Object);
            return this.Find(graphName, s, p, o).Select(q => QuadToSet(q, t));
        }

        protected virtual IEnumerable<ISolution> Match(INode graphName, Triple t, ISolution input)
        {
            if (t.IsGround)
            {
                // Return an empty row for each quad that matches
                return this.Find(graphName, t.Subject, t.Predicate, t.Object).Select(q => input);
            }
            INode s = ToFindItem(t.Subject, input);
            INode p = ToFindItem(t.Predicate, input);
            INode o = ToFindItem(t.Object, input);
            return this.Find(graphName, s, p, o).Select(q => QuadToSet(q, t, input));
        }

        protected abstract IEnumerable<Quad> Find(INode g, INode s, INode p, INode o);

        protected static INode ToFindItem(INode n)
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

        protected static INode ToFindItem(INode n, ISolution set)
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

        protected static ISolution QuadToSet(Quad matched, Triple pattern)
        {
            return QuadToSet(matched, pattern, null);
        }

        protected static ISolution QuadToSet(Quad matched, Triple pattern, ISolution set)
        {
            ISolution output = set != null ? new Solution(set) : new Solution();
            String sVar = ToVarName(pattern.Subject);
            if (sVar != null && output[sVar] == null) output.Add(sVar, matched.Subject);
            String pVar = ToVarName(pattern.Predicate);
            if (pVar != null && output[pVar] == null) output.Add(pVar, matched.Predicate);
            String oVar = ToVarName(pattern.Object);
            if (oVar != null && output[oVar] == null) output.Add(oVar, matched.Object);

            return output;
        }

        protected static String ToVarName(INode n)
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