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
    public class FilterPlacementTests
    {
        [TestMethod]
        public void SparqlFilterOptionalNotBound()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
            query.Namespaces.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
            query.CommandText = "SELECT * WHERE { ?property a rdf:Property . OPTIONAL { ?property rdfs:range ?range } FILTER (!BOUND(?range)) }";

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);
            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            if (results != null)
            {
                TestTools.ShowResults(results);

                Assert.IsTrue(results.All(r => !r.HasValue("range") || r["range"] == null), "There should be no values for ?range returned");
            }
            else
            {
                Assert.Fail("Did not get a SparqlResultSet as expected");
            }
        }
    }
}
