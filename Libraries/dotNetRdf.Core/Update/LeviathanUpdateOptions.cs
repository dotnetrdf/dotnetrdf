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
using VDS.RDF.Query;

namespace VDS.RDF.Update;

/// <summary>
/// A class encapsulating run-time options that can be configured for a <see cref="LeviathanUpdateProcessor"/>.
/// </summary>
public class LeviathanUpdateOptions : LeviathanQueryOptions
{
#pragma warning disable CS0618 // Type or member is obsolete
    private long _updateExecutionTimeout = Options.UpdateExecutionTimeout; // = 180000;
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Get or set whether to automatically flush dataset changes after an update changeset is processed.
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool AutoCommit { get; set; } = true;

    /// <summary>
    /// Gets/Sets the Hard Timeout limit for SPARQL Update Execution (in milliseconds).
    /// </summary>
    /// <remarks>
    /// This is used to stop SPARQL Updates running away and never completing execution, it defaults to 3 mins (180,000 milliseconds).
    /// </remarks>
    public long UpdateExecutionTimeout
    {
        get => _updateExecutionTimeout;
        set
        {
            _updateExecutionTimeout = Math.Max(0, value);
            QueryExecutionTimeout = _updateExecutionTimeout;
        }
    }
}