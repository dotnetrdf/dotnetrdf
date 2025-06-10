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
using VDS.RDF.Parsing;

namespace VDS.RDF.Skos;

/// <summary>
/// Represents SKOS resources that can be members of collections (concepts and collections).
/// </summary>
public abstract class SkosMember : SkosResource
{
    internal SkosMember(INode resource, IGraph graph) : base(resource, graph) { }

    internal static SkosMember Create(INode node, IGraph graph)
    {
        IUriNode a = graph.CreateUriNode(graph.UriFactory.Create(RdfSpecsHelper.RdfType));
        IEnumerable<Triple> typeStatements = graph.GetTriplesWithSubjectPredicate(node, a);

        IUriNode skosOrderedCollection = graph.CreateUriNode(graph.UriFactory.Create(SkosHelper.OrderedCollection));
        if (typeStatements.WithObject(skosOrderedCollection).Any())
        {
            return new SkosOrderedCollection(node, graph);
        }

        IUriNode skosCollection = graph.CreateUriNode(graph.UriFactory.Create(SkosHelper.Collection));
        if (typeStatements.WithObject(skosCollection).Any())
        {
            return new SkosCollection(node, graph);
        }

        return new SkosConcept(node, graph);
    }
}
