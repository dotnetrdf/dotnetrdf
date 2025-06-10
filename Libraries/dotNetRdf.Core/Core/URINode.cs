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
/// Class for representing URI Nodes.
/// </summary>
public class UriNode
    : BaseUriNode, IEquatable<UriNode>, IComparable<UriNode>
{
    /// <summary>
    /// Constructor for URI Nodes.
    /// </summary>
    /// <param name="uri">URI for the Node.</param>
    public UriNode(Uri uri) : base(uri) { }

    /// <summary>
    /// Implementation of Compare To for URI Nodes.
    /// </summary>
    /// <param name="other">URI Node to Compare To.</param>
    /// <returns></returns>
    /// <remarks>
    /// Simply invokes the more general implementation of this method.
    /// </remarks>
    public int CompareTo(UriNode other)
    {
        return base.CompareTo((IUriNode)other);
    }

    /// <summary>
    /// Determines whether this Node is equal to a URI Node.
    /// </summary>
    /// <param name="other">URI Node.</param>
    /// <returns></returns>
    public bool Equals(UriNode other)
    {
        return base.Equals((IUriNode)other);
    }
}
