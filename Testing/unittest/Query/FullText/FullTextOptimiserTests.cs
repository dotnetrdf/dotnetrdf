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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Query.FullText
{
    [TestClass]
    public class FullTextOptimiserTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private List<IAlgebraOptimiser> _optimisers;
        private SparqlFormatter _formatter = new SparqlFormatter();

        private void TestOptimisation(String query)
        {
            query = "PREFIX pf: <http://jena.hpl.hp.com/ARQ/property#>" + query;
            SparqlQuery q = this._parser.ParseFromString(query);
            Console.WriteLine(this._formatter.Format(q));

            Console.WriteLine("Normal Algebra: " + q.ToAlgebra().ToString());

            if (this._optimisers == null)
            {
                this._optimisers = new List<IAlgebraOptimiser>()
                {
                    new StrictAlgebraOptimiser(),
                    new FullTextOptimiser(new MockSearchProvider())
                };
            }
            q.AlgebraOptimisers = this._optimisers;

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine("Optimised Algebra: " + algebra);
            Assert.IsTrue(algebra.Contains("FullTextQuery("), "Optimised Algebra should use FullTextQuery operator");
            Assert.IsTrue(algebra.Contains("PropertyFunction("), "Optimised Algebra should use PropertyFunction operator");
        }

        [TestMethod]
        public void FullTextOptimiserSimple1()
        {
            this.TestOptimisation("SELECT * WHERE { ?s pf:textMatch 'value' }");
        }

        [TestMethod]
        public void FullTextOptimiserSimple2()
        {
            this.TestOptimisation("SELECT * WHERE { ?s ?p ?o . ?s pf:textMatch 'value' }");
        }

        [TestMethod]
        public void FullTextOptimiserSimple3()
        {
            this.TestOptimisation("SELECT * WHERE { ?s pf:textMatch 'value' . FILTER(ISURI(?s)) }");
        }

        [TestMethod]
        public void FullTextOptimiserSimple4()
        {
            this.TestOptimisation("SELECT * WHERE { (?match ?score) pf:textMatch 'value' }");
        }

        [TestMethod]
        public void FullTextOptimiserComplex1()
        {
            this.TestOptimisation("SELECT * WHERE { ?s ?p ?o . ?s pf:textMatch 'value' }");
        }

        [TestMethod]
        public void FullTextOptimiserComplex2()
        {
            this.TestOptimisation("SELECT * WHERE { ?s ?p ?o . FILTER(ISLITERAL(?o)) . ?s pf:textMatch 'value' }");
        }
    }

    class MockSearchProvider
        : IFullTextSearchProvider
    {

        #region IFullTextSearchProvider Members

        public IEnumerable<IFullTextSearchResult> Match(string text, double scoreThreshold, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFullTextSearchResult> Match(string text, double scoreThreshold)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFullTextSearchResult> Match(string text, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFullTextSearchResult> Match(string text)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text, double scoreThreshold, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text, double scoreThreshold)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text)
        {
            throw new NotImplementedException();
        }

        public bool IsAutoSynced
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
