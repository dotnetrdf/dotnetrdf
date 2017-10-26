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
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Update
{

    public class LoadTests
    {
        private readonly SparqlUpdateParser _parser = new SparqlUpdateParser();

        [Fact]
        public void SparqlUpdateLoadQuads1()
        {
            var tripleStore = new TripleStore();
            SparqlUpdateCommandSet cmds = this._parser.ParseFromFile(@"resources\core-421\test.ru");

            tripleStore.ExecuteUpdate(cmds);
            Assert.Equal(3, tripleStore.Triples.Count());
            Assert.Equal(3, tripleStore.Graphs.Count);

            tripleStore.SaveToFile("core-421.nq", new NQuadsWriter());

            var newStore = new TripleStore();
            newStore.LoadFromFile("core-421.nq", new NQuadsParser());

            Assert.Equal(3, newStore.Triples.Count());
            Assert.Equal(2, newStore.Graphs.Count);
        }

        [Fact]
        public void SparqlUpdateLoadQuads2()
        {
            var tripleStore = new TripleStore();
            SparqlUpdateCommandSet cmds = this._parser.ParseFromFile(@"resources\core-421\test2.ru");

            tripleStore.ExecuteUpdate(cmds);
            Assert.Equal(3, tripleStore.Triples.Count());
            Assert.Equal(3, tripleStore.Graphs.Count);

            tripleStore.SaveToFile("core-421.nq", new NQuadsWriter());

            var newStore = new TripleStore();
            newStore.LoadFromFile("core-421.nq", new NQuadsParser());

            Assert.Equal(3, newStore.Triples.Count());
            Assert.Equal(2, newStore.Graphs.Count);
        }

        [Fact]
        public void SparqlUpdateLoadQuads3()
        {
            var tripleStore = new TripleStore();
            SparqlUpdateCommandSet cmds = this._parser.ParseFromFile(@"resources\core-421\test3.ru");

            tripleStore.ExecuteUpdate(cmds);
            Assert.Equal(3, tripleStore.Triples.Count());
            Assert.Equal(3, tripleStore.Graphs.Count);

            tripleStore.SaveToFile("core-421.nq", new NQuadsWriter());

            var newStore = new TripleStore();
            newStore.LoadFromFile("core-421.nq", new NQuadsParser());

            Assert.Equal(3, newStore.Triples.Count());
            Assert.Equal(2, newStore.Graphs.Count);
        }

        [Fact]
        public void SparqlUpdateLoadQuads4()
        {
            var tripleStore = new TripleStore();

            String g1 = Path.GetFullPath(@"resources\core-421\g1.nq").Replace('\\','/');
            String g2 = Path.GetFullPath(@"resources\core-421\g2.nq").Replace('\\','/');

            tripleStore.ExecuteUpdate("LOAD <file:///" + g1 + "> into graph <http://test.org/user>");
            tripleStore.ExecuteUpdate("LOAD <file:///" + g2 + "> into graph <http://test.org/prodList/>");
            Assert.Equal(3, tripleStore.Triples.Count());
            Assert.Equal(3, tripleStore.Graphs.Count);

            tripleStore.SaveToFile("core-421.nq", new NQuadsWriter());

            var newStore = new TripleStore();
            newStore.LoadFromFile("core-421.nq", new NQuadsParser());

            Assert.Equal(3, newStore.Triples.Count());
            Assert.Equal(2, newStore.Graphs.Count);
        }

        [Fact]
        public void SparqlUpdateLoadQuads5()
        {
            var tripleStore = new TripleStore();

            tripleStore.LoadFromFile(@"resources\core-421\test.nq");
            Assert.Equal(3, tripleStore.Triples.Count());
            Assert.Equal(2, tripleStore.Graphs.Count);

            tripleStore.SaveToFile("core-421.nq", new NQuadsWriter());

            var newStore = new TripleStore();
            newStore.LoadFromFile("core-421.nq", new NQuadsParser());

            Assert.Equal(3, newStore.Triples.Count());
            Assert.Equal(2, newStore.Graphs.Count);
        }
    }
}
