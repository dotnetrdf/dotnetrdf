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

using System;

namespace VDS.RDF;

/**
 * Class for representing quads in memory.
 */

#nullable enable
public sealed class Quad: IComparable<Quad>, IEquatable<Quad>
{
    /// <summary>
    /// Get the subject node of the quad.
    /// </summary>
    public INode Subject { get; }

    /// <summary>
    /// Get the predicate node of the quad.
    /// </summary>
    public INode Predicate { get; }

    /// <summary>
    /// Get the object node of the quad.
    /// </summary>
    public INode Object { get; }
    
    /// <summary>
    /// Get the graph node of the quad.
    /// </summary>
    public IRefNode? Graph { get; }
    
    private readonly int _hashCode;

    /// <summary>
    /// Construct a new Quad with the specified subject, predicate, object, and optionally graph.
    /// </summary>
    /// <param name="subject">The subject node of the quad.</param>
    /// <param name="predicate">The predicate node of the quad.</param>
    /// <param name="object">The object node of the quad.</param>
    /// <param name="graphName">The graph that the statement is in. A null value indicates the unnamed graph.</param>
    public Quad(INode subject, INode predicate, INode @object, IRefNode? graphName)
    {
        Subject = subject;
        Predicate = predicate;
        Object = @object;
        Graph = @graphName;
        _hashCode = Tools.CombineHashCodes(
            Subject.GetHashCode(),
            Predicate.GetHashCode(),
            Object.GetHashCode(),
            Graph?.GetHashCode() ?? -1);
    }
    
    /// <summary>
    /// Construct a new Quad with the specified triple contained in the specified graph.
    /// </summary>
    /// <param name="t">The subject, predicate and object of the quad as a <see cref="Triple"/>.</param>
    /// <param name="graph">The graph that the statement is in. A null value indicates the unnamed graph.</param>
    public Quad(Triple t, IRefNode? graph) : this(t.Subject, t.Predicate, t.Object, graph) {}

    /// <summary>
    /// Create and return a new <see cref="Triple"/> instance created with the <see cref="Subject"/>, <see cref="Predicate"/>, and <see cref="Object"/> of this quad.
    /// </summary>
    /// <returns>A new <see cref="Triple"/> instance.</returns>
    public Triple AsTriple()
    {
        return new Triple(Subject, Predicate, Object);
    }
    
    /// <inheritdoc />
    public int CompareTo(Quad other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }
        if (ReferenceEquals(Subject, other.Subject) &&
            ReferenceEquals(Predicate, other.Predicate) &&
            ReferenceEquals(Object, other.Object) &&
            ReferenceEquals(Graph, other.Graph))
        {
            return 0;
        }
        var result = Subject.CompareTo(other.Subject);
        if (result == 0)
        {
            result = Predicate.CompareTo(other.Predicate);
            if (result == 0)
            {
                result = Object.CompareTo(other.Object);
                if (result == 0)
                {
                    if (Graph == null)
                    {
                        result = other.Graph == null ? 0 : 1;
                    }
                    else
                    {
                        result = Graph.CompareTo(other.Graph);
                    }
                }
            }
        }
        return result;
    }

    /// <inheritdoc />
    public bool Equals(Quad other)
    {
        return this.CompareTo(other) == 0;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _hashCode;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Subject}, {Predicate}, {Object}, {Graph}";
    }
}
