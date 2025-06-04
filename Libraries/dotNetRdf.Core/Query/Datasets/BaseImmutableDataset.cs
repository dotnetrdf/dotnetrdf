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

namespace VDS.RDF.Query.Datasets;

/// <summary>
/// Abstract Base Class for Immutable Datasets.
/// </summary>
public abstract class BaseImmutableDataset
    : BaseDataset
{
    /// <summary>
    /// Throws an exception since Immutable Datasets cannot be altered.
    /// </summary>
    /// <param name="g">Graph to add.</param>
    public override bool AddGraph(IGraph g)
    {
        throw new NotSupportedException("Cannot add a Graph to an immutable Dataset");
    }

    /// <summary>
    /// Throws an exception since Immutable Datasets cannot be altered.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    [Obsolete("Replaced by RemoveGraph(IRefNode)")]
    public override bool RemoveGraph(Uri graphUri)
    {
        throw new NotSupportedException("Cannot remove a Graph from an immutable Dataset");
    }

    /// <summary>
    /// Throws an exception since Immutable Datasets cannot be altered.
    /// </summary>
    /// <param name="graphName">Graph URI.</param>
    public override IGraph GetModifiableGraph(IRefNode graphName)
    {
        throw new NotSupportedException("Cannot retrieve a Modifiable Graph from an immutable Dataset");
    }

    /// <summary>
    /// Ensures that any changes to the Dataset (if any) are flushed to the underlying Storage.
    /// </summary>
    public sealed override void Flush()
    {
        // Does Nothing
    }

    /// <summary>
    /// Ensures that any changes to the Dataset (if any) are discarded.
    /// </summary>
    public sealed override void Discard()
    {
        // Does Nothing
    }
}