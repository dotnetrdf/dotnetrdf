/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class ServiceTests
    {
        [TestMethod]
        public void SparqlServiceUsingDBPedia()
        {
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
                Assert.Fail("Should have returned a SPARQL Result Set");
            }
        }

        [TestMethod]
        public void SparqlServiceUsingDBPediaAndBindings()
        {
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
                Assert.Fail("Should have returned a SPARQL Result Set");
            }
        }

        [TestMethod]
        public void SparqlServiceWithNonExistentService()
        {
            String query = "SELECT * WHERE { SERVICE <http://www.dotnetrdf.org/noSuchService> { ?s a ?type } } LIMIT 10";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(new TripleStore());
            try
            {
                Object results = processor.ProcessQuery(q);
                Assert.Fail("Should have errored");
            }
            catch (RdfQueryException queryEx)
            {
                Console.WriteLine("Errored as expected");
                TestTools.ReportError("Query Error", queryEx);
            }
        }

        [TestMethod]
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
