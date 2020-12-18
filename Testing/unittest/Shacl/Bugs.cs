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
    }
}
