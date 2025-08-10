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
using System.IO;
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing.Contexts;

/// <summary>
/// Writer Context for Compressing Turtle Writers.
/// </summary>
public class CompressingTurtleWriterContext 
    : TurtleWriterContext, ICollectionCompressingWriterContext, IAnnotationCompressingWriterContext
{
    private readonly TripleCollection _triplesDone = new TripleCollection();

    /// <summary>
    /// Creates a new Turtle Writer Context with default settings.
    /// </summary>
    /// <param name="g">Graph to write.</param>
    /// <param name="output">TextWriter to write to.</param>
    public CompressingTurtleWriterContext(IGraph g, TextWriter output)
        : this(g, output, TurtleSyntax.Original) { }

    /// <summary>
    /// Creates a new Turtle Writer Context with default settings.
    /// </summary>
    /// <param name="g">Graph to write.</param>
    /// <param name="output">TextWriter to write to.</param>
    /// <param name="syntax">Turtle Syntax.</param>
    public CompressingTurtleWriterContext(IGraph g, TextWriter output, TurtleSyntax syntax)
        : this(g, output, WriterCompressionLevel.More, true, true, syntax) { }

    /// <summary>
    /// Creates a new Turtle Writer Context with custom settings.
    /// </summary>
    /// <param name="g">Graph to write.</param>
    /// <param name="output">TextWriter to write to.</param>
    /// <param name="prettyPrint">Pretty Print Mode.</param>
    /// <param name="hiSpeed">High Speed Mode.</param>
    public CompressingTurtleWriterContext(IGraph g, TextWriter output, bool prettyPrint, bool hiSpeed)
        : this(g, output, WriterCompressionLevel.More, prettyPrint, hiSpeed, TurtleSyntax.Original) { }

    
    /// <summary>
    /// Creates a new Turtle Writer Context with custom settings.
    /// </summary>
    /// <param name="g">Graph to write.</param>
    /// <param name="output">TextWriter to write to.</param>
    /// <param name="prettyPrint">Pretty Print Mode.</param>
    /// <param name="hiSpeed">High Speed Mode.</param>
    /// <param name="syntax">Turtle Syntax.</param>
    public CompressingTurtleWriterContext(IGraph g, TextWriter output, bool prettyPrint, bool hiSpeed, TurtleSyntax syntax)
        : this(g, output, WriterCompressionLevel.More, true, true, syntax) { }

    /// <summary>
    /// Creates a new Turtle Writer Context with custom settings.
    /// </summary>
    /// <param name="g">Graph to write.</param>
    /// <param name="output">TextWriter to write to.</param>
    /// <param name="compressionLevel">Compression Level to use.</param>
    /// <param name="prettyPrint">Pretty Print Mode.</param>
    /// <param name="hiSpeed">High Speed Mode.</param>
    public CompressingTurtleWriterContext(IGraph g, TextWriter output, int compressionLevel, bool prettyPrint, bool hiSpeed)
        : this(g, output, compressionLevel, prettyPrint, hiSpeed, TurtleSyntax.Original) { }

    /// <summary>
    /// Creates a new Turtle Writer Context with custom settings.
    /// </summary>
    /// <param name="g">Graph to write.</param>
    /// <param name="output">TextWriter to write to.</param>
    /// <param name="compressionLevel">Compression Level to use.</param>
    /// <param name="prettyPrint">Pretty Print Mode.</param>
    /// <param name="hiSpeed">High Speed Mode.</param>
    /// <param name="syntax">Turtle Syntax.</param>
    public CompressingTurtleWriterContext(IGraph g, TextWriter output, int compressionLevel, bool prettyPrint, bool hiSpeed, TurtleSyntax syntax)
        : base(g, output, compressionLevel, prettyPrint, hiSpeed, syntax) { }

    /// <summary>
    /// Represents the mapping from Blank Nodes to Collections.
    /// </summary>
    public Dictionary<INode, OutputRdfCollection> Collections { get; } = new Dictionary<INode, OutputRdfCollection>();

    /// <inheritdoc />
    public Dictionary<Triple, List<Triple>> Annotations { get; } = new Dictionary<Triple, List<Triple>>();

    /// <summary>
    /// Stores the Triples that should be excluded from standard output as they are part of collections or triple annotations.
    /// </summary>
    public BaseTripleCollection TriplesDone => _triplesDone;
}