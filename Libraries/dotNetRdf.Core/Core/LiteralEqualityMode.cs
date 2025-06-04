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

namespace VDS.RDF;

/// <summary>
/// Possible Literal Equality Mode Settings.
/// </summary>
public enum LiteralEqualityMode
{
    /// <summary>
    /// Strict Mode compares Literals according to the official W3C RDF Specification
    /// </summary>
    /// <remarks>
    /// This means Literals are equal if and only if:
    /// <ol>
    /// <li>The Lexical Values are identical when compared using Ordinal Comparison</li>
    /// <li>The Language Tags if present are identical</li>
    /// <li>The Datatypes if present are identical</li>
    /// </ol>
    /// </remarks>
    Strict,
    /// <summary>
    /// Loose Mode compares Literals based on values (if they have known Datatypes)
    /// </summary>
    /// <remarks>
    /// This means Literals can be equal if they have lexically different values which are equivalent when converted to the Datatype.
    /// <br /><br />
    /// Literals without Datatypes and those whose Datatypes are unknown or not handled by the Library will be compared using lexical equivalence as with <see cref="LiteralEqualityMode.Strict">Strict</see> mode.
    /// </remarks>
    Loose,
}
