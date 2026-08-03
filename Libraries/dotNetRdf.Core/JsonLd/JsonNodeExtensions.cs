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

using System.Text.Json;
using System.Text.Json.Nodes;

internal static class JsonNodeExtensions
{
    /// <summary>
    /// Creates a detached clone of the given JsonNode.
    /// </summary>
    /// <remarks>
    /// If the node is null, returns null.
    /// If the node has no parent, returns the node itself.
    /// Otherwise, returns a deep clone of the node.
    /// </remarks>
    public static JsonNode DetachedClone(this JsonNode node)
    {
        if (node == null) return null;
        if (node.Parent == null) return node;
        return node.DeepClone();
    }

    /// <summary>
    /// Gets the value kind of the given JsonNode, returning JsonValueKind.Null if the node is null.
    /// </summary>
    /// <param name="node">The JsonNode to get the value kind of.</param>
    /// <returns>The value kind of the JsonNode, or JsonValueKind.Null if the node is null.</returns>
    public static JsonValueKind SafeValueKind(this JsonNode node)
    {
        if (node == null) return JsonValueKind.Null;
        return node.GetValueKind();
    }
}