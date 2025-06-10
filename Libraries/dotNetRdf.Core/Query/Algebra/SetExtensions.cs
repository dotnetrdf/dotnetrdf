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

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Additional utility methods for ISet.
/// </summary>
public static class SetExtensions
{
    /// <summary>
    /// Creates a <see cref="SparqlResult"/> instance that contains all of the variables in this set.
    /// </summary>
    /// <param name="set"></param>
    /// <returns></returns>
    [Obsolete("Replaced by the ISparqlResultFactory interface and its implementation.")]
    public static SparqlResult AsSparqlResult(this ISet set)
    {
        return new SparqlResult(set.Variables.Select(var => new KeyValuePair<string, INode>(var, set[var])));
    }

    /// <summary>
    /// Creates a <see cref="SparqlResult"/> instance that contains the bindings for the specified variables in the set.
    /// </summary>
    /// <param name="set">The set containing the bindings to be added to the SPARQL result.</param>
    /// <param name="variables">The names of the variables to be included in the SPARQL result.</param>
    /// <returns></returns>
    public static SparqlResult AsSparqlResult(this ISet set, IEnumerable<string> variables)
    {
        return new SparqlResult(variables.Where(set.ContainsVariable)
            .Select(x => new KeyValuePair<string, INode>(x, set[x])));
    }
}
