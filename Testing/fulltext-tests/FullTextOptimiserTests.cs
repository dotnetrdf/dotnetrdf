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
            Assert.IsTrue(algebra.Contains("FullTextMatch("), "Optimised Algebra should use FullTextMatch operator");
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

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
