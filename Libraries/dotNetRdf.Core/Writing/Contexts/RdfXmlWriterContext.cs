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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing.Contexts;

/// <summary>
/// Writer Context for RDF/XML Writers.
/// </summary>
public class RdfXmlWriterContext 
    : IWriterContext, ICollectionCompressingWriterContext
{
    private readonly TripleCollection _triplesDone = new TripleCollection();

    /// <summary>
    /// Creates a new RDF/XML Writer Context.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="output">Output destination.</param>
    public RdfXmlWriterContext(IGraph g, TextWriter output)
    {
        Graph = g;
        Output = output;
        Writer = XmlWriter.Create(Output, GetSettings(output.Encoding));
        NamespaceMap.Import(Graph.NamespaceMap);
        UriFactory = g.UriFactory;
    }

    /// <summary>
    /// Generates the required settings for the <see cref="XmlWriter">XmlWriter</see>.
    /// </summary>
    /// <returns></returns>
    private XmlWriterSettings GetSettings(Encoding fileEncoding)
    {
        return new XmlWriterSettings
        {
            ConformanceLevel = ConformanceLevel.Document,
            CloseOutput = true,
            Encoding = fileEncoding,
            Indent = PrettyPrint,
            NewLineHandling = NewLineHandling.None,
            OmitXmlDeclaration = false,
        };
    }

    /// <summary>
    /// Gets the Graph being written.
    /// </summary>
    public IGraph Graph { get; }

    /// <summary>
    /// Gets the TextWriter being written to.
    /// </summary>
    public TextWriter Output { get; }

    /// <summary>
    /// Gets the XML Writer in use.
    /// </summary>
    public XmlWriter Writer { get; }

    /// <summary>
    /// Gets/Sets the Pretty Printing Mode used.
    /// </summary>
    public bool PrettyPrint { get; set; } = true;

    /// <summary>
    /// Gets/Sets the Node Formatter.
    /// </summary>
    /// <remarks>
    /// Node Formatters are not used for RDF/XML output.
    /// </remarks>
    public INodeFormatter NodeFormatter
    {
        get => null;
        set => throw new NotSupportedException("Node Formatters are not used for RDF/XML output");
    }

    /// <summary>
    /// Gets/Sets the URI Formatter.
    /// </summary>
    /// <remarks>
    /// URI Formatters are not used for RDF/XML output.
    /// </remarks>
    public IUriFormatter UriFormatter
    {
        get => null;
        set => throw new NotSupportedException("URI Formatters are not used for RDF/XML output");
    }

    /// <summary>
    /// Gets/sets the URI factory used.
    /// </summary>
    public IUriFactory UriFactory { get; set; }

    /// <summary>
    /// Gets the Namespace Map in use.
    /// </summary>
    public NestedNamespaceMapper NamespaceMap { get; } = new NestedNamespaceMapper(true);

    /// <summary>
    /// Gets the Blank Node map in use.
    /// </summary>
    public BlankNodeOutputMapper BlankNodeMapper { get; } = new BlankNodeOutputMapper(XmlSpecsHelper.IsName);

    /// <summary>
    /// Gets/Sets whether High Speed Mode is permitted.
    /// </summary>
    /// <remarks>
    /// Not currently supported.
    /// </remarks>
    public bool HighSpeedModePermitted
    {
        get => false;
        set
        {
            // Do Nothing
        }
    }

    /// <summary>
    /// Gets/Sets the Compression Level used.
    /// </summary>
    /// <remarks>
    /// Not currently supported.
    /// </remarks>
    public int CompressionLevel { get; set; } = WriterCompressionLevel.Default;

    /// <summary>
    /// Gets/Sets the next ID to use for issuing Temporary Namespaces.
    /// </summary>
    public int NextNamespaceID { get; set; } = 0;

    /// <summary>
    /// Gets/Sets whether a DTD is used.
    /// </summary>
    public bool UseDtd { get; set; } = true;

    /// <summary>
    /// Gets/Sets whether attributes are used to encode the predicates and objects of triples with simple literal properties.
    /// </summary>
    public bool UseAttributes { get; set; } = true;

    /// <summary>
    /// Represents the mapping from Blank Nodes to Collections.
    /// </summary>
    public Dictionary<INode, OutputRdfCollection> Collections { get; } = new Dictionary<INode, OutputRdfCollection>();

    /// <summary>
    /// Stores the Triples that should be excluded from standard output as they are part of collections.
    /// </summary>
    public BaseTripleCollection TriplesDone => _triplesDone;
}
