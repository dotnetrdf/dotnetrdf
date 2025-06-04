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
/// Represents a wrapper around a SKOS graph providing convenient access to concepts, schemes, and collections.
/// </summary>
public class SkosGraph : WrapperGraph
{
    /// <summary>
    /// Creates a new SKOS graph.
    /// </summary>
    /// <param name="name">The name to assign to the SKOS graph.</param>
    public SkosGraph(IRefNode name = null) : base(new Graph(name)) { }

    /// <summary>
    /// Creates a new SKOS graph for the given graph.
    /// </summary>
    /// <param name="g">The graph this SKOS graph wraps.</param>
    public SkosGraph(IGraph g) : base(g) { }

    /// <summary>
    /// Gets concept schemes contained in the graph.
    /// </summary>
    public IEnumerable<SkosConceptScheme> ConceptSchemes
    {
        get
        {
            return 
                GetInstances(SkosHelper.ConceptScheme)
                .Select(node => new SkosConceptScheme(node, this));
        }
    }

    /// <summary>
    /// Gets concepts contained in the graph.
    /// </summary>
    public IEnumerable<SkosConcept> Concepts
    {
        get
        {
            return 
                GetInstances(SkosHelper.Concept)
                .Select(node => new SkosConcept(node, this));
        }
    }

    /// <summary>
    /// Gets collections contained in the graph.
    /// </summary>
    public IEnumerable<SkosCollection> Collections
    {
        get
        {
            return 
                GetInstances(SkosHelper.Collection)
                .Select(node => new SkosCollection(node, this));
        }
    }

    /// <summary>
    /// Gets ordered collections contained in the graph.
    /// </summary>
    public IEnumerable<SkosOrderedCollection> OrderedCollections
    {
        get
        {
            return 
                GetInstances(SkosHelper.OrderedCollection)
                .Select(node => new SkosOrderedCollection(node, this));
        }
    }

    private IEnumerable<INode> GetInstances(string typeUri)
    {
        IUriNode a = CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
        IUriNode type = CreateUriNode(UriFactory.Create(typeUri));

        return 
            GetTriplesWithPredicateObject(a, type)
            .Select(t => t.Subject);
    }
}
