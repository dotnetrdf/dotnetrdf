/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

using Xunit;

namespace VDS.RDF.Shacl
{
    public class Bugs
    {
        // https://github.com/dotnetrdf/dotnetrdf/issues/303
        [Fact]
        public void Issue303()
        {
            var shapesGraph = new Graph();
            shapesGraph.LoadFromString(@"
@prefix sh: <http://www.w3.org/ns/shacl#> .

[
    sh:targetNode <http://example.com/s> ;
    sh:sparql [ sh:select 'SELECT * { ?this <http://example.com/p> ?o . }' ] ;
] .
");

            var dataGraph = new Graph();
            dataGraph.LoadFromString(@"<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            new ShapesGraph(shapesGraph).Validate(dataGraph);
        }

        // https://github.com/dotnetrdf/dotnetrdf/issues/670
        [Fact]
        public void Issue670()
        {
            var shapesGraph = new Graph();
            shapesGraph.LoadFromString(@"
@prefix sh: <http://www.w3.org/ns/shacl#> .
@prefix schema: <http://schema.org/> .
@prefix foaf: <http://xmlns.com/foaf/0.1/> .
@prefix : <http://example.com/> .
:UserShape a sh:NodeShape ;
sh:targetClass :User ;
sh:property [
sh:path schema:givenName ;
sh:equals foaf:firstName
];
sh:property [
sh:path schema:givenName ;
sh:disjoint schema:lastName
] .");
            var dataGraph1 = new Graph();
            dataGraph1.LoadFromString(@"
@prefix schema: <http://schema.org/> .
@prefix foaf: <http://xmlns.com/foaf/0.1/> .
@prefix : <http://example.com/> .
:alice a :User ; #Passes as a :UserShape
schema:givenName ""Alice"";
schema:lastName ""Cooper"";
foaf:firstName ""Alice"" .");
            var dataGraph2 = new Graph();
            dataGraph2.LoadFromString(@"
@prefix schema: <http://schema.org/> .
@prefix foaf: <http://xmlns.com/foaf/0.1/> .
@prefix : <http://example.com/> .
:bob a :User ; #Fails as a :UserShape
schema:givenName ""Bob"";
schema:lastName ""Smith"" ;
foaf:firstName ""Robert"" .
");
            var dataGraph3 = new Graph();
            dataGraph3.LoadFromString(@"
@prefix schema: <http://schema.org/> .
@prefix foaf: <http://xmlns.com/foaf/0.1/> .
@prefix : <http://example.com/> .
:carol a :User ; #Fails as a :UserShape
schema:givenName ""Carol"";
schema:lastName ""Carol"" ;
foaf:firstName ""Carol"" .");
            var shaclGraph = new ShapesGraph(shapesGraph);
            Assert.True(shaclGraph.Validate(dataGraph1).Conforms);
            Assert.False(shaclGraph.Validate(dataGraph2).Conforms);
            Assert.False(shaclGraph.Validate(dataGraph3).Conforms);
        }

        // https://github.com/dotnetrdf/dotnetrdf/issues/692
        [Fact]
        public void Issue692()
        {
            var dataGraph = new Graph();
            dataGraph.LoadFromString("""
                @prefix ex: <http://example.org/> .
                @prefix foaf: <http://xmlns.com/foaf/0.1/> .

                ex:Alice
                    a foaf:Person ;
                    foaf:knows ex:Bob .

                ex:Bob a ex:Person .
                """);

            var shapesGraph = new Graph();
            shapesGraph.LoadFromString(""""
                @prefix ex: <http://example.org/> .
                @prefix foaf: <http://xmlns.com/foaf/0.1/> .
                @prefix sh: <http://www.w3.org/ns/shacl#> .

                ex:doesNotKnowSomeoneShape
                    sh:targetClass foaf:Person ;
                    sh:property [
                        sh:path foaf:knows ;
                        ex:someone ex:Bob ;
                    ]
                .
    
                ex:doesNotKnowSomeoneComponent
                    a sh:ConstraintComponent ;
                    sh:parameter [
                        sh:path ex:someone ;
                    ] ;
                    sh:propertyValidator [
                        sh:select """
                            PREFIX ex: <http://example.org/>
                            PREFIX foaf: <http://xmlns.com/foaf/0.1/>
                            SELECT DISTINCT $this
                            WHERE {
                                $this
                                    a foaf:Person ;
                                    $PATH $someone ;
                                .
                            }
                        """ ;
                    ] .
                """");



            var shaclGraph = new ShapesGraph(shapesGraph);

            Assert.False(shaclGraph.Validate(dataGraph).Conforms);
        }

        // https://github.com/dotnetrdf/dotnetrdf/issues/693
        [Fact]
        public void Issue693()
        {
            var dataGraph = new Graph();
            dataGraph.LoadFromString("""
                @prefix foaf: <http://xmlns.com/foaf/0.1/> .

                [ a foaf:Person ] .

                """);

            var shapesGraph = new Graph();
            shapesGraph.LoadFromString(""""
                @prefix ex: <http://example.org/> .
                @prefix foaf: <http://xmlns.com/foaf/0.1/> .
                @prefix sh: <http://www.w3.org/ns/shacl#> .

                ex:knowsSomeoneShape
                    sh:targetClass foaf:Person ;
                    ex:someone ex:Bob ; 
                .

                ex:knowsSomeoneComponent
                    a sh:ConstraintComponent ;
                    sh:parameter [
                        sh:path ex:someone ;
                    ] ;
                    sh:validator [
                        sh:ask """
                            PREFIX ex: <http://example.org/>
                            PREFIX foaf: <http://xmlns.com/foaf/0.1/>
                            ASK
                            WHERE {
                                $this
                                    a foaf:Person ;
                                    foaf:knows $someone .
                            }
                        """ ;
                    ] .
                """");


            var shaclGraph = new ShapesGraph(shapesGraph);

            Assert.True(shaclGraph.Validate(dataGraph).Conforms);
        }
    }
}
