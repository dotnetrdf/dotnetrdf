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
/// Class for representing RDF-star triple nodes.
/// </summary>
public class TripleNode : BaseTripleNode, IComparable<TripleNode>, IEquatable<TripleNode>
{
    /// <summary>
    /// Create a new node that quotes the specified triple.
    /// </summary>
    /// <param name="triple">The triple to be quoted.</param>
    public TripleNode(Triple triple):base(triple){}

    /// <summary>
    /// Determines whether this node is equal to another <see cref="TripleNode"/>.
    /// </summary>
    /// <param name="other">The other node to compare to.</param>
    /// <returns>True if the quoted triple of this node is equal to the quoted triple of the other node, false otherwise.</returns>
    public bool Equals(TripleNode other)
    {
        return base.Equals(other as ITripleNode);
    }

    /// <summary>
    /// Perform a sort order comparison of this node with another <see cref="TripleNode"/>.
    /// </summary>
    /// <param name="other">The other node to compare to.</param>
    /// <returns>1 if <paramref name="other"/> is null, otherwise the result of comparing this node's <see cref="Triple"/> property value with <paramref name="other"/>'s <see cref="Triple"/> property value.</returns>
    public int CompareTo(TripleNode other)
    {
        return base.CompareTo(other as ITripleNode);
    }
}
