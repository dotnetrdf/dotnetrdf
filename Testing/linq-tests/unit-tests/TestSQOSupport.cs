using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RdfMusic;

namespace UnitTests
{
    /// <summary>
    /// Summary description for TestSQOSupport
    /// </summary>
    [TestFixture]
    public class TestSQOSupport
    {

        [Test]
        public void TestFirst()
        {
            var ctx = new MusicDataContext(Properties.Settings.Default.testSparqlEndpoint);
            var albums = (from a in ctx.Albums
                          where a.Name.StartsWith("Thomas")
                          select a);
            Assert.IsTrue(albums.First() != null);
        }

        [Test]
        public void TestFirstOrDefault()
        {
            var ctx = new MusicDataContext(Properties.Settings.Default.testSparqlEndpoint);
            var albums = (from a in ctx.Albums
                          where a.Name.StartsWith("Thomas")
                          select a);
            Assert.IsTrue(albums.FirstOrDefault() != null);
        }

        [Test]
        public void TestCount()
        {
            var ctx = new MusicDataContext(Properties.Settings.Default.testSparqlEndpoint);
            var albums = (from a in ctx.Albums
                          where a.Name.StartsWith("Thomas")
                          select a);
            Assert.IsTrue(albums.Count() == 1);

        }

        [Test]
        public void TestSkip()
        {
            var ctx = new MusicDataContext(Properties.Settings.Default.testSparqlEndpoint);
            var albums = (from a in ctx.Albums
                          where a.Name.StartsWith("Thomas")
                          select a);
            Assert.IsTrue(albums.Count() == 1);

        }

        [Test, ExpectedException(typeof(NotSupportedException))]
        public void TestSum()
        {
            var ctx = new MusicDataContext(Properties.Settings.Default.testSparqlEndpoint);
            var albums = (from a in ctx.Albums
                          where a.Name.StartsWith("Thomas")
                          select a);
            Assert.IsTrue(albums.Sum(a=>1) == 1);
        }

    }
}
