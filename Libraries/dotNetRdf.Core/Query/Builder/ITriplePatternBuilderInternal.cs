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

using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder;

/// <summary>
/// Additional methods and properties that can be used by extensions of the <see cref="ITriplePatternBuilder"/> interface.
/// </summary>
public interface ITriplePatternBuilderInternal : ITriplePatternBuilder
{
    /// <summary>
    /// Gets the pattern item factory used by the builder.
    /// </summary>
    IPatternItemFactory PatternItemFactory { get; }

    /// <summary>
    /// Gets the prefix manager, which allows adding prefixes to the query or graph pattern.
    /// </summary>
    INamespaceMapper Prefixes { get; }

    /// <summary>
    /// Add TriplePattern to the builder.
    /// </summary>
    /// <param name="triplePattern"></param>
    void AddPattern(TriplePattern triplePattern);
}