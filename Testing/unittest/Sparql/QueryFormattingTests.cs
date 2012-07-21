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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class QueryFormattingTests
    {
        private SparqlFormatter _formatter = new SparqlFormatter();

        [TestMethod]
        public void SparqlFormattingFilter1()
        {
            String query = "SELECT * WHERE { { ?s ?p ?o } FILTER(ISURI(?o)) }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Console.WriteLine("ToString() form:");
            String toString = q.ToString();
            Console.WriteLine(toString);
            Console.WriteLine();
            Console.WriteLine("Format() form:");
            String formatted = this._formatter.Format(q);
            Console.WriteLine(formatted);

            Assert.IsTrue(toString.Contains("FILTER"), "ToString() form should contain FILTER");
            Assert.IsTrue(formatted.Contains("FILTER"), "Format() form should contain FILTER");
        }

        [TestMethod]
        public void SparqlFormattingFilter2()
        {
            String query = "SELECT * WHERE { { ?s ?p ?o } FILTER(REGEX(?o, 'search', 'i')) }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Console.WriteLine("ToString() form:");
            String toString = q.ToString();
            Console.WriteLine(toString);
            Console.WriteLine();
            Console.WriteLine("Format() form:");
            String formatted = this._formatter.Format(q);
            Console.WriteLine(formatted);

            Assert.IsTrue(toString.Contains("FILTER"), "ToString() form should contain FILTER");
            Assert.IsTrue(toString.Contains("i"), "ToString() form should contain i option");
            Assert.IsTrue(formatted.Contains("FILTER"), "Format() form should contain FILTER");
            Assert.IsTrue(toString.Contains("i"), "Format() form should contain i option");
        }
    }
}
