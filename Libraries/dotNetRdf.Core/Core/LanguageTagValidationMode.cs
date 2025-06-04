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
/// Enumeration of the forms of language tag validation supported by the library.
/// </summary>
public enum LanguageTagValidationMode
{
    /// <summary>
    /// Do not perform any validation on language tags
    /// </summary>
    None = 0,
    /// <summary>
    /// Validate language tags against the production defined in the Turtle 1.1 specification
    /// </summary>
    /// <remarks>
    /// <para>The Turtle 1.1 grammar requires a language tag to match the production: <code>'@' [a-zA-Z]+ ('-' [a-zA-Z0-9]+)*</code>.</para>
    /// <para>NOTE: This validation is more lax than the <see cref="WellFormed"/> validation option.</para>
    /// </remarks>
    Turtle = 1,
    /// <summary>
    /// Validate that language tags are well-formed according to the rules defined by the BCP-47 specification.
    /// </summary>
    WellFormed = 2,
}
