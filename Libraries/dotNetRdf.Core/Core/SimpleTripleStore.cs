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

namespace VDS.RDF;

/// <summary>
/// A minimal implementation of the ITripleStore interface that provides only graph management functionality.
/// </summary>
/// <remarks>More comprehensive implementations that provide additional query and update functionality can be found in the dotNetRDF.InMemory package.</remarks>
public sealed class SimpleTripleStore: BaseTripleStore
{
    /// <summary>
    /// Create a new empty triple store.
    /// </summary>
    public SimpleTripleStore() : this(RDF.UriFactory.Root)
    {
    }

    /// <summary>
    /// Create a new empty triple store.
    /// </summary>
    /// <param name="uriFactory">The factory to use when creating new URIs in the context of this triple store.</param>
    public SimpleTripleStore(IUriFactory uriFactory):base(new GraphCollection())
    {
        UriFactory = uriFactory;
    }

    /// <inheritdoc />
    public override IUriFactory UriFactory { get; }

    /// <inheritdoc />
    public override void Dispose()
    {
        Graphs.Dispose();
    }

}
