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
using System.Collections;
using System.Collections.Generic;

namespace VDS.RDF.JsonLd;

/// <summary>
/// Interface for a blank node identifier generator that generates a unique blank node identifier
/// for a given input identifier.
/// </summary>
public interface IBlankNodeGenerator
{
    /// <summary>
    /// Return a unique blank node identifier for the specified input identifier.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns>A blank node identifier string (using standard Turtle notation for blank node identifiers).</returns>
    /// <remarks>An implementation MUST guarantee to return the same blank node identifier when called multiple times with the same input identifier.</remarks>
    string GenerateBlankNodeIdentifier(string identifier);


    /// <summary>
    /// Returns the mapped identifier for the specified input identifier, or null if no mapping exists.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public string GetMappedIdentifier(string identifier);
    
    /// <summary>
    /// Returns all input identifiers mapped by this issuer.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetMappedIdentifiers();

    /// <summary>
    /// Returns a copy of the object.
    /// </summary>
    /// <returns></returns>
    public IBlankNodeGenerator Clone();


    /// <summary>
    /// Returns the mapping dictionary. Only useful for unit tests.
    /// </summary>
    /// <returns></returns>
    public IDictionary<string, string> GetDictionary();
}