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
using VDS.RDF.Parsing;

namespace VDS.RDF;

/// <summary>
/// A default implementation of a Node Factory which generates Nodes unrelated to Graphs (wherever possible we suggest using a Graph based implementation instead).
/// </summary>
public class NodeFactory 
    : INodeFactory
{
    private readonly BlankNodeMapper _bnodeMap = new();
    private IUriFactory _uriFactory;

    /// <summary>
    /// Get the namespace map for this node factory.
    /// </summary>
    public INamespaceMapper NamespaceMap { get; }

    /// <summary>
    /// Get or set the base URI used to resolve relative URI references.
    /// </summary>
    /// <remarks>If <see cref="BaseUri"/> is null, attempting to invoke <see cref="CreateUriNode(Uri)"/> with a relative URI or <see cref="CreateUriNode(string)"/> with a QName that resolves to a relative URI will result in a <see cref="RdfException"/> being raised.</remarks>
    public Uri BaseUri { get; set; }

    /// <summary>
    /// Get or set the factory to use when creating URIs.
    /// </summary>
    public IUriFactory UriFactory { 
        get => _uriFactory;
        set
        {
            _uriFactory = value ??
                          throw new ArgumentNullException(nameof(value),
                              "The UriFactory property cannot be set to null.");
        }
    }
    
    /// <summary>
    /// Creates a new Node Factory.
    /// </summary>
    /// <param name="baseUri">The initial base URI to use for the resolution of relative URI references. Defaults to null.</param>
    /// <param name="namespaceMap">The namespace map to use for the resolution of QNames. If not specified, a default <see cref="NamespaceMapper"/> instance will be created.</param>
    /// <param name="normalizeLiteralValues">Whether or not to normalize the value strings of literal nodes.</param>
    /// <param name="uriFactory">The factory to use to create URIs. Defaults to <see cref="VDS.RDF.UriFactory.Root">the root UriFactory instance</see>.</param>
    [Obsolete("Replaced by NodeFactory(NodeFactoryOptions, INamespaceMapper, IUriFactory)")]
    public NodeFactory(Uri baseUri = null, INamespaceMapper namespaceMap = null, bool normalizeLiteralValues = false, IUriFactory uriFactory = null)
    {
        BaseUri = baseUri;
        NamespaceMap = namespaceMap ?? new NamespaceMapper();
        NormalizeLiteralValues = normalizeLiteralValues;
        UriFactory = uriFactory ?? VDS.RDF.UriFactory.Root;
    }

    /// <summary>
    /// Creates a new Node Factory with default settings.
    /// </summary>
    public NodeFactory(): this(new NodeFactoryOptions()) {}

    /// <summary>
    /// Creates a new Node Factory.
    /// </summary>
    /// <param name="options">Configuration options for this node factory.</param>
    /// <param name="namespaceMap">The namespace map to use for the resolution of QNames. If not specified, a default <see cref="NamespaceMapper"/> instance will be created.</param>
    /// <param name="uriFactory">The factory to use to create URIs. Defaults to <see cref="VDS.RDF.UriFactory.Root">the root UriFactory instance</see>.</param>
    public NodeFactory(NodeFactoryOptions options, INamespaceMapper namespaceMap = null,
        IUriFactory uriFactory = null)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        BaseUri = options.BaseUri;
        NormalizeLiteralValues = options.NormalizeLiteralValues;
#pragma warning disable CS0618 // Type or member is obsolete
        LanguageTagValidation = !options.ValidateLanguageSpecifiers ? LanguageTagValidationMode.None: options.LanguageTagValidation;
#pragma warning restore CS0618 // Type or member is obsolete
        NamespaceMap = namespaceMap ?? new NamespaceMapper();
        UriFactory = uriFactory ?? RDF.UriFactory.Root;
    }

    #region INodeFactory Members

    /// <inheritdoc />
    public bool NormalizeLiteralValues { get; set; }

    /// <summary>
    /// Get or set the flag that controls whether or not language tags are validated when creating language-tagged literal nodes.
    /// </summary>
    /// <remarks>
    /// This property is now deprecated and replaced by <see cref="LanguageTagValidation"/>.
    /// Setting this property to 'true' will set <see cref="LanguageTagValidation"/> to <see cref="LanguageTagValidationMode.Turtle"/>.
    /// Setting this property to 'false' will set <see cref="LanguageTagValidation"/> to <see cref="LanguageTagValidationMode.None"/>.
    /// </remarks>
    [Obsolete("Use LanguageTagValidation to set the validation mode.")]
    public bool ValidateLanguageSpecifiers
    {
        get => LanguageTagValidation != LanguageTagValidationMode.None;
        set => LanguageTagValidation =
            value ? LanguageTagValidationMode.Turtle : LanguageTagValidationMode.None;
    }

    /// <summary>
    /// Flush any manually assigned blank node ids to start a new context.
    /// This will force any future creation of a blank node with the same blank node id to map the id to a new
    /// auto-assigned id.
    /// </summary>
    public void FlushBlankNodeAssignments()
    {
        _bnodeMap.FlushBlankNodeAssignments();
    }
    
    /// <summary>
    /// Get or set the type of validation to apply to language tags when creating language-tagged literal nodes.
    /// </summary>
    public LanguageTagValidationMode LanguageTagValidation { get; set; } = LanguageTagValidationMode.WellFormed;

    /// <summary>
    /// Resolve a QName to a URI using this factory's <see cref="INodeFactory.NamespaceMap"/> and <see cref="INodeFactory.BaseUri"/>.
    /// </summary>
    /// <param name="qName"></param>
    /// <returns></returns>
    public Uri ResolveQName(string qName)
    {
        return UriFactory.Create(Tools.ResolveQName(qName, NamespaceMap, BaseUri));
    }

    /// <summary>
    /// Creates a Blank Node with a new automatically generated ID.
    /// </summary>
    /// <returns></returns>
    public IBlankNode CreateBlankNode()
    {
        return new BlankNode(this);
    }

    /// <summary>
    /// Creates a Blank Node with the given Node ID.
    /// </summary>
    /// <param name="nodeId">Node ID.</param>
    /// <returns></returns>
    public IBlankNode CreateBlankNode(string nodeId)
    {
        _bnodeMap.CheckID(ref nodeId);
        return new BlankNode(nodeId);
    }

    /// <summary>
    /// Creates a Graph Literal Node which represents the empty Sub-graph.
    /// </summary>
    /// <returns></returns>
    public IGraphLiteralNode CreateGraphLiteralNode()
    {
        return new GraphLiteralNode();
    }

    /// <summary>
    /// Creates a Graph Literal Node which represents the given Sub-graph.
    /// </summary>
    /// <param name="subgraph">Subgraph.</param>
    /// <returns></returns>
    public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
    {
        return new GraphLiteralNode(subgraph);
    }

    /// <summary>
    /// Creates a Literal Node with the given Value and Data Type.
    /// </summary>
    /// <param name="literal">Value of the Literal.</param>
    /// <param name="datatype">Data Type URI of the Literal.</param>
    /// <returns></returns>
    public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
    {
        return new LiteralNode(literal, datatype, NormalizeLiteralValues);
    }

    /// <summary>
    /// Creates a Literal Node with the given Value.
    /// </summary>
    /// <param name="literal">Value of the Literal.</param>
    /// <returns></returns>
    public ILiteralNode CreateLiteralNode(string literal)
    {
        return new LiteralNode(literal, NormalizeLiteralValues);
    }

    /// <summary>
    /// Creates a Literal Node with the given Value and Language.
    /// </summary>
    /// <param name="literal">Value of the Literal.</param>
    /// <param name="langSpec">Language Specifier for the Literal.</param>
    /// <returns></returns>
    public ILiteralNode CreateLiteralNode(string literal, string langSpec)
    {
        ValidateLanguageSpecifier(langSpec);
        return new LiteralNode(literal, langSpec, NormalizeLiteralValues);
    }

    /// <summary>
    /// Creates a URI Node for the given URI.
    /// </summary>
    /// <param name="uri">URI.</param>
    /// <returns></returns>
    public IUriNode CreateUriNode(Uri uri)
    {
        if (uri == null) throw new ArgumentNullException(nameof(uri), "URI must not be null.");
        if (uri.IsAbsoluteUri)
        {
            return new UriNode(uri);
        }

        if (BaseUri == null)
        {
            throw new ArgumentException(
                "Cannot create a URI node from a relative URI as there is no base URI currently defined.");
        }

        uri = new Uri(BaseUri, uri);
        return new UriNode(uri);
    }

    /// <summary>
    /// Creates a URI Node for the given QName using the Graphs NamespaceMap to resolve the QName.
    /// </summary>
    /// <param name="qName">QName.</param>
    /// <returns>A new URI node.</returns>
    public IUriNode CreateUriNode(string qName)
    {
        if (qName == null) throw new ArgumentNullException(nameof(qName), "QName must not be null");
        return CreateUriNode(UriFactory.Create(Tools.ResolveQName(qName, NamespaceMap, BaseUri)));
    }

    /// <summary>
    /// Creates a URI Node that corresponds to the current Base URI of the node factory.
    /// </summary>
    /// <returns>A new URI Node.</returns>
    /// <exception cref="RdfException">Raised if <see cref="BaseUri"/> is currently null.</exception>
    public IUriNode CreateUriNode()
    {
        if (BaseUri == null)
        {
            throw new RdfException(
                "Cannot create a URI node from the factory base URI because the base URI is not set.");
        }

        return CreateUriNode(BaseUri);
    }

    /// <summary>
    /// Creates a Variable Node for the given Variable Name.
    /// </summary>
    /// <param name="varName"></param>
    /// <returns></returns>
    public IVariableNode CreateVariableNode(string varName)
    {
        return new VariableNode(varName);
    }

    /// <inheritdoc />
    public ITripleNode CreateTripleNode(Triple triple)
    {
        return new TripleNode(triple);
    }

    /// <summary>
    /// Creates a new unused Blank Node ID and returns it.
    /// </summary>
    /// <returns></returns>
    public string GetNextBlankNodeID()
    {
        return _bnodeMap.GetNextID();
    }

    #endregion

    private void ValidateLanguageSpecifier(string langSpec)
    {
        if (string.IsNullOrEmpty(langSpec)) return;
        switch (LanguageTagValidation)
        {
            case LanguageTagValidationMode.None:
                break;
            case LanguageTagValidationMode.Turtle:
                if (!LanguageTag.IsValidTurtle(langSpec))
                {
                    throw new ArgumentException(
                        $"The provided language specifier '{langSpec}' does not conform to the Turtle 1.1 grammar.",
                        nameof(langSpec));
                }
                break;
            case LanguageTagValidationMode.WellFormed:
                if (!LanguageTag.IsWellFormed(langSpec))
                {
                    throw new ArgumentException(
                        $"The provided language specifier '{langSpec}' is not a well-formed BCP47 language tag.",
                        nameof(langSpec));
                }
                break;
        }
    }
}



/// <summary>
/// A private implementation of a Node Factory which returns mock constants regardless of the inputs.
/// </summary>
/// <remarks>
/// <para>
/// Intended for usage in scenarios where the user of the factory does not care about the values returned, for example it is used internally in the <see cref="VDS.RDF.Parsing.Handlers.CountHandler">CountHandler</see> to speed up processing.
/// </para>
/// </remarks>
class MockNodeFactory
    : INodeFactory
{
    private readonly IBlankNode _bnode = new BlankNode("mock");
    private readonly IGraphLiteralNode _glit = new GraphLiteralNode();
    private readonly ILiteralNode _lit = new LiteralNode("mock", false);
    private readonly IUriNode _uri = new UriNode(RDF.UriFactory.Root.Create("dotnetrdf:mock"));
    private readonly IVariableNode _var = new VariableNode("mock");

    private readonly TripleNode _triple = new(new Triple(
        new UriNode(RDF.UriFactory.Root.Create("urn:s")),
        new UriNode(RDF.UriFactory.Root.Create("urn:p")),
        new UriNode(RDF.UriFactory.Root.Create("urn:o"))));

    #region INodeFactory Members

    public Uri BaseUri { get; set; }

    public INamespaceMapper NamespaceMap { get; } = new NamespaceMapper();

    public IUriFactory UriFactory { get; set; } = new CachingUriFactory(null);

    public IBlankNode CreateBlankNode()
    {
        return _bnode;
    }

    public IBlankNode CreateBlankNode(string nodeId)
    {
        return _bnode;
    }

    public IGraphLiteralNode CreateGraphLiteralNode()
    {
        return _glit;
    }

    public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
    {
        return _glit;
    }

    public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
    {
        return _lit;
    }

    public ILiteralNode CreateLiteralNode(string literal)
    {
        return _lit;
    }

    public ILiteralNode CreateLiteralNode(string literal, string langSpec)
    {
        return _lit;
    }

    public IUriNode CreateUriNode(Uri uri)
    {
        return _uri;
    }

    public IUriNode CreateUriNode(string qName)
    {
        return _uri;
    }

    public IUriNode CreateUriNode()
    {
        throw new NotImplementedException();
    }

    public IVariableNode CreateVariableNode(string varName)
    {
        return _var;
    }

    public ITripleNode CreateTripleNode(Triple triple)
    {
        return _triple;
    }

    public string GetNextBlankNodeID()
    {
        throw new NotImplementedException("Not needed by the MockNodeFactory");
    }

    public bool NormalizeLiteralValues
    {
        get => false;
        set => throw new NotImplementedException("Not needed by the MockNodeFactory");
    }

    public LanguageTagValidationMode LanguageTagValidation
    {
        get => LanguageTagValidationMode.None;
        set => throw new NotImplementedException("Not needed by the MockNodeFactory");
    }

    public Uri ResolveQName(string qName)
    {
        throw new NotImplementedException();
    }

    #endregion
}
