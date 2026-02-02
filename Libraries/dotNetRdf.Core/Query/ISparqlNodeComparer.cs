/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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

using System.Globalization;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query;

/// <summary>
/// Interface for a comparer that can compare INode and IValuedNode instances.
/// </summary>
public interface ISparqlNodeComparer
{
    /// <summary>
    /// Get the culture to use for string literal comparison.
    /// </summary>
    CultureInfo Culture { get; }
    /// <summary>
    /// Get the options to apply to string literal comparison.
    /// </summary>
    CompareOptions Options { get; }

    /// <summary>
    /// Attempt to compare the specified nodes.
    /// </summary>
    /// <param name="x">Node to be compared.</param>
    /// <param name="y">Node to be compared.</param>
    /// <param name="result">Comparison result.</param>
    /// <returns>True if a comparison could be made, false otherwise.</returns>
    /// <remarks><paramref name="result"/> is set to 0 if the nodes compare as equal, a positive value if <paramref name="x"/> compares greater than <paramref name="y"/>, or a negative value if <paramref name="y"/> compares greater than <paramref name="x"/>.</remarks>
    public bool TryCompare(INode x, INode y, out int result);

    /// <summary>
    /// Attempt to compare the specified nodes.
    /// </summary>
    /// <param name="x">Node to be compared.</param>
    /// <param name="y">Node to be compared.</param>
    /// <param name="result">Comparison result.</param>
    /// <returns>True if a comparison could be made, false otherwise.</returns>
    /// <remarks><paramref name="result"/> is set to 0 if the nodes compare as equal, a positive value if <paramref name="x"/> compares greater than <paramref name="y"/>, or a negative value if <paramref name="y"/> compares greater than <paramref name="x"/>.</remarks>
    public bool TryCompare(IValuedNode x, IValuedNode y, out int result);
}