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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using VDS.RDF.Linq;
using NUnit.Framework;
using RdfMusic;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using UnitTests.Properties;

namespace UnitTests
{
	[TestFixture]
	public class LocalSparqlTests
{
        private SparqlQueryParser _parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);

		#region query strings

		private string generatedQueryString2 =
			@"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-generatedNamespaceChar#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
PREFIX fn: <http://www.w3.org/2005/xpath-functions#> 
PREFIX ontology: <http://aabs.purl.org/ontologies/2007/04/music#>

SELECT ?Title ?FileLocation 
WHERE {
_:mi ontology:year ?Year ;
ontology:genreName ?GenreName ;
ontology:title ?Title ;
ontology:fileLocation ?FileLocation .
FILTER((regex(?Year, ""2007""))&&(regex(?GenreName, ""Rory Blyth: The Smartest Man in the World"") ))
}
";
		#endregion

		private static IInMemoryQueryableStore store;

		private static void CreateIInMemoryQueryableStore()
		{
			string serialisedLocation = Settings.Default.testStoreLocation;
			store = new TripleStore();
//			store.AddReasoner(new Euler(new N3Reader(MusicConstants.OntologyURL)));
            VDS.RDF.Graph g = new VDS.RDF.Graph();
            FileLoader.Load(g, serialisedLocation);
            store.Add(g);
		}

		[Test]
		public void LocalSparqlQuery1()
		{
			CreateIInMemoryQueryableStore();
			var x = new {Title="foo", FileLocation="bar"};
			ObjectDeserialiserQuerySink sink = new ObjectDeserialiserQuerySink(typeof(Track), x.GetType(), null, false, null, null);
			string qry = CreateQueryForArtist("Rory Blythe");

            SparqlQuery query = this._parser.ParseFromString(qry);
            SparqlEvaluationContext evalContext = new SparqlEvaluationContext(query, store);
            ISparqlAlgebra algebra = query.ToAlgebra();
            BaseMultiset localResults = algebra.Evaluate(evalContext);
            sink.Fill(localResults);

			foreach (object track in sink.IncomingResults)
			{
				Console.WriteLine(track.ToString());
			}

		}
		private string CreateQueryForArtist(string artistName)
		{
//			string queryFmt = "@prefix m: <http://aabs.purl.org/ontologies/2007/04/music#> .\n";
//			foreach (PropertyInfo info in OwlClassSupertype.GetAllPersistentProperties(typeof(Track)))
//			{
//				queryFmt += string.Format("?track <{0}> ?{1} .\n", OwlClassSupertype.GetPropertyUri(typeof(Track), info.Name), info.Name);
//			}
//			queryFmt += string.Format("?track <{0}> \"{1}\" .\n", OwlClassSupertype.GetPropertyUri(typeof(Track), "ArtistName"), artistName);
			return generatedQueryString2;
		}
		#region Additional test attributes
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
			if (store != null)
				store = null;
		}

		#endregion
	}
}
