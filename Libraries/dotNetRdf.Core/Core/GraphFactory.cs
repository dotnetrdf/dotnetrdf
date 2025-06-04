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
/// A Graph Factory provides access to consistent Graph References so that Nodes and Triples can be instantiated in the correct Graphs.
/// </summary>
/// <remarks>
/// <para>
/// Primarily designed for internal use in some of our code but may prove useful to other users hence is a public class.  Internally this is just a wrapper around a <see cref="TripleStore">TripleStore</see> instance.
/// </para>
/// <para>
/// The main usage for this class is scenarios where consistent graph references matter such as returning node references from out of memory datasets (like SQL backed ones) particularly with regards to blank nodes since blank node equality is predicated upon Graph reference.
/// </para>
/// </remarks>
[Obsolete("This class is obsolete and will be removed in a future release. There is no replacement for this class.")]
public class GraphFactory
{
    private TripleStore _store = new TripleStore();

    /// <summary>
    /// Gets a Graph Reference for the given Graph URI.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    public IGraph this[Uri graphUri]
    {
        get
        {
            if (_store.HasGraph(graphUri))
            {
                return _store[graphUri];
            }
            else
            {
                var g = new Graph();
                g.BaseUri = graphUri;
                _store.Add(g);
                return g;
            }
        }
    }

    /// <summary>
    /// Gets a Graph Reference for the given Graph URI.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    /// <remarks>
    /// Synonym for the index access method i.e. factory[graphUri].
    /// </remarks>
    public IGraph GetGraph(Uri graphUri)
    {
        return this[graphUri];
    }

    /// <summary>
    /// Gets a Graph Reference for the given Graph URI and indicates whether this was a new Graph reference.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <param name="created">Indicates whether the returned reference was newly created.</param>
    /// <returns></returns>
    public IGraph TryGetGraph(Uri graphUri, out bool created)
    {
        if (_store.HasGraph(graphUri))
        {
            created = false;
            return _store[graphUri];
        }
        else
        {
            created = true;
            var g = new Graph();
            g.BaseUri = graphUri;
            _store.Add(g);
            return g;
        }
    }

    /// <summary>
    /// Resets the Factory so any Graphs with contents are emptied.
    /// </summary>
    public void Reset()
    {
        foreach (IGraph g in _store.Graphs)
        {
            g.Clear();
        }
    }
}
