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
/// Class for representing Triple Stores which are collections of RDF Graphs.
/// </summary>
/// <remarks>
/// The 'Web Demand' Triple Store is a Triple Store which automatically retrieves Graphs from the Web based on the URIs of Graphs that you ask it for.
/// </remarks>
public class WebDemandTripleStore 
    : TripleStore
{
    /// <summary>
    /// Creates a web demand triple store.
    /// </summary>
    /// <param name="defaultGraphName">The name of the default graph which should be loaded from the web. The graph name MUST be a non-null <see cref="IUriNode"/> instance.</param>
    /// <exception cref="RdfException">Raised if the graph referenced by <paramref name="defaultGraphName"/> could not be loaded from the web.</exception>
    public WebDemandTripleStore(IUriNode defaultGraphName) : this()
    {
        if (defaultGraphName == null)
        {
            throw new ArgumentNullException(nameof(defaultGraphName),
                "The default graph URI reference must not be null.");
        }

        if (!_graphs.Contains(defaultGraphName))
        {
            throw new RdfException($"Cannot load the requested default graph from {defaultGraphName.Uri}");
        }
    }

    /// <summary>
    /// Creates an Web Demand Triple Store.
    /// </summary>
    /// <param name="defaultGraphUri">A Uri for the Default Graph which should be loaded from the Web as the initial Graph.</param>
    /// <exception cref="RdfException">Raised if the graph referenced by <paramref name="defaultGraphUri"/> could not be loaded from the web.</exception>
    public WebDemandTripleStore(Uri defaultGraphUri)
        : this()
    {
        if (defaultGraphUri == null)
        {
            throw new ArgumentNullException(nameof(defaultGraphUri),
                "The default graph URI reference must not be null.");
        }
        // Call Contains() which will try to load the Graph if it exists in the Store
        if (!_graphs.Contains(new UriNode(defaultGraphUri)))
        {
            throw new RdfException("Cannot load the requested Default Graph since a valid Graph with that URI could not be retrieved from the Web");
        }
    }

    /// <summary>
    /// Creates an Web Demand Triple Store.
    /// </summary>
    /// <param name="defaultGraphFile">A Filename for the Default Graph which should be loaded from a local File as the initial Graph.</param>
    public WebDemandTripleStore(string defaultGraphFile)
        : this()
    {
        try
        {
            var g = new Graph();
            FileLoader.Load(g, defaultGraphFile);
            _graphs.Add(g, false);
        }
        catch (Exception)
        {
            throw new RdfException("Cannot load the requested Default Graph since a valid Graph could not be retrieved from the given File");
        }
    }

    /// <summary>
    /// Creates a new Web Demand Triple Store.
    /// </summary>
    public WebDemandTripleStore()
        : base(new WebDemandGraphCollection()) { }
}
