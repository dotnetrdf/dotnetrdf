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
/// Represents a mapping from a URI to a QName.
/// </summary>
public class QNameMapping 
{
    /// <summary>
    /// Creates a new QName Mapping.
    /// </summary>
    /// <param name="u">URI.</param>
    public QNameMapping(string u)
    {
        Uri = u;
    }

    /// <summary>
    /// URI this is a mapping for.
    /// </summary>
    public string Uri { get; }

    /// <summary>
    /// QName this URI maps to.
    /// </summary>
    public string QName { get; set; }

    /// <summary>
    /// Gets the String representation of the URI.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Uri;
    }

    /// <summary>
    /// Checks whether this is equal to another Object.
    /// </summary>
    /// <param name="obj">Object to test against.</param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (obj is QNameMapping)
        {
            return ToString().Equals(obj.ToString(), StringComparison.Ordinal);
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Uri.GetHashCode();
    }
}