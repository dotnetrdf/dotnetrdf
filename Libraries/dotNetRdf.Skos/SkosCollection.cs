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

using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.Skos;

/// <summary>
/// Represents a labelled group of SKOS concepts.
/// </summary>
public class SkosCollection : SkosMember
{
    /// <summary>
    /// Creates a new collection for the given resource.
    /// </summary>
    /// <param name="resource">Resource representing the collection.</param>
    /// <param name="graph">Graph containing the resource representing the collection.</param>
    public SkosCollection(INode resource, IGraph graph) : base(resource, graph) { }

    /// <summary>
    /// Gets the members of the collection.
    /// </summary>
    public IEnumerable<SkosMember> Member
    {
        get
        {
            return 
                GetObjects(SkosHelper.Member)
                .Select(x=>SkosMember.Create(x, Graph));
        }
    }
}
