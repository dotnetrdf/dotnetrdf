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
/// Class representing Variable Nodes (only used for N3).
/// </summary>
public class VariableNode
    : BaseVariableNode, IEquatable<VariableNode>, IComparable<VariableNode>
{
    /// <summary>
    /// Creates a new Variable Node.
    /// </summary>
    /// <param name="varName">Variable Name.</param>
    public VariableNode(string varName)
        : base(varName) { }

    /// <summary>
    /// Compares this Node to another Variable Node.
    /// </summary>
    /// <param name="other">Variable Node.</param>
    /// <returns></returns>
    public int CompareTo(VariableNode other)
    {
        return base.CompareTo((IVariableNode)other);
    }

    /// <summary>
    /// Determines whether this Node is equal to a Variable Node.
    /// </summary>
    /// <param name="other">Variable Node.</param>
    /// <returns></returns>
    public bool Equals(VariableNode other)
    {
        return base.Equals((IVariableNode)other);
    }
}
