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

namespace VDS.RDF
{
    /// <summary>
    /// Interface for classes which can create Nodes
    /// </summary>
    public interface INodeFactory
    {
        /// <summary>
        /// Creates a Blank Node with a new automatically generated ID
        /// </summary>
        /// <returns></returns>
        IBlankNode CreateBlankNode();

        /// <summary>
        /// Creates a Blank Node with the given Node ID
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns></returns>
        IBlankNode CreateBlankNode(string nodeId);

        /// <summary>
        /// Creates a Graph Literal Node which represents the empty Subgraph
        /// </summary>
        /// <returns></returns>
        IGraphLiteralNode CreateGraphLiteralNode();

        /// <summary>
        /// Creates a Graph Literal Node which represents the given Subgraph
        /// </summary>
        /// <param name="subgraph">Subgraph</param>
        /// <returns></returns>
        IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph);

        /// <summary>
        /// Creates a Literal Node with the given Value and Data Type
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="datatype">Data Type URI of the Literal</param>
        /// <returns></returns>
        ILiteralNode CreateLiteralNode(string literal, Uri datatype);

        /// <summary>
        /// Creates a Literal Node with the given Value
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <returns></returns>
        ILiteralNode CreateLiteralNode(string literal);

        /// <summary>
        /// Creates a Literal Node with the given Value and Language
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="langspec">Language Specifier for the Literal</param>
        /// <returns></returns>
        ILiteralNode CreateLiteralNode(string literal, string langspec);

        /// <summary>
        /// Creates a URI Node for the given URI
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns></returns>
        IUriNode CreateUriNode(Uri uri);

        /// <summary>
        /// Creates a Variable Node for the given Variable Name
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        IVariableNode CreateVariableNode(string varname);

        /// <summary>
        /// Creates a new unused Blank Node ID and returns it
        /// </summary>
        /// <returns></returns>
        String GetNextBlankNodeID();
    }

    /// <summary>
    /// A Default Implementation of a Node Factory which generates Nodes unrelated to Graphs (wherever possible we suggest using a Graph based implementation instead)
    /// </summary>
    public class NodeFactory : INodeFactory
    {
        private BlankNodeMapper _bnodeMap = new BlankNodeMapper();

        /// <summary>
        /// Creates a new Node Factory
        /// </summary>
        public NodeFactory()
        { }

        #region INodeFactory Members

        /// <summary>
        /// Creates a Blank Node with a new automatically generated ID
        /// </summary>
        /// <returns></returns>
        public IBlankNode CreateBlankNode()
        {
            return new BlankNode(this);
        }

        /// <summary>
        /// Creates a Blank Node with the given Node ID
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns></returns>
        public IBlankNode CreateBlankNode(string nodeId)
        {
            this._bnodeMap.CheckID(ref nodeId);
            return new BlankNode(null, nodeId);
        }

        /// <summary>
        /// Creates a Graph Literal Node which represents the empty Subgraph
        /// </summary>
        /// <returns></returns>
        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            return new GraphLiteralNode(null);
        }

        /// <summary>
        /// Creates a Graph Literal Node which represents the given Subgraph
        /// </summary>
        /// <param name="subgraph">Subgraph</param>
        /// <returns></returns>
        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return new GraphLiteralNode(null, subgraph);
        }

        /// <summary>
        /// Creates a Literal Node with the given Value and Data Type
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="datatype">Data Type URI of the Literal</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return new LiteralNode(null, literal, datatype);
        }

        /// <summary>
        /// Creates a Literal Node with the given Value
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal)
        {
            return new LiteralNode(null, literal);
        }

        /// <summary>
        /// Creates a Literal Node with the given Value and Language
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="langspec">Language Specifier for the Literal</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            return new LiteralNode(null, literal, langspec);
        }

        /// <summary>
        /// Creates a URI Node for the given URI
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns></returns>
        public IUriNode CreateUriNode(Uri uri)
        {
            return new UriNode(null, uri);
        }

        /// <summary>
        /// Creates a Variable Node for the given Variable Name
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        public IVariableNode CreateVariableNode(string varname)
        {
            return new VariableNode(null, varname);
        }

        /// <summary>
        /// Creates a new unused Blank Node ID and returns it
        /// </summary>
        /// <returns></returns>
        public string GetNextBlankNodeID()
        {
            return this._bnodeMap.GetNextID();
        }

        #endregion
    }
}
