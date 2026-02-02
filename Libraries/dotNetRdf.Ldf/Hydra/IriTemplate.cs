/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Nodes;

namespace VDS.RDF.LDF.Hydra;

internal class IriTemplate(INode node, IGraph graph) : GraphWrapperNode(node, graph)
{
    internal string SubjectVariable =>
        SelectMapping(Vocabulary.Rdf.Subject)
        ?? throw new LdfException("IRI template contains no subject");

    internal string PredicateVariable =>
        SelectMapping(Vocabulary.Rdf.Predicate)
        ?? throw new LdfException("IRI template contains no predicate mapping");

    internal string ObjectVariable =>
        SelectMapping(Vocabulary.Rdf.Object)
        ?? throw new LdfException("IRI template contains no object mapping");

    internal string Template =>
        Vocabulary.Hydra.Template.ObjectsOf(this).SingleOrDefault()?.AsValuedNode().AsString()
        ?? throw new LdfException("IRI template contains no template statement");

    private IEnumerable<IriTemplateMapping> Mappings =>
        from n in Vocabulary.Hydra.Mapping.ObjectsOf(this)
        select new IriTemplateMapping(n, Graph);

    private string SelectMapping(IUriNode property) => (
            from m in Mappings
            where m.Property.Equals(property)
            select m.Variable)
            .SingleOrDefault();
}
