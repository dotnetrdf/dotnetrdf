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
}
