#if !NO_FILE

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
            SparqlUpdateCommandSet cmds = this._parser.ParseFromFile(@"..\\resources\core-421\test.ru");

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
            SparqlUpdateCommandSet cmds = this._parser.ParseFromFile(@"..\\resources\core-421\test2.ru");

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
            SparqlUpdateCommandSet cmds = this._parser.ParseFromFile(@"..\\resources\core-421\test3.ru");

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

            String g1 = Path.GetFullPath(@"..\\resources\core-421\g1.nq").Replace('\\','/');
            String g2 = Path.GetFullPath(@"..\\resources\core-421\g2.nq").Replace('\\','/');

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

            tripleStore.LoadFromFile(@"..\\resources\core-421\test.nq");
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

#endif
