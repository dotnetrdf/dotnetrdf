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

namespace VDS.RDF.Query;

/// <summary>
/// The interface for the context object to be passed to pattern evaluation functions.
/// </summary>
public interface IPatternEvaluationContext
{
    /// <summary>
    /// Gets whether pattern evaluation should use rigorous evaluation mode.
    /// </summary>
    bool RigorousEvaluation { get; }

    /// <summary>
    /// Get whether the specified variable is found in the evaluation context.
    /// </summary>
    /// <param name="varName">The name of the variable to look for.</param>
    /// <returns>True if the evaluation context contains a whose name matches <paramref name="varName"/>, false otherwise.</returns>
    bool ContainsVariable(string varName);

    /// <summary>
    /// Gets whether the evaluation context contains a binding of the specified value to the specified variable.
    /// </summary>
    /// <param name="varName">The name of the variable to look for.</param>
    /// <param name="value">The expected value.</param>
    /// <returns>True if the evaluation context contains a binding for <paramref name="varName"/> to <paramref name="value"/>, false otherwise.</returns>
    bool ContainsValue(string varName, INode value);
}
