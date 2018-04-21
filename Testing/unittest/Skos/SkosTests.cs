/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Linq;
using Xunit;

namespace VDS.RDF.Skos
{
    public class SkosTests
    {
        [Fact]
        public void Graph_gets_conceptScheme()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#ConceptScheme> .
");

            var resource = skosGraph.Triples.SubjectNodes.Single();
            var skosResource = skosGraph.ConceptSchemes.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Graph_gets_concept()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
");

            var resource = skosGraph.Triples.SubjectNodes.Single();
            var skosResource = skosGraph.Concepts.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Graph_gets_collection()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Collection> .
");

            var resource = skosGraph.Triples.SubjectNodes.Single();
            var skosResource = skosGraph.Collections.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Graph_gets_orderedCollection()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#OrderedCollection> .
");

            var resource = skosGraph.Triples.SubjectNodes.Single();
            var skosResource = skosGraph.OrderedCollections.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void ConceptScheme_gets_hasTopConcept()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/conceptScheme> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#ConceptScheme> .
<http://example.com/conceptScheme> <http://www.w3.org/2004/02/skos/core#hasTopConcept> <http://example.com/concept> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/concept"));
            var skosResource = skosGraph.ConceptSchemes.Single().HasTopConcept.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_inScheme()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept> <http://www.w3.org/2004/02/skos/core#inScheme> <http://example.com/conceptScheme> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/conceptScheme"));
            var skosResource = skosGraph.Concepts.Single().InScheme.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_topConceptOf()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept> <http://www.w3.org/2004/02/skos/core#topConceptOf> <http://example.com/conceptScheme> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/conceptScheme"));
            var skosResource = skosGraph.Concepts.Single().TopConceptOf.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_prefLabel()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept> <http://www.w3.org/2004/02/skos/core#prefLabel> """" .
");

            var resource = skosGraph.Nodes.LiteralNodes().Single();
            var skosResource = skosGraph.Concepts.Single().PrefLabel.Single();

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_altLabel()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept> <http://www.w3.org/2004/02/skos/core#altLabel> """" .
");

            var resource = skosGraph.Nodes.LiteralNodes().Single();
            var skosResource = skosGraph.Concepts.Single().AltLabel.Single();

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_hiddenLabel()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept> <http://www.w3.org/2004/02/skos/core#hiddenLabel> """" .
");

            var resource = skosGraph.Nodes.LiteralNodes().Single();
            var skosResource = skosGraph.Concepts.Single().HiddenLabel.Single();

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_notation()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept> <http://www.w3.org/2004/02/skos/core#notation> """" .
");

            var resource = skosGraph.Nodes.LiteralNodes().Single();
            var skosResource = skosGraph.Concepts.Single().Notation.Single();

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_note()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept> <http://www.w3.org/2004/02/skos/core#note> """" .
");

            var resource = skosGraph.Nodes.LiteralNodes().Single();
            var skosResource = skosGraph.Concepts.Single().Note.Single();

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_changeNote()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept> <http://www.w3.org/2004/02/skos/core#changeNote> """" .
");

            var resource = skosGraph.Nodes.LiteralNodes().Single();
            var skosResource = skosGraph.Concepts.Single().ChangeNote.Single();

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_definition()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept> <http://www.w3.org/2004/02/skos/core#definition> """" .
");

            var resource = skosGraph.Nodes.LiteralNodes().Single();
            var skosResource = skosGraph.Concepts.Single().Definition.Single();

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_editorialNote()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept> <http://www.w3.org/2004/02/skos/core#editorialNote> """" .
");

            var resource = skosGraph.Nodes.LiteralNodes().Single();
            var skosResource = skosGraph.Concepts.Single().EditorialNote.Single();

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_example()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept> <http://www.w3.org/2004/02/skos/core#example> """" .
");

            var resource = skosGraph.Nodes.LiteralNodes().Single();
            var skosResource = skosGraph.Concepts.Single().Example.Single();

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_historyNote()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept> <http://www.w3.org/2004/02/skos/core#historyNote> """" .
");

            var resource = skosGraph.Nodes.LiteralNodes().Single();
            var skosResource = skosGraph.Concepts.Single().HistoryNote.Single();

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_scopeNote()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept> <http://www.w3.org/2004/02/skos/core#scopeNote> """" .
");

            var resource = skosGraph.Nodes.LiteralNodes().Single();
            var skosResource = skosGraph.Concepts.Single().ScopeNote.Single();

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_semanticRelation()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept1> <http://www.w3.org/2004/02/skos/core#semanticRelation> <http://example.com/concept2> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/concept2"));
            var skosResource = skosGraph.Concepts.Single().SemanticRelation.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_broader()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept1> <http://www.w3.org/2004/02/skos/core#broader> <http://example.com/concept2> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/concept2"));
            var skosResource = skosGraph.Concepts.Single().Broader.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_narrower()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept1> <http://www.w3.org/2004/02/skos/core#narrower> <http://example.com/concept2> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/concept2"));
            var skosResource = skosGraph.Concepts.Single().Narrower.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_related()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept1> <http://www.w3.org/2004/02/skos/core#related> <http://example.com/concept2> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/concept2"));
            var skosResource = skosGraph.Concepts.Single().Related.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_broaderTransitive()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept1> <http://www.w3.org/2004/02/skos/core#broaderTransitive> <http://example.com/concept2> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/concept2"));
            var skosResource = skosGraph.Concepts.Single().BroaderTransitive.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_narrowerTransitive()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept1> <http://www.w3.org/2004/02/skos/core#narrowerTransitive> <http://example.com/concept2> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/concept2"));
            var skosResource = skosGraph.Concepts.Single().NarrowerTransitive.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_mappingRelation()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept1> <http://www.w3.org/2004/02/skos/core#mappingRelation> <http://example.com/concept2> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/concept2"));
            var skosResource = skosGraph.Concepts.Single().MappingRelation.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_closeMatch()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept1> <http://www.w3.org/2004/02/skos/core#closeMatch> <http://example.com/concept2> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/concept2"));
            var skosResource = skosGraph.Concepts.Single().CloseMatch.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_exactMatch()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept1> <http://www.w3.org/2004/02/skos/core#exactMatch> <http://example.com/concept2> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/concept2"));
            var skosResource = skosGraph.Concepts.Single().ExactMatch.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_broadMatch()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept1> <http://www.w3.org/2004/02/skos/core#broadMatch> <http://example.com/concept2> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/concept2"));
            var skosResource = skosGraph.Concepts.Single().BroadMatch.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_narrowMatch()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept1> <http://www.w3.org/2004/02/skos/core#narrowMatch> <http://example.com/concept2> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/concept2"));
            var skosResource = skosGraph.Concepts.Single().NarrowMatch.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Concept_gets_relatedMatch()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/concept1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Concept> .
<http://example.com/concept1> <http://www.w3.org/2004/02/skos/core#relatedMatch> <http://example.com/concept2> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/concept2"));
            var skosResource = skosGraph.Concepts.Single().RelatedMatch.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void Collection_gets_member()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/collection> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#Collection> .
<http://example.com/collection> <http://www.w3.org/2004/02/skos/core#member> <http://example.com/concept> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/concept"));
            var skosResource = skosGraph.Collections.Single().Member.Single().Resource;

            Assert.Same(resource, skosResource);
        }

        [Fact]
        public void OrderedCollection_gets_memberList()
        {
            var skosGraph = new SkosGraph();
            skosGraph.LoadFromString(@"
<http://example.com/orderedCollection> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2004/02/skos/core#OrderedCollection> .
<http://example.com/orderedCollection> <http://www.w3.org/2004/02/skos/core#memberList> _:list .
_:list <http://www.w3.org/1999/02/22-rdf-syntax-ns#first> <http://example.com/concept> .
_:list <http://www.w3.org/1999/02/22-rdf-syntax-ns#rest> <http://www.w3.org/1999/02/22-rdf-syntax-ns#nil> .
");

            var resource = skosGraph.GetUriNode(new Uri("http://example.com/concept"));
            var skosResource = skosGraph.OrderedCollections.Single().MemberList.Single().Resource;

            Assert.Same(resource, skosResource);
        }
    }
}
