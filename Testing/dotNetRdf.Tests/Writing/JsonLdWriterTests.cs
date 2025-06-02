/*
dotNetRDF is free and open source software licensed under the MIT License
-----------------------------------------------------------------------------
Copyright (c) 2009-2018 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)
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

using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF.Writing;

public class JsonLdWriterTests
{
    [Fact]
    public void ItCanSerializeAnRdfListThatIsNotReferenced()
    {
        // Reproduction for GitHub issue #600
        const string turtleContent = @"
@base <https://purl.org/coscine/ap/Dataverse_Citation_Metadata/minimal/>.

@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>.
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
@prefix xsd: <http://www.w3.org/2001/XMLSchema#>.
@prefix dcterms: <http://purl.org/dc/terms/>.
@prefix sh: <http://www.w3.org/ns/shacl#>.

_:autos28 rdf:first ""P1"";
          rdf:rest _:autos29.
_:autos29 rdf:first ""P2"";
          rdf:rest _:autos30.
_:autos30 rdf:first ""P3"";
          rdf:rest _:autos31.
_:autos31 rdf:first ""P4"";
          rdf:rest _:autos32.
_:autos32 rdf:first ""P5"";
          rdf:rest _:autos33.
_:autos33 rdf:first ""P6"";
          rdf:rest _:autos34.
_:autos34 rdf:first ""P7"";
          rdf:rest _:autos35.
_:autos35 rdf:first ""P8"";
          rdf:rest _:autos36.
_:autos36 rdf:first ""P9"";
          rdf:rest _:autos37.
_:autos37 rdf:first ""P10"";
          rdf:rest _:autos38.
_:autos38 rdf:first ""P11"";
          rdf:rest _:autos39.
_:autos39 rdf:first ""P12"";
          rdf:rest _:autos40.
_:autos40 rdf:first ""P13"";
          rdf:rest _:autos41.
_:autos41 rdf:first ""P14"";
          rdf:rest rdf:nil.
		  
<https://purl.org/coscine/ap/Dataverse_Citation_Metadata/minimal/> dcterms:created ""2023-08-04""^^xsd:date;
                                                                    dcterms:description ""Description DE""@de,
                                                                                        ""Description EN""@en;
                                                                    dcterms:license ""https://spdx.org/licenses/CC-BY-4.0.html"";
                                                                    dcterms:title ""Title DE""@de,
                                                                                  ""Title EN""@en;
                                                                    a rdfs:Class,
                                                                      sh:NodeShape;
                                                                    sh:closed false;
																	
                                                                    sh:property [].
";
		// Parse the Turtle string into an IGraph
		IGraph graph = new Graph();
		var turtleParser = new TurtleParser();
		turtleParser.Load(graph, new System.IO.StringReader(turtleContent));
        var tripleStore = new TripleStore();
        tripleStore.Add(graph);
        var storeWriter = new JsonLdWriter();
        StringWriter.Write(tripleStore, storeWriter);
    }
}