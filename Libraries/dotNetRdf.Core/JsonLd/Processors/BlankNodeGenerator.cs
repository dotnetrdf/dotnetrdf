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
using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.JsonLd.Processors;

/// <summary>
/// Default implementation of the <see cref="IBlankNodeGenerator"/> interface.
/// </summary>
public class BlankNodeGenerator : IBlankNodeGenerator
{
    private readonly string _prefix;
    private int _counter;
    private readonly Dictionary<string, string> _identifierMap = new();

    /// <summary>
    /// Create a new generator instance.
    /// </summary>
    /// <param name="counterPrefix">The value to insert before the counter part of the generated blank node identifier. Must be a non-empty string.</param>
    public BlankNodeGenerator(string counterPrefix = "b")
    {
        if (string.IsNullOrEmpty(counterPrefix))
        {
            throw new ArgumentException("The counter prefix must be a non-empty string.");
        }
        _prefix = "_:" + counterPrefix;
    }
    
    private BlankNodeGenerator(string prefix, int counter, Dictionary<string, string> identifierMap)
    {
        _prefix = prefix;
        _counter = counter;
        _identifierMap = identifierMap;
    }

    /// <summary>
    /// Generates a unique blank node identifier for the specified input identifier.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    /// <remarks>Blank node identifiers are of the form _:{counterPrefix}{count} where {counterPrefix} is the
    /// string provided in the constructor of this class and {count} is a monotonically increasing integer value.</remarks>
    public string GenerateBlankNodeIdentifier(string identifier)
    {
        // 1 - If identifier is not null and has an entry in the identifier map, return the mapped identifier.
        if (identifier != null && _identifierMap.TryGetValue(identifier, out var mappedIdentifier))
        {
            return mappedIdentifier;
        }
        // 2 - Otherwise, generate a new blank node identifier
        mappedIdentifier = _prefix + _counter++;
        // 3 - If identifier is not null, create a new entry for identifier in identifier map and set its value to the new blank node identifier.
        if (identifier != null)
        {
            _identifierMap[identifier] = mappedIdentifier;
        }
        // 4 - Return the new blank node identifier.
        return mappedIdentifier;
    }

    /// <inheritdoc/>
    public string GetMappedIdentifier(string identifier)
    {
        // If identifier is not null and has an entry in the identifier map, return the mapped identifier.
        if (identifier != null && _identifierMap.TryGetValue(identifier, out var mappedIdentifier))
        {
            return mappedIdentifier;
        }

        return null;
    }

    /// <inheritdoc />
    public IEnumerable<string> GetMappedIdentifiers()
    {
        return _identifierMap.Keys;
    }

    /// <inheritdoc />
    public IBlankNodeGenerator Clone()
    {
        return new BlankNodeGenerator(_prefix, _counter, new Dictionary<string, string>(_identifierMap));
    }

    /// <inheritdoc />
    public IDictionary<string, string> GetDictionary() => this._identifierMap;
}