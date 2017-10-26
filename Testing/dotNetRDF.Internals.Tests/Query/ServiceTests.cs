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
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.XunitExtensions;

namespace VDS.RDF.Query
{
    public class ServiceTests
    {
        [SkippableFact]
        public void SparqlServiceUsingDBPedia()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing))
            {
                throw new SkipTestException("Test Config marks Remote Parsing as unavailable, test cannot be run");
            }

            String query = "SELECT * WHERE { SERVICE <http://dbpedia.org/sparql> { ?s a ?type } } LIMIT 10";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(new TripleStore());
            Object results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);
            }
            else
            {
                Assert.True(false, "Should have returned a SPARQL Result Set");
            }
        }

        [SkippableFact]
        public void SparqlServiceUsingDBPediaAndBindings()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing))
            {
                throw new SkipTestException("Test Config marks Remote Parsing as unavailable, test cannot be run");
            }

            String query = "SELECT * WHERE { SERVICE <http://dbpedia.org/sparql> { ?s a ?type } } VALUES ?s { <http://dbpedia.org/resource/Southampton> <http://dbpedia.org/resource/Ilkeston> }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(new TripleStore());
            Object results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);
            }
            else
            {
                Assert.True(false, "Should have returned a SPARQL Result Set");
            }
        }

        [Fact]
        public void SparqlServiceWithNonExistentService()
        {
            String query = "SELECT * WHERE { SERVICE <http://www.dotnetrdf.org/noSuchService> { ?s a ?type } } LIMIT 10";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(new TripleStore());
            try
            {
                Object results = processor.ProcessQuery(q);
                Assert.True(false, "Should have errored");
            }
            catch (RdfQueryException queryEx)
            {
                Console.WriteLine("Errored as expected");
                TestTools.ReportError("Query Error", queryEx);
            }
        }

        [Fact]
        public void SparqlServiceWithSilentNonExistentService()
        {
            String query = "SELECT * WHERE { SERVICE SILENT <http://www.dotnetrdf.org/noSuchService> { ?s a ?type } } LIMIT 10";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(new TripleStore());
            try
            {
                Object results = processor.ProcessQuery(q);
                Console.WriteLine("Errors were suppressed as expected");
                TestTools.ShowResults(results);
            }
            catch (RdfQueryException queryEx)
            {
                Console.WriteLine("Errored when errors should have been suppressed");
                TestTools.ReportError("Query Error", queryEx);
            }
        }
    }
}
