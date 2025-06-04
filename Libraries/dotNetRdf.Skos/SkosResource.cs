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
/// Represents a SKOS resource.
/// </summary>
public abstract class SkosResource
{
    /// <summary>
    /// Gets the original resource underlying the SKOS resource.
    /// </summary>
    public INode Resource { get; private set; }

    /// <summary>
    /// Gets the graph containing the SKOS resource.
    /// </summary>
    public IGraph Graph { get; }

    internal SkosResource(INode resource, IGraph graph)
    {
        Resource = resource ?? throw new RdfSkosException("Cannot create a SKOS Resource for a null Resource");
        Graph = graph ?? throw new RdfSkosException("Cannot create a SKOS resource for a null graph");
    }

    internal IEnumerable<SkosConcept> GetConcepts(string predicateUri)
    {
        return 
            GetObjects(predicateUri)
            .Select(o => new SkosConcept(o, Graph));
    }

    internal IEnumerable<INode> GetObjects(string predicateUri)
    {
        IUriNode predicate = Graph.CreateUriNode(Graph.UriFactory.Create(predicateUri));

        return Graph
            .GetTriplesWithSubjectPredicate(Resource, predicate)
            .Select(t => t.Object);
    }
}
