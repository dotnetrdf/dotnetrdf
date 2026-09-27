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
/// Base class for RDF readers that provides common loading functionality for graphs, triple stores, and RDF handlers.
/// </summary>
public abstract class BaseRdfParser : IRdfReader
{
    /// <inheritdoc/>
    public event RdfReaderWarning Warning;

    /// <inheritdoc/>
    public virtual void Load(IGraph g, StreamReader input)
    {
        if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
        if (input == null) throw new RdfParseException("Cannot read RDF from a null Input");
        Load(new GraphHandler(g), input);
    }

    /// <inheritdoc/>
    public virtual void Load(IGraph g, TextReader input)
    {
        if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
        if (input == null) throw new RdfParseException("Cannot read RDF from a null Input");
        Load(new GraphHandler(g), input);
    }

    /// <inheritdoc/>
    public virtual void Load(IGraph g, string filename)
    {
        if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
        if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
        using var reader = new StreamReader(File.OpenRead(filename));
        Load(new GraphHandler(g), reader);
    }

    /// <inheritdoc/>
    public virtual void Load(ITripleStore store, StreamReader input)
    {
        if (store == null) throw new RdfParseException("Cannot read RDF into a null Triple Store");
        if (input == null) throw new RdfParseException("Cannot read RDF from a null Input");
        Load(new TripleStoreHandler(store), input);
    }

    /// <inheritdoc/>
    public virtual void Load(ITripleStore store, TextReader input)
    {
        if (store == null) throw new RdfParseException("Cannot read RDF into a null Triple Store");
        if (input == null) throw new RdfParseException("Cannot read RDF from a null Input");
        Load(new TripleStoreHandler(store), input);
    }

    /// <inheritdoc/>
    public virtual void Load(ITripleStore store, string filename)
    {
        if (store == null) throw new RdfParseException("Cannot read RDF into a null Triple Store");
        if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
        Load(new TripleStoreHandler(store), filename);
    }

    /// <inheritdoc/>
    public virtual void Load(IRdfHandler handler, StreamReader input)
    {
        if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
        if (input == null) throw new RdfParseException("Cannot read RDF from a null Input");
        Load(handler, input, UriFactory.Root);
    }

    /// <inheritdoc/>
    public virtual void Load(IRdfHandler handler, TextReader input)
    {
        if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
        if (input == null) throw new RdfParseException("Cannot read RDF from a null Input");
        Load(handler, input, UriFactory.Root);
    }

    /// <inheritdoc/>
    public virtual void Load(IRdfHandler handler, string filename)
    {
        if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
        if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
        Load(handler, filename, UriFactory.Root);
    }

    protected virtual void RaiseWarning(string message)
    {
        Warning?.Invoke(message);
    }

    /// <inheritdoc/>
    public abstract void Load(IRdfHandler handler, StreamReader input, IUriFactory uriFactory);

    /// <inheritdoc/>
    public abstract void Load(IRdfHandler handler, TextReader input, IUriFactory uriFactory);

    /// <inheritdoc/>
    public abstract void Load(IRdfHandler handler, string filename, IUriFactory uriFactory);
}