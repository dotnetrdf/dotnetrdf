/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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

using System.IO;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing;

/// <summary>
/// An adapter class that presents a <see cref="IStoreReader"/> that loads quads into an <see cref="ITripleStore"/> as a
/// <see cref="IRdfReader"/> that can load triples into a <see cref="Graph"/>. When using the methods of
/// <see cref="IRdfReader"/> that take an <see cref="IGraph"/> argument, the adapter will use the <see cref="GraphHandler"/>
/// implementation which populates the graph with all triples from all graphs read by the underlying <see cref="IStoreReader"/>.
/// </summary>
public class StoreReaderAdapter : IRdfReader
{
    private readonly IStoreReader _storeReader;

    /// <summary>
    /// Create an adapter that presents the specified <see cref="IStoreReader"/> instance as an <see cref="IRdfReader"/>.
    /// </summary>
    /// <param name="storeReader">The store reader to be wrapped.</param>
    public StoreReaderAdapter(IStoreReader storeReader)
    {
        _storeReader = storeReader;
        _storeReader.Warning += StoreReaderOnWarning;
    }

    private void StoreReaderOnWarning(string message)
    {
        Warning?.Invoke(message);
    }

    /// <summary>
    /// Method for Loading a Graph from some Concrete RDF Syntax via some arbitrary Stream.
    /// </summary>
    /// <param name="g">Graph to load RDF into.</param>
    /// <param name="input">The reader to read input from.</param>
    /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF.</exception>
    /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input.</exception>
    /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream.</exception>
    /// <remarks>When loading from a quad format, the graph will receive a merge of the triples defined in all graphs in the source data.</remarks>
    public void Load(IGraph g, StreamReader input)
    {
        _storeReader.Load(new GraphHandler(g), input);
    }

    /// <summary>
    /// Method for Loading a Graph from some Concrete RDF Syntax via some arbitrary Input.
    /// </summary>
    /// <param name="g">Graph to load RDF into.</param>
    /// <param name="input">The reader to read input from.</param>
    /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF.</exception>
    /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input.</exception>
    /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream.</exception>
    /// <remarks>When loading from a quad format, the graph will receive a merge of the triples defined in all graphs in the source data.</remarks>
    public void Load(IGraph g, TextReader input)
    {
        _storeReader.Load(new GraphHandler(g), input);
    }

    /// <summary>
    /// Method for Loading a Graph from some Concrete RDF Syntax from a given File.
    /// </summary>
    /// <param name="g">Graph to load RDF into.</param>
    /// <param name="filename">The Filename of the File to read from.</param>
    /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF.</exception>
    /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input.</exception>
    /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the File.</exception>
    /// <remarks>When loading from a quad format, the graph will receive a merge of the triples defined in all graphs in the source data.</remarks>
    public void Load(IGraph g, string filename)
    {
        _storeReader.Load(new GraphHandler(g), filename);
    }

    /// <inheritdoc />
    public void Load(IRdfHandler handler, StreamReader input)
    {
        _storeReader.Load(handler, input);
    }

    /// <inheritdoc />
    public void Load(IRdfHandler handler, TextReader input)
    {
        _storeReader.Load(handler, input);
    }

    /// <inheritdoc />
    public void Load(IRdfHandler handler, string filename)
    {
        _storeReader.Load(handler, filename);
    }

    /// <inheritdoc />
    public void Load(IRdfHandler handler, StreamReader input, IUriFactory uriFactory)
    {
        _storeReader.Load(handler, input, uriFactory);
    }

    /// <inheritdoc />
    public void Load(IRdfHandler handler, TextReader input, IUriFactory uriFactory)
    {
        _storeReader.Load(handler, input, uriFactory);
    }

    /// <inheritdoc />
    public void Load(IRdfHandler handler, string filename, IUriFactory uriFactory)
    {
        _storeReader.Load(handler, filename, uriFactory);
    }

    /// <inheritdoc />
    public event RdfReaderWarning Warning;
}