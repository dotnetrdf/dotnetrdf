using System;
using System.Diagnostics;
using System.Linq;
using VDS.RDF.Linq;
using VDS.RDF.Linq.Sparql;
using log4net.Config;
using NUnit.Framework;
using RdfMusic;

namespace UnitTests
{
    /// <summary>
    /// Summary description for TestRelationships
    /// </summary>
    [TestFixture]
    public class TestRelationships
    {

		[SetUp]
		public void SetUp()
		{
			XmlConfigurator.Configure();
		}
  
        [Test]
        public void TestAlbumToTracks()
        {
            var ctx = new MusicDataContext(Properties.Settings.Default.testSparqlEndpoint);
            Album album = (from a in ctx.Albums
                           where a.Name.StartsWith("Thomas")
                           select a).First();
            Assert.IsNotNull(album);
            Assert.IsTrue(album.Tracks.Count() > 1);
        }

        [Test]
        public void TestGetAlbum()
        {
            var ctx = new MusicDataContext(Properties.Settings.Default.testSparqlEndpoint);
            IQueryable<Album> q1 = from a in ctx.Albums where a.Name == "Thomas Laqueur - History Lectures" select a;
            Album[] aa = q1.ToArray();
            Assert.IsTrue(aa.Count() == 1);
        }

        [Test]
        public void TestTrackToAlbum1()
        {
            var ctx = new MusicDataContext(Properties.Settings.Default.testSparqlEndpoint);
            IQueryable<Track> q = from t in ctx.Tracks
                                  where t.Year == "2006" &&
                                        t.GenreName == "History 5"
                                  select t;
            try
            {
                Track[] ta = q.ToArray();

                Assert.IsTrue(ta.Count() == 2);
                Assert.IsTrue(ta[0].AlbumName == "Thomas Laqueur - History Lectures");
                Assert.IsTrue(ta[0].Album != null);
            }
            catch
            {
                Console.WriteLine(((LinqToSparqlQuery<Track>)q).QueryText);
                throw;
            }
        }
    }
}