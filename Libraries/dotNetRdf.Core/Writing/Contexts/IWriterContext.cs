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
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing.Contexts;

/// <summary>
/// Interface for Writer Contexts.
/// </summary>
public interface IWriterContext
{
    /// <summary>
    /// Gets the Graph being written.
    /// </summary>
    IGraph Graph
    {
        get;
    }

    /// <summary>
    /// Gets the TextWriter being written to.
    /// </summary>
    TextWriter Output
    {
        get;
    }

    /// <summary>
    /// Gets/Sets the Pretty Printing Mode used.
    /// </summary>
    bool PrettyPrint
    {
        get;
        set;
    }

    /// <summary>
    /// Gets/Sets the High Speed Mode used.
    /// </summary>
    bool HighSpeedModePermitted
    {
        get;
        set;
    }

    /// <summary>
    /// Gets/Sets the Compression Level used.
    /// </summary>
    int CompressionLevel
    {
        get;
        set;
    }

    /// <summary>
    /// Gets/Sets the Node Formatter used.
    /// </summary>
    INodeFormatter NodeFormatter
    {
        get;
        set;
    }

    /// <summary>
    /// Gets/Sets the URI Formatter used.
    /// </summary>
    IUriFormatter UriFormatter
    {
        get;
        set;
    }

    /// <summary>
    /// Gets/sets the URI factory used.
    /// </summary>
    IUriFactory UriFactory { get; set; }
}