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

// unset

using System.IO;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing;

/// <summary>
/// Abstract base class for SPARQL result readers.
/// </summary>
/// <remarks>This implementation just implements the defaulting of the old interface methods to the new extended methods that take an <see cref="IUriFactory"/> parameter.</remarks>
public abstract class BaseSparqlResultsReader : ISparqlResultsReader
{
    /// <inheritdoc />
    public void Load(SparqlResultSet results, StreamReader input)
    {
        Load(results, input, UriFactory.Root);
    }

    /// <inheritdoc />
    public void Load(SparqlResultSet results, string filename)
    {
        Load(results, filename, UriFactory.Root);
    }

    /// <inheritdoc />
    public void Load(SparqlResultSet results, TextReader input)
    {
        Load(results, input, UriFactory.Root);
    }

    /// <inheritdoc />
    public void Load(ISparqlResultsHandler handler, StreamReader input)
    {
        Load(handler, input, UriFactory.Root);
    }

    /// <inheritdoc />
    public void Load(ISparqlResultsHandler handler, string filename)
    {
        Load(handler, filename, UriFactory.Root);
    }

    /// <inheritdoc />
    public void Load(ISparqlResultsHandler handler, TextReader input)
    {
        Load(handler, input, UriFactory.Root);
    }

    /// <inheritdoc />
    public abstract void Load(SparqlResultSet results, StreamReader input, IUriFactory uriFactory);

    /// <inheritdoc />
    public abstract void Load(SparqlResultSet results, string filename, IUriFactory uriFactory);

    /// <inheritdoc />
    public abstract void Load(SparqlResultSet results, TextReader input, IUriFactory uriFactory);

    /// <inheritdoc />
    public abstract void Load(ISparqlResultsHandler handler, StreamReader input, IUriFactory uriFactory);

    /// <inheritdoc />
    public abstract void Load(ISparqlResultsHandler handler, string filename, IUriFactory uriFactory);

    /// <inheritdoc />
    public abstract void Load(ISparqlResultsHandler handler, TextReader input, IUriFactory uriFactory);

    /// <inheritdoc />
    public abstract event SparqlWarning Warning;
}