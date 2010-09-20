using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Linq;
using VDS.RDF.Linq.Sparql;
using NUnit.Framework;
using RdfMusic;


namespace UnitTests
{
    [TestFixture]
    public class PersistenceTests : HighLevelTests
    {
        [Test]
        public void Persist1()
        {
            LinqTripleStore store = new LinqTripleStore(CreateMemoryStore());

            Track t = new Track()
            {
                AlbumName = "Test Album",
                ArtistName = "Some Artist",
                Comment = "blah de blah",
                GenreName = "Easy Listening",
                InstanceUri = "http://example.org/music/someAlbum/tracks/1",
                Title = "Track 1",
                Rating = 5,
                Year = "2010"
            };

            Assert.IsTrue(store.SupportsPersistence, "Persistence should be supported by in-memory stores");

            Console.WriteLine(store.UpdateProcessor.GetSaveCommandText(t, null));

            store.UpdateProcessor.SaveObject(t, null);

            IQueryable<Track> qry = new RdfDataContext(store).ForType<Track>();
            IQueryable<Track> q = from x in qry
                                  where x.ArtistName == "Some Artist"
                                  select x;
            Assert.IsTrue(q.Count() > 0, "Expected 1 or more results");
            Assert.IsTrue(q.Count() == 1, "Expected only 1 result");
        }

        [Test]
        public void PersistToGraph1()
        {
            LinqTripleStore store = new LinqTripleStore(CreateMemoryStore());

            Track t = new Track()
            {
                AlbumName = "Test Album",
                ArtistName = "Some Artist",
                Comment = "blah de blah",
                GenreName = "Easy Listening",
                InstanceUri = "http://example.org/music/someAlbum/tracks/1",
                Title = "Track 1",
                Rating = 5,
                Year = "2010"
            };

            Assert.IsTrue(store.SupportsPersistence, "Persistence should be supported by in-memory stores");

            String text = store.UpdateProcessor.GetSaveCommandText(t, "http://example.org/someGraph");
            Console.WriteLine(text);
            Assert.IsTrue(text.Contains("GRAPH <"), "Persistence Command should have contained a GRAPH clause");

            store.UpdateProcessor.SaveObject(t, "http://example.org/someGraph");

            RdfDataContext context = new RdfDataContext(store);
            IQueryable<Track> qry = context.ForType<Track>();
            IQueryable<Track> q = from x in qry
                                  where x.ArtistName == "Some Artist"
                                  select x;

            Assert.IsTrue(q.Count() > 0, "Expected 1 or more results");
            Assert.IsTrue(q.Count() == 1, "Expected only 1 result");

            //Try setting Graph to non-existent Graph - should then throw an error
            try
            {
                context.DefaultGraph = "http://example.org/noSuchGraph";
                int count = q.Count();
                Assert.Fail("Should have thrown an error as tried to query a non-existent Graph");
            }
            catch
            {
                Console.WriteLine("Errored as expected when a non-existent Graph was used");
            }

            //Try setting Graph to correct Graph, should then get the 1 result as expected
            context.DefaultGraph = "http://example.org/someGraph";
            Assert.IsTrue(q.Count() == 1, "Expected only 1 result");
        }
    }
}
