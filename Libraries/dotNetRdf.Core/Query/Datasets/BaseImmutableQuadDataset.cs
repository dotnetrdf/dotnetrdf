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
/// Abstract Base class for immutable quad datasets.
/// </summary>
public abstract class BaseImmutableQuadDataset
    : BaseQuadDataset
{
    /// <summary>
    /// Throws an error as this dataset is immutable.
    /// </summary>
    /// <param name="g">Graph.</param>
    public sealed override bool AddGraph(IGraph g)
    {
        throw new NotSupportedException("This dataset is immutable");
    }

    /// <summary>
    /// Throws an error as this dataset is immutable.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <param name="t">Triple.</param>
    [Obsolete("Replaced by AddQuad(IRefNode, Triple)")]
    public override bool AddQuad(Uri graphUri, Triple t)
    {
        throw new NotSupportedException("This dataset is immutable");
    }

    /// <summary>
    /// Adds a Quad to the Dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <param name="t">Triple.</param>
    public override bool AddQuad(IRefNode graphName, Triple t)
    {
        throw new NotSupportedException("This dataset is immutable");
    }

    /// <summary>
    /// Throws an error as this dataset is immutable.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    [Obsolete("Replaced by RemoveGraph(IRefNode)")]
    public sealed override bool RemoveGraph(Uri graphUri)
    {
        throw new NotSupportedException("This dataset is immutable");
    }

    /// <summary>
    /// Removes a Graph from the Dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <exception cref="NotSupportedException">May be thrown if the Dataset is immutable i.e. Updates not supported.</exception>
    public override bool RemoveGraph(IRefNode graphName)
    {
        throw new NotSupportedException("This dataset is immutable");
    }

    /// <summary>
    /// Throws an error as this dataset is immutable.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <param name="t">Triple.</param>
    [Obsolete("Replacecd by RemoveQuad(IRefNode, Triple)")]
    public override bool RemoveQuad(Uri graphUri, Triple t)
    {
        throw new NotSupportedException("This dataset is immutable");
    }

    /// <summary>
    /// Removes a quad from the dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <param name="t">Triple to remove.</param>
    /// <returns></returns>
    public override bool RemoveQuad(IRefNode graphName, Triple t)
    {
        throw new NotSupportedException("This dataset is immutable");
    }

    /// <summary>
    /// Throws an error as this dataset is immutable.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by GetModifiableGraph(IRefNode)")]
    public sealed override IGraph GetModifiableGraph(Uri graphUri)
    {
        throw new NotSupportedException("This dataset is immutable");
    }

    /// <summary>
    /// Gets the Graph with the given name from the Dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">May be thrown if the Dataset is immutable i.e. Updates not supported.</exception>
    /// <remarks>
    /// <para>
    /// Graphs returned from this method must be modifiable and the Dataset must guarantee that when it is Flushed or Disposed of that any changes to the Graph are persisted.
    /// </para>
    /// </remarks>
    public sealed override IGraph GetModifiableGraph(IRefNode graphName)
    {
        throw new NotSupportedException("This dataset is immutable");
    }
}