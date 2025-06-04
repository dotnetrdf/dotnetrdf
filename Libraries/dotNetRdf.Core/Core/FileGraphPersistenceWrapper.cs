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
using System.IO;

namespace VDS.RDF;

/// <summary>
/// The File Graph Persistence Wrapper is a wrapper around another Graph that will be persisted to a file.
/// </summary>
public class FileGraphPersistenceWrapper 
    : GraphPersistenceWrapper
{
    private readonly string _filename;

    /// <summary>
    /// Creates a new File Graph Persistence Wrapper around the given Graph.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="filename">File to persist to.</param>
    public FileGraphPersistenceWrapper(IGraph g, string filename)
        : base(g)
    {
        _filename = filename ?? throw new ArgumentException("Cannot persist to a null Filename", nameof(filename));
    }

    /// <summary>
    /// Creates a new File Graph Persistence Wrapper around a new empty Graph.
    /// </summary>
    /// <param name="filename">File to persist to.</param>
    /// <param name="graphName">The name to assign to the new graph.</param>
    /// <remarks>
    /// If the given file already exists then the Graph will be loaded from that file.
    /// </remarks>
    public FileGraphPersistenceWrapper(string filename, IRefNode graphName = null)
        : base(new Graph(graphName))
    {
        if (filename == null) throw new ArgumentException("Cannot persist to a null Filename", nameof(filename));

        if (File.Exists(filename))
        {
            _g.LoadFromFile(filename);
        }
    }

    /// <summary>
    /// Returns that Triple persistence is not supported.
    /// </summary>
    protected override bool SupportsTriplePersistence => false;

    /// <summary>
    /// Persists the entire Graph to a File.
    /// </summary>
    protected override void PersistGraph()
    {
        this.SaveToFile(_filename);
    }
}