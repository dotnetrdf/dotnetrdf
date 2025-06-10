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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder;

/// <summary>
/// Interface for creating <see cref="PatternItem"/> instances.
/// </summary>
public interface IPatternItemFactory
{
    /// <summary>
    /// Create a variable pattern item.
    /// </summary>
    /// <param name="variableName">The name of the variable.</param>
    /// <returns></returns>
    PatternItem CreateVariablePattern(string variableName);

    /// <summary>
    /// Create a URI node pattern.
    /// </summary>
    /// <param name="qName">The CURIE representation of the URI.</param>
    /// <param name="namespaceMapper">The mapper to use when resolving the prefix part of the CURIE.</param>
    /// <returns></returns>
    PatternItem CreateNodeMatchPattern(string qName, INamespaceMapper namespaceMapper);

    /// <summary>
    /// Create a URI node pattern.
    /// </summary>
    /// <param name="uri">The URI for the pattern item.</param>
    /// <returns></returns>
    PatternItem CreateNodeMatchPattern(Uri uri);

    /// <summary>
    /// Create a pattern item that matches the specified node.
    /// </summary>
    /// <param name="node">The node to match.</param>
    /// <returns></returns>
    PatternItem CreateNodeMatchPattern(INode node);

    /// <summary>
    /// Create a pattern item that matches a specific blank node.
    /// </summary>
    /// <param name="blankNodeIdentifier">The blank node identifier to match.</param>
    /// <returns></returns>
    PatternItem CreateBlankNodeMatchPattern(string blankNodeIdentifier);

    /// <summary>
    /// Create a pattern item that matches a literal value.
    /// </summary>
    /// <param name="literal">The value to match as a literal.</param>
    /// <returns></returns>
    /// <remarks>If <paramref name="literal"/> is a <see cref="DateTime"/> or <see cref="DateTimeOffset"/> then the literal pattern will use the appropriate XML Schema-compatible string representation as the literal value. For all other types, the value returned by the ToString method will be used as the literal value.</remarks>
    PatternItem CreateLiteralNodeMatchPattern(object literal);

    /// <summary>
    /// Create a pattern item that matches a literal value with its associated datatype.
    /// </summary>
    /// <param name="literal">The literal value to match.</param>
    /// <param name="datatype">The literal datatype URI to match.</param>
    /// <returns></returns>
    /// <remarks>If <paramref name="literal"/> is a <see cref="DateTime"/> or <see cref="DateTimeOffset"/> then the literal pattern will use the appropriate XML Schema-compatible string representation as the literal value. For all other types, the value returned by the ToString method will be used as the literal value.</remarks>
    PatternItem CreateLiteralNodeMatchPattern(object literal, Uri datatype);

    /// <summary>
    /// Create a pattern item that matches a literal value with an associated language tag.
    /// </summary>
    /// <param name="literal">The literal value to match.</param>
    /// <param name="langSpec">The language tag to match.</param>
    /// <returns></returns>
    /// <remarks>If <paramref name="literal"/> is a <see cref="DateTime"/> or <see cref="DateTimeOffset"/> then the literal pattern will use the appropriate XML Schema-compatible string representation as the literal value. For all other types, the value returned by the ToString method will be used as the literal value.</remarks>
    PatternItem CreateLiteralNodeMatchPattern(object literal, string langSpec);

    /// <summary>
    /// Create a pattern item that matches a node of a specific type.
    /// </summary>
    /// <param name="nodeType">The type of node to match. Must be one of <see cref="IBlankNode"/>, <see cref="ILiteralNode"/>, <see cref="IUriNode"/> or <see cref="IVariableNode"/>.</param>
    /// <param name="patternString">The string value of the pattern item.</param>
    /// <param name="namespaceMapper">The namespace mapper to use when resolving any prefixes in <paramref name="patternString"/>.</param>
    /// <returns></returns>
    /// <remarks>It is recommended to use one of the CreateXxxNodeMatchPattern methods in preference to this method.</remarks>
    PatternItem CreatePatternItem(Type nodeType, string patternString, INamespaceMapper namespaceMapper);
}