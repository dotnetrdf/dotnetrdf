/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using System.Collections.Generic;
using VDS.Common.Collections;
using VDS.Common.References;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Construct;

/// <summary>
/// Context used for Constructing Triples in SPARQL Query/Update.
/// </summary>
public class ConstructContext
{
    private static readonly ThreadIsolatedReference<NodeFactory> GlobalFactory = new(() => new NodeFactory(new NodeFactoryOptions()));
    private Dictionary<string, INode> _bnodeMap;
    private MultiDictionary<INode, INode> _nodeMap;
    private ISet _set;

    /// <summary>
    /// Creates a new Construct Context.
    /// </summary>
    /// <param name="g">Graph to construct Triples in. May be null.</param>
    /// <param name="preserveBNodes">Whether Blank Nodes bound to variables should be preserved as-is.</param>
    public ConstructContext(IGraph g, bool preserveBNodes)
    {
        Graph = g;
        NodeFactory = Graph as INodeFactory ?? GlobalFactory.Value;
        PreserveBlankNodes = preserveBNodes;
    }

    /// <summary>
    /// Creates a new Construct Context.
    /// </summary>
    /// <param name="factory">Factory to create nodes with. May be null.</param>
    /// <param name="preserveBNodes">Whether Blank Nodes bound to variables should be preserved as-is.</param>
    public ConstructContext(INodeFactory factory, bool preserveBNodes)
    {
        NodeFactory = factory ?? GlobalFactory.Value;
        PreserveBlankNodes = preserveBNodes;
    }

    internal ConstructContext(ISet s, bool preserveBNodes)
    {
        NodeFactory = GlobalFactory.Value;
        Set = s;
        PreserveBlankNodes = preserveBNodes;
    }

    internal ConstructContext(bool preserveBNodes)
    {
        NodeFactory = GlobalFactory.Value;
        PreserveBlankNodes = preserveBNodes;
    }

    /// <summary>
    /// Gets the Set that this Context pertains to.
    /// </summary>
    public ISet Set
    {
        get => _set;
        internal set
        {
            _set = value;
            // Reset the blank node map for each solution
            _bnodeMap?.Clear();
        }
    }

    /// <summary>
    /// Gets the Graph that Triples should be constructed in.
    /// </summary>
    public IGraph Graph { get; }

    private INodeFactory NodeFactory { get; }

    /// <summary>
    /// Gets whether Blank Nodes bound to variables should be preserved.
    /// </summary>
    public bool PreserveBlankNodes { get; }

    /// <summary>
    /// Creates a new Blank Node for this Context.
    /// </summary>
    /// <param name="id">ID.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// If the same Blank Node ID is used multiple times in this Context you will always get the same Blank Node for that ID.
    /// </para>
    /// </remarks>
    public INode GetBlankNode(string id)
    {
        _bnodeMap ??= new Dictionary<string, INode>();

        if (_bnodeMap.TryGetValue(id, out INode node)) return node;

        INode temp;
        if (Graph != null)
        {
            temp = Graph.CreateBlankNode();
        }
        else if (NodeFactory != null)
        {
            temp = NodeFactory.CreateBlankNode();
        }
        else if (Set != null)
        {
            temp = new BlankNode(id.Substring(2) + Set.ID);
        }
        else
        {
            temp = new BlankNode(id.Substring(2));
        }
        _bnodeMap.Add(id, temp);
        return temp;
    }

    /// <summary>
    /// Creates a Node for the Context.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// In effect all this does is ensure that all Nodes end up in the same Graph which may occasionally not happen otherwise when Graph wrappers are involved.
    /// </para>
    /// </remarks>
    public INode GetNode(INode n)
    {
        return GetNode(n, false);
    }

    private INode GetNode(INode n, bool preserveBlankNodes)
    {
        _nodeMap ??= new MultiDictionary<INode, INode>(new FastVirtualNodeComparer());

        if (_nodeMap.TryGetValue(n, out INode node)) return node;

        INode temp;
        switch (n.NodeType)
        {
            case NodeType.Blank:
                temp = preserveBlankNodes ? NodeFactory.CreateBlankNode(((IBlankNode)n).InternalID) :  GetBlankNode(((IBlankNode)n).InternalID);
                break;

            case NodeType.Variable:
                var v = (IVariableNode)n;
                temp = NodeFactory.CreateVariableNode(v.VariableName);
                break;

            case NodeType.GraphLiteral:
                var g = (IGraphLiteralNode)n;
                temp = NodeFactory.CreateGraphLiteralNode(g.SubGraph);
                break;

            case NodeType.Uri:
                var u = (IUriNode)n;
                temp = NodeFactory.CreateUriNode(u.Uri);
                break;

            case NodeType.Literal:
                var l = (ILiteralNode)n;
                if (!l.Language.Equals(string.Empty))
                {
                    temp = NodeFactory.CreateLiteralNode(l.Value, l.Language);
                }
                else if (l.DataType != null)
                {
                    temp = NodeFactory.CreateLiteralNode(l.Value, l.DataType);
                }
                else
                {
                    temp = NodeFactory.CreateLiteralNode(l.Value);
                }
                break;

            case NodeType.Triple:
                var t = (ITripleNode)n;
                INode s = GetNode(t.Triple.Subject, true);
                INode p = GetNode(t.Triple.Predicate, true);
                INode o = GetNode(t.Triple.Object, true);
                var triple = new Triple(s, p, o);
                temp = NodeFactory.CreateTripleNode(triple);
                break;

            default:
                throw new RdfQueryException("Cannot construct unknown Node Types");
        }
        _nodeMap.Add(n, temp);
        return temp;
    }
}
