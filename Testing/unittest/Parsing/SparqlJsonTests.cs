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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
{
    [TestFixture]
    public class SparqlJsonTests
    {
        private SparqlJsonParser _parser = new SparqlJsonParser();

        [Test]
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

        [Test]
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

        [Test, ExpectedException(typeof(RdfParseException))]
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

        [Test]
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

        [Test, ExpectedException(typeof(RdfParseException))]
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

        [Test]
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
