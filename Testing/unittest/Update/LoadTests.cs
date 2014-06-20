using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Update
{
    [TestFixture]
    public class LoadTests
    {
        private readonly SparqlUpdateParser _parser = new SparqlUpdateParser();

        [Test]
        public void SparqlUpdateLoadQuads1()
        {
            var tripleStore = new TripleStore();
            SparqlUpdateCommandSet cmds = this._parser.ParseFromFile(@"resources\core-421\test.ru");

            tripleStore.ExecuteUpdate(cmds);
            Assert.That(tripleStore.Triples.ToList(), Has.Count.EqualTo(3));
            Assert.That(tripleStore.Graphs, Has.Count.EqualTo(3));

            tripleStore.SaveToFile("core-421.nq", new NQuadsWriter());

            var newStore = new TripleStore();
            newStore.LoadFromFile("core-421.nq", new NQuadsParser());

            Assert.That(newStore.Triples.ToList(), Has.Count.EqualTo(3));
            Assert.That(newStore.Graphs, Has.Count.EqualTo(2));
        }

        [Test]
        public void SparqlUpdateLoadQuads2()
        {
            var tripleStore = new TripleStore();
            SparqlUpdateCommandSet cmds = this._parser.ParseFromFile(@"resources\core-421\test2.ru");

            tripleStore.ExecuteUpdate(cmds);
            Assert.That(tripleStore.Triples.ToList(), Has.Count.EqualTo(3));
            Assert.That(tripleStore.Graphs, Has.Count.EqualTo(3));

            tripleStore.SaveToFile("core-421.nq", new NQuadsWriter());

            var newStore = new TripleStore();
            newStore.LoadFromFile("core-421.nq", new NQuadsParser());

            Assert.That(newStore.Triples.ToList(), Has.Count.EqualTo(3));
            Assert.That(newStore.Graphs, Has.Count.EqualTo(2));
        }

        [Test]
        public void SparqlUpdateLoadQuads3()
        {
            var tripleStore = new TripleStore();

            String g1 = Path.GetFullPath(@"resources\core-421\g1.nq").Replace('\\','/');
            String g2 = Path.GetFullPath(@"resources\core-421\g2.nq").Replace('\\','/');

            tripleStore.ExecuteUpdate("LOAD <file:///" + g1 + "> into graph <http://test.org/user>");
            tripleStore.ExecuteUpdate("LOAD <file:///" + g2 + "> into graph <http://test.org/prodList/>");
            Assert.That(tripleStore.Triples.ToList(), Has.Count.EqualTo(3));
            Assert.That(tripleStore.Graphs, Has.Count.EqualTo(3));

            tripleStore.SaveToFile("core-421.nq", new NQuadsWriter());

            var newStore = new TripleStore();
            newStore.LoadFromFile("core-421.nq", new NQuadsParser());

            Assert.That(newStore.Triples.ToList(), Has.Count.EqualTo(3));
            Assert.That(newStore.Graphs, Has.Count.EqualTo(2));
        }

        [Test]
        public void SparqlUpdateLoadQuads4()
        {
            var tripleStore = new TripleStore();

            tripleStore.LoadFromFile(@"resources\core-421\test.nq");
            Assert.That(tripleStore.Triples.ToList(), Has.Count.EqualTo(3));
            Assert.That(tripleStore.Graphs, Has.Count.EqualTo(2));

            tripleStore.SaveToFile("core-421.nq", new NQuadsWriter());

            var newStore = new TripleStore();
            newStore.LoadFromFile("core-421.nq", new NQuadsParser());

            Assert.That(newStore.Triples.ToList(), Has.Count.EqualTo(3));
            Assert.That(newStore.Graphs, Has.Count.EqualTo(2));
        }
    }
}
