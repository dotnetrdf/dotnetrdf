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

using System;

namespace VDS.RDF;

/// <summary>
/// Interface for classes which can create Nodes.
/// </summary>
public interface INodeFactory
{
    /// <summary>
    /// Get or set the base URI used to resolve relative URI references.
    /// </summary>
    public Uri BaseUri { get; set; }

    /// <summary>
    /// Get the namespace map for this node factory.
    /// </summary>
    public INamespaceMapper NamespaceMap { get; }

    /// <summary>
    /// Get or set the factory to use when creating URIs.
    /// </summary>
    public IUriFactory UriFactory { get; set; }

    /// <summary>
    /// Creates a Blank Node with a new automatically generated ID.
    /// </summary>
    /// <returns></returns>
    IBlankNode CreateBlankNode();

    /// <summary>
    /// Creates a Blank Node with the given Node ID.
    /// </summary>
    /// <param name="nodeId">Node ID.</param>
    /// <returns></returns>
    IBlankNode CreateBlankNode(string nodeId);

    /// <summary>
    /// Creates a Graph Literal Node which represents the empty Subgraph.
    /// </summary>
    /// <returns></returns>
    IGraphLiteralNode CreateGraphLiteralNode();

    /// <summary>
    /// Creates a Graph Literal Node which represents the given Subgraph.
    /// </summary>
    /// <param name="subgraph">Subgraph.</param>
    /// <returns></returns>
    IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph);

    /// <summary>
    /// Creates a Literal Node with the given Value and Data Type.
    /// </summary>
    /// <param name="literal">Value of the Literal.</param>
    /// <param name="datatype">Data Type URI of the Literal.</param>
    /// <returns></returns>
    ILiteralNode CreateLiteralNode(string literal, Uri datatype);

    /// <summary>
    /// Creates a Literal Node with the given Value.
    /// </summary>
    /// <param name="literal">Value of the Literal.</param>
    /// <returns></returns>
    ILiteralNode CreateLiteralNode(string literal);

    /// <summary>
    /// Creates a Literal Node with the given Value and Language.
    /// </summary>
    /// <param name="literal">Value of the Literal.</param>
    /// <param name="langSpec">Language Specifier for the Literal.</param>
    /// <returns></returns>
    ILiteralNode CreateLiteralNode(string literal, string langSpec);

    /// <summary>
    /// Creates a URI Node for the given URI.
    /// </summary>
    /// <param name="uri">URI.</param>
    /// <returns></returns>
    IUriNode CreateUriNode(Uri uri);

    /// <summary>
    /// Creates a URI Node for the given QName using the Graphs NamespaceMap to resolve the QName.
    /// </summary>
    /// <param name="qName">QName.</param>
    /// <returns></returns>
    IUriNode CreateUriNode(string qName);

    /// <summary>
    /// Creates a URI Node that corresponds to the current Base URI of the node factory.
    /// </summary>
    /// <returns></returns>
    IUriNode CreateUriNode();


    /// <summary>
    /// Creates a Variable Node for the given Variable Name.
    /// </summary>
    /// <param name="varName"></param>
    /// <returns></returns>
    IVariableNode CreateVariableNode(string varName);

    /// <summary>
    /// Creates a node that quotes the given triple.
    /// </summary>
    /// <param name="triple">The triple to be the quoted value of the created node.</param>
    /// <returns></returns>
    ITripleNode CreateTripleNode(Triple triple);

    /// <summary>
    /// Creates a new unused Blank Node ID and returns it.
    /// </summary>
    /// <returns></returns>
    string GetNextBlankNodeID();

    /// <summary>
    /// Get or set the flag that controls whether the value strings of literal nodes should be normalized on creation.
    /// </summary>
    bool NormalizeLiteralValues { get; set; }

    /// <summary>
    /// Get or set the type of validation to apply to language tags when creating language-tagged literal nodes.
    /// </summary>
    LanguageTagValidationMode LanguageTagValidation { get; set; }

    /// <summary>
    /// Resolve a QName to a URI using this factory's <see cref="NamespaceMap"/> and <see cref="BaseUri"/>.
    /// </summary>
    /// <param name="qName"></param>
    /// <returns></returns>
    Uri ResolveQName(string qName);
}
