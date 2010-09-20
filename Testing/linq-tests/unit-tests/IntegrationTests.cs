/* 
 * Copyright (C) 2007, Andrew Matthews http://aabs.wordpress.com/
 *
 * This file is Free Software and part of LinqToRdf http://code.google.com/fromName/linqtordf/
 *
 * It is licensed under the following license:
 *   - Berkeley License, V2.0 or any newer version
 *
 * You may not use this file except in compliance with the above license.
 *
 * See http://code.google.com/fromName/linqtordf/ for the complete text of the license agreement.
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Linq;
using VDS.RDF.Linq.Sparql;
using NUnit.Framework;
using RdfMusic;

namespace UnitTests
{
    /// <summary>
    /// Summary description for IntegrationTests
    /// </summary>
    [TestFixture]
    public class IntegrationTests : HighLevelTests
    {
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in ontology class have run
        // [TearDown]
        // public static void MyClassCleanup() { }
        //
        // Use SetUp to run code before running each test 
        // [SetUp]
        // public void MySetUp() { }
        //
        // Use TearDown to run code after each test has run

        [TearDown]
        public void MyTearDown()
        {
        }

        [Test]
        public void Query1()
        {
            var ts = new LinqTripleStore(CreateMemoryStore());
            IQueryable<Track> qry = new RdfDataContext(ts).ForType<Track>();
            IQueryable<Track> q = from t in qry
                                  where t.ArtistName == "Thomas Laqueur"
                                  select t;
            var resultList = new List<Track>();
            resultList.AddRange(q);
            Assert.IsTrue(resultList.Count > 0);
        }

        [Test]
        public void Query3()
        {
            LinqTripleStore ts = CreateOnlineTripleStore();
            IRdfQuery<Track> qry = new RdfDataContext(ts).ForType<Track>();
            Track[] q = (from t in qry
                         where Convert.ToInt32(t.Year) > 1998 &&
                               t.GenreName == "Rory Blyth: The Smartest Man in the World"
                         select t).ToArray();
            Assert.IsTrue(q.Length > 0);
        }

        [Test]
        public void Query5()
        {
            LinqTripleStore ts = CreateOnlineTripleStore();
            var ctx = new RdfDataContext(ts);
            IRdfQuery<Track> qry = ctx.ForType<Track>();
            IQueryable<Track> q = from t in qry
                                  where t.GenreName == "Rory Blyth: The Smartest Man in the World"
                                  select t;
            foreach (Track track in q)
            {
                track.Rating = 5;
            }
            ctx.SubmitChanges();
        }

        [Test]
        public void QueryWithProjection()
        {
            var ctx = new MusicDataContext(Properties.Settings.Default.testSparqlEndpoint);
            var q = from t in ctx.Tracks
                    where t.Year == "2006" &&
                          t.GenreName == "History 5"
                    select new {t.Title, t.FileLocation};
            Assert.IsTrue(q.ToList().Count() == 2);
        }

        [Test,ExpectedException(typeof(NotSupportedException))]
        public void QueryWithMethodProjection()
        {
            var ctx = new MusicDataContext(Properties.Settings.Default.testSparqlEndpoint);
            var q = from t in ctx.Tracks
                    where t.Year == "2006" &&
                          t.GenreName == "History 5"
                    select t.Title.Trim();
            Assert.IsTrue(q.ToList().Count() == 2);
        }

        [Test]
        public void SparqlQuery()
        {
            LinqTripleStore ts = CreateSparqlTripleStore();
            IRdfQuery<Track> qry = new RdfDataContext(ts).ForType<Track>();
            var q = from t in qry
                    where t.Year == "2007" &&
                          t.GenreName == "Rory Blyth: The Smartest Man in the World"
                    select new {t.Title, t.FileLocation};
            foreach (var track in q)
            {
                Console.WriteLine(track.Title + ": " + track.FileLocation);
            }
        }

        [Test]
        public void SparqlQueryAll()
        {
            LinqTripleStore ts = CreateSparqlTripleStore();
            IRdfQuery<Track> qry = new RdfDataContext(ts).ForType<Track>();
            IQueryable<Track> q = from t in qry select t;
            var lt = new List<Track>(q);
            foreach (Track track in q)
            {
                Console.WriteLine("Track: " + track.Title);
            }
            Assert.IsTrue(lt.Count > 1);
        }

        [Test]
        public void SparqlQueryDistinct()
        {
            LinqTripleStore ts = CreateSparqlTripleStore();
            IRdfQuery<Track> qry = new RdfDataContext(ts).ForType<Track>();
            var q = (from t in qry select t).Distinct();
            var lt = new List<Track>(q);
            foreach (Track track in q)
            {
                Console.WriteLine("Track: " + track.Title);
            }
            Assert.IsTrue(lt.Count > 1);

            //Ensure appropriate modifier has been applied to query
            LinqToSparqlQuery<Track> q2 = (LinqToSparqlQuery<Track>)q;
            Console.WriteLine(q2.QueryText);
            foreach (String name in q2.Expressions.Keys)
            {
                Console.WriteLine(name);
            }
            Assert.IsTrue(q2.QueryText.Contains("SELECT DISTINCT"));
        }

        [Test]
        public void SparqlQueryReduced()
        {
            LinqTripleStore ts = CreateSparqlTripleStore();
            IRdfQuery<Track> qry = new RdfDataContext(ts).ForType<Track>();
            var q = (from t in qry select t).Reduced<Track>();
            var lt = new List<Track>(q);
            foreach (Track track in q)
            {
                Console.WriteLine("Track: " + track.Title);
            }
            Assert.IsTrue(lt.Count > 1);

            //Ensure appropriate modifier has been applied to query
            LinqToSparqlQuery<Track> q2 = (LinqToSparqlQuery<Track>)q;
            Console.WriteLine(q2.QueryText);
            foreach (String name in q2.Expressions.Keys)
            {
                Console.WriteLine(name);
            }
            Assert.IsTrue(q2.QueryText.Contains("SELECT REDUCED"));
        }

        [Test]
        public void SparqlQueryOrdered()
        {
            LinqTripleStore ts = CreateSparqlTripleStore();
            IRdfQuery<Track> qry = new RdfDataContext(ts).ForType<Track>();
            var q = from t in qry
                    where t.Year == "2006" &&
                          t.GenreName == "History 5 | Fall 2006 | UC Berkeley"
                    orderby t.FileLocation
                    select t;
            foreach (var track in q)
            {
                Console.WriteLine(track.Title + ": " + track.FileLocation);
            }

            Console.WriteLine(q.GetType().ToString());

            //Ensure appropriate modifier has been applied to query
            LinqToSparqlQuery<Track> q2 = (LinqToSparqlQuery<Track>)q;
            Console.WriteLine(q2.QueryText);
            foreach (String name in q2.Expressions.Keys)
            {
                Console.WriteLine(name);
            }
            Assert.IsTrue(q2.QueryText.Contains("ORDER BY"));
        }

        [Test]
        public void SparqlQueryUsingCachedResults()
        {
            LinqTripleStore ts = CreateSparqlTripleStore();
            IRdfQuery<Track> qry = new RdfDataContext(ts).ForType<Track>();
            var q = from t in qry
                    where t.Year == "2007" &&
                          t.GenreName == "Rory Blyth: The Smartest Man in the World"
                    select new {t.Title, t.FileLocation};
            foreach (var track in q)
            {
                Console.WriteLine(track.Title + ": " + track.FileLocation);
            }
            // this should not invoke query parsing or execution
            foreach (var track in q)
            {
                Console.WriteLine("Title: " + track.Title);
            }
        }

        [Test]
        public void SparqlQueryUsingHttp()
        {
            LinqTripleStore ts = CreateOnlineTripleStore();
            IRdfQuery<Track> qry = new RdfDataContext(ts).ForType<Track>();
            var q = from t in qry
                    where t.Year == "2007" &&
                          t.GenreName == "Rory Blyth: The Smartest Man in the World"
                    select new {t.Title, t.FileLocation};
            foreach (var track in q)
            {
                Console.WriteLine(track.Title + ": " + track.FileLocation);
            }
        }

        [Test]
        public void SparqlQueryWithTheLot()
        {
            LinqTripleStore ts = CreateSparqlTripleStore();
            MusicDataContext ctx = new MusicDataContext(Properties.Settings.Default.testSparqlEndpoint);
            var q = (from t in ctx.Tracks
                     where t.Year == "2006" &&
                           t.GenreName == "History 5 | Fall 2006 | UC Berkeley"
                     orderby t.FileLocation
                     select new {t.Title, t.FileLocation}).Skip(10).Take(5);
            foreach (var track in q)
            {
                Console.WriteLine(track.Title + ": " + track.FileLocation);
            }
        }

        [Test,ExpectedException(typeof(NotSupportedException))]
        public void SparqlQueryGroupBy1()
        {
            LinqTripleStore ts = CreateSparqlTripleStore();
            IRdfQuery<Track> qry = new RdfDataContext(ts).ForType<Track>();
            var q = from t in qry
                    group t by t.ArtistName into t2
                    select t2.First();
            var lt = new List<Track>(q);
            foreach (Track track in q)
            {
                Console.WriteLine("Track: " + track.Title + " by " + track.ArtistName);
            }

            //Ensure appropriate modifier has been applied to query
            LinqToSparqlQuery<Track> q2 = (LinqToSparqlQuery<Track>)q;
            Console.WriteLine(q2.QueryText);
            foreach (String name in q2.Expressions.Keys)
            {
                Console.WriteLine(name);
            }
            Assert.IsTrue(q2.QueryText.Contains("GROUP BY"));
        }

        [Test,ExpectedException(typeof(NotSupportedException))]
        public void SparqlQueryGroupBy2()
        {
            LinqTripleStore ts = CreateSparqlTripleStore();
            IRdfQuery<Track> qry = new RdfDataContext(ts).ForType<Track>();
            var q = from t in qry
                    group t by t.ArtistName into t2
                    select t2;
            //var lt = new List<Track>(q);
            foreach (IGrouping<string,Track> group in q)
            {
                Console.WriteLine("Group for Artist: " + group.Key + " has " + group.Count() + " Tracks");
            }
            //Assert.IsTrue(lt.Count == 4);

            //Ensure appropriate modifier has been applied to query
            LinqToSparqlQuery<Track> q2 = (LinqToSparqlQuery<Track>)q;
            Console.WriteLine(q2.QueryText);
            foreach (String name in q2.Expressions.Keys)
            {
                Console.WriteLine(name);
            }
            Assert.IsTrue(q2.QueryText.Contains("GROUP BY"));
        }
    }
}