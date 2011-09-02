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
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Construct
{
    /// <summary>
    /// Context used for Constructing Triples in SPARQL Query/Update
    /// </summary>
    public class ConstructContext
    {
        private static ThreadIsolatedReference<NodeFactory> _globalFactory = new ThreadIsolatedReference<NodeFactory>(() => new NodeFactory());
        private ISet _s;
        private INodeFactory _factory;
        private IGraph _g;
        private bool _preserveBNodes = false;
        private Dictionary<String, INode> _bnodeMap;
        private Dictionary<INode, INode> _nodeMap;

        /// <summary>
        /// Creates a new Construct Context
        /// </summary>
        /// <param name="g">Graph to construct Triples in</param>
        /// <param name="s">Set to construct from</param>
        /// <param name="preserveBNodes">Whether Blank Nodes bound to variables should be preserved as-is</param>
        /// <remarks>
        /// <para>
        /// Either the <paramref name="s">Set</paramref>  or <paramref name="g">Graph</paramref> parameters may be null if required
        /// </para>
        /// </remarks>
        public ConstructContext(IGraph g, ISet s, bool preserveBNodes)
        {
            this._g = g;
            this._factory = (this._g != null ? (INodeFactory)this._g : _globalFactory.Value);
            this._s = s;
            this._preserveBNodes = preserveBNodes;
        }

        /// <summary>
        /// Creates a new Construct Context
        /// </summary>
        /// <param name="factory">Factory to create nodes with</param>
        /// <param name="s">Set to construct from</param>
        /// <param name="preserveBNodes">Whether Blank Nodes bound to variables should be preserved as-is</param>
        /// <remarks>
        /// <para>
        /// Either the <paramref name="s">Set</paramref>  or <paramref name="factory">Factory</paramref> parameters may be null if required
        /// </para>
        /// </remarks>
        public ConstructContext(INodeFactory factory, ISet s, bool preserveBNodes)
        {
            this._factory = (factory != null ? factory : _globalFactory.Value);
            this._s = s;
            this._preserveBNodes = preserveBNodes;
        }

        /// <summary>
        /// Gets the Set that this Context pertains to
        /// </summary>
        public ISet Set
        {
            get
            {
                return this._s;
            }
        }

        /// <summary>
        /// Gets the Graph that Triples should be constructed in
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return this._g;
            }
        }

        private INodeFactory NodeFactory
        {
            get
            {
                return this._factory;
            }
        }

        /// <summary>
        /// Gets whether Blank Nodes bound to variables should be preserved
        /// </summary>
        public bool PreserveBlankNodes
        {
            get
            {
                return this._preserveBNodes;
            }
        }

        /// <summary>
        /// Creates a new Blank Node for this Context
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// If the same Blank Node ID is used multiple times in this Context you will always get the same Blank Node for that ID
        /// </para>
        /// </remarks>
        public INode GetBlankNode(String id)
        {
            if (this._bnodeMap == null) this._bnodeMap = new Dictionary<string, INode>();

            if (this._bnodeMap.ContainsKey(id)) return this._bnodeMap[id];

            INode temp;
            if (this._g != null)
            {
                temp = this._g.CreateBlankNode();
            }
            else if (this._factory != null)
            {
                temp = this._factory.CreateBlankNode();
            }
            else if (this._s != null)
            {
                temp = new BlankNode(this._g, id.Substring(2) + "-" + this._s.ID);
            }
            else
            {
                temp = new BlankNode(this._g, id.Substring(2));
            }
            this._bnodeMap.Add(id, temp);
            return temp;
        }

        /// <summary>
        /// Creates a Node for the Context
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// In effect all this does is ensure that all Nodes end up in the same Graph which may occassionally not happen otherwise when Graph wrappers are involved
        /// </para>
        /// </remarks>
        public INode GetNode(INode n)
        {
            if (this._nodeMap == null) this._nodeMap = new Dictionary<INode,INode>();

            if (this._nodeMap.ContainsKey(n)) return this._nodeMap[n];

            INode temp;
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    temp = this.GetBlankNode(((IBlankNode)n).InternalID);
                    break;

                case NodeType.Variable:
                    IVariableNode v = (IVariableNode)n;
                    temp = this._factory.CreateVariableNode(v.VariableName);
                    break;

                case NodeType.GraphLiteral:
                    IGraphLiteralNode g = (IGraphLiteralNode)n;
                    temp = this._factory.CreateGraphLiteralNode(g.SubGraph);
                    break;

                case NodeType.Uri:
                    IUriNode u = (IUriNode)n;
                    temp = this._factory.CreateUriNode(u.Uri);
                    break;

                case NodeType.Literal:
                    ILiteralNode l = (ILiteralNode)n;
                    if (l.DataType != null)
                    {
                        temp = this._factory.CreateLiteralNode(l.Value, l.DataType);
                    } 
                    else if (!l.Language.Equals(String.Empty))
                    {
                        temp = this._factory.CreateLiteralNode(l.Value, l.Language);
                    } 
                    else
                    {
                        temp = this._factory.CreateLiteralNode(l.Value);
                    }
                    break;
                
                default:
                    throw new RdfQueryException("Cannot construct unknown Node Types");
            }
            this._nodeMap.Add(n, temp);
            return temp;
        }
    }
}
