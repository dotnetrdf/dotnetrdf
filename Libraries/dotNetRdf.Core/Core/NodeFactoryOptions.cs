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
/// Configuration options that can be passed to the <see cref="NodeFactory"/> constructor.
/// </summary>
public class NodeFactoryOptions
{
    /// <summary>
    /// The initial base URI to use for the resolution of relative URI references. Defaults to null.
    /// </summary>
    public Uri BaseUri { get; set; }

    /// <summary>
    /// Whether or not to normalize the value strings of literal nodes.
    /// </summary>
    public bool NormalizeLiteralValues { get; set; }

    /// <summary>
    /// Whether or not to validate the language specifier of language-tagged literal nodes.
    /// </summary>
    [Obsolete("Replaced by NodeFactoryOptions.LanguageTagValidation")]
    public bool ValidateLanguageSpecifiers { get; set; } = true;

    /// <summary>
    /// Set the type of validation applied to the language specified of language tagged literal nodes.
    /// </summary>
    public LanguageTagValidationMode LanguageTagValidation { get; set; } = LanguageTagValidationMode.Turtle;

}
