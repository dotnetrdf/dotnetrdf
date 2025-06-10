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
/// Class for representing Blank RDF Nodes.
/// </summary>
public class BlankNode 
    : BaseBlankNode, IEquatable<BlankNode>, IComparable<BlankNode>
{
    /// <summary>
    /// Create a new blank node.
    /// </summary>
    /// <param name="id">Custom Node ID to use.</param>
    /// <remarks>This constructor does not check for identifier collisions in a graph. When creating blank nodes to store in a <see cref="IGraph"/>, it is strongly recommended to only create blank nodes through the <see cref="INodeFactory"/> interface of the target graph so as to avoid the possibility of duplicating existing blank nodes.</remarks>
    public BlankNode(string id)
        : base(id) { }

    /// <summary>
    /// Internal Only constructor for Blank Nodes.
    /// </summary>
    /// <param name="factory">Node Factory from which to obtain a Node ID.</param>
    protected internal BlankNode(INodeFactory factory)
        : base(factory) { }


    /// <summary>
    /// Implementation of Compare To for Blank Nodes.
    /// </summary>
    /// <param name="other">Blank Node to Compare To.</param>
    /// <returns></returns>
    /// <remarks>
    /// Simply invokes the more general implementation of this method.
    /// </remarks>
    public int CompareTo(BlankNode other)
    {
        return CompareTo((IBlankNode)other);
    }

    /// <summary>
    /// Determines whether this Node is equal to a Blank Node.
    /// </summary>
    /// <param name="other">Blank Node.</param>
    /// <returns></returns>
    public bool Equals(BlankNode other)
    {
        return base.Equals((IBlankNode)other);
    }
}
