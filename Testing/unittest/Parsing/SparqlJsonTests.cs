using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class SparqlJsonTests
    {
        private SparqlJsonParser _parser = new SparqlJsonParser();

        [TestMethod]
        public void ParsingSparqlJsonDates1()
        {
            String data = @"{
 ""head"" : { ""vars"" : [ ""date"" ] } ,
 ""results"" : {
  ""bindings"" : [
    { ""date"" : { ""type"" : ""literal"" , ""value"" : ""2012-12-03T11:41:00-08:00"" } }
  ]
 }
}";

            SparqlResultSet results = new SparqlResultSet();
            this._parser.Load(results, new StringReader(data));

            Assert.AreEqual(1, results.Count);
        }

        [TestMethod]
        public void ParsingSparqlJsonNumerics1()
        {
            String data = @"{
 ""head"" : { ""vars"" : [ ""num"" ] } ,
 ""results"" : {
  ""bindings"" : [
    { ""num"" : { ""type"" : ""literal"" , ""value"" : ""1234"" } }
  ]
 }
}";

            SparqlResultSet results = new SparqlResultSet();
            this._parser.Load(results, new StringReader(data));

            Assert.AreEqual(1, results.Count);
        }

        [TestMethod, ExpectedException(typeof(RdfParseException))]
        public void ParsingSparqlJsonNumerics2()
        {
            String data = @"{
 ""head"" : { ""vars"" : [ ""num"" ] } ,
 ""results"" : {
  ""bindings"" : [
    { ""num"" : { ""type"" : ""literal"" , ""value"" : 1234 } }
  ]
 }
}";

            SparqlResultSet results = new SparqlResultSet();
            this._parser.Load(results, new StringReader(data));

            Assert.AreEqual(1, results.Count);
        }

        [TestMethod]
        public void ParsingSparqlJsonBoolean1()
        {
            String data = @"{
 ""head"" : { ""vars"" : [ ""bool"" ] } ,
 ""results"" : {
  ""bindings"" : [
    { ""bool"" : { ""type"" : ""literal"" , ""value"" : ""true"" } }
  ]
 }
}";

            SparqlResultSet results = new SparqlResultSet();
            this._parser.Load(results, new StringReader(data));

            Assert.AreEqual(1, results.Count);
        }

        [TestMethod, ExpectedException(typeof(RdfParseException))]
        public void ParsingSparqlJsonBoolean2()
        {
            String data = @"{
 ""head"" : { ""vars"" : [ ""bool"" ] } ,
 ""results"" : {
  ""bindings"" : [
    { ""bool"" : { ""type"" : ""literal"" , ""value"" : true } }
  ]
 }
}";

            SparqlResultSet results = new SparqlResultSet();
            this._parser.Load(results, new StringReader(data));

            Assert.AreEqual(1, results.Count);
        }

        [TestMethod]
        public void ParsingSparqlJsonGuid1()
        {
            String data = @"{
 ""head"" : { ""vars"" : [ ""guid"" ] } ,
 ""results"" : {
  ""bindings"" : [
    { ""guid"" : { ""type"" : ""literal"" , ""value"" : """ + Guid.NewGuid().ToString() + @""" } }
  ]
 }
}";

            SparqlResultSet results = new SparqlResultSet();
            this._parser.Load(results, new StringReader(data));

            Assert.AreEqual(1, results.Count);
        }
    }
}
