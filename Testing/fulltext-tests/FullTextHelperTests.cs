using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Query.FullText
{
    [TestClass]
    public class FullTextHelperTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        private void TextExtractPatterns(String query, int expectedPatterns)
        {
            SparqlParameterizedString queryString = new SparqlParameterizedString(query);
            queryString.Namespaces.AddNamespace("pf", new Uri(FullTextHelper.FullTextMatchNamespace));
            SparqlQuery q = this._parser.ParseFromString(queryString);
            SparqlFormatter formatter = new SparqlFormatter(queryString.Namespaces);

            Console.WriteLine(formatter.Format(q));
            Console.WriteLine();

            List<FullTextPattern> ps = FullTextHelper.ExtractPatterns(q.RootGraphPattern.TriplePatterns);
            Console.WriteLine(ps.Count + " Pattern(s) extracted");
            foreach (FullTextPattern ft in ps)
            {
                Console.WriteLine("Match Variable: " + ft.MatchVariable.ToString());
                Console.WriteLine("Score Variable: " + (ft.ScoreVariable != null ? ft.ScoreVariable.ToString() : "N/A"));
                Console.WriteLine("Search Term: " + ft.SearchTerm.ToString());
                Console.WriteLine("Threshold: " + ft.ScoreThreshold.ToString());
                Console.WriteLine("Limit: " + ft.Limit.ToString());
                Console.WriteLine();
            }

            Assert.AreEqual(expectedPatterns, ps.Count);
        }

        [TestMethod]
        public void FullTextHelperExtractPatterns1()
        {
            this.TextExtractPatterns("SELECT * WHERE { ?s pf:textMatch 'text' }", 1);
        }

        [TestMethod]
        public void FullTextHelperExtractPatterns2()
        {
            this.TextExtractPatterns("SELECT * WHERE { ?s pf:textMatch 'text' . ?s2 pf:textMatch 'text2' }", 2);
        }

        [TestMethod]
        public void FullTextHelperExtractPatterns3()
        {
            this.TextExtractPatterns("SELECT * WHERE { (?match ?score) pf:textMatch 'text' }", 1);
        }

        [TestMethod]
        public void FullTextHelperExtractPatterns4()
        {
            this.TextExtractPatterns("SELECT * WHERE { (?match ?score) pf:textMatch 'text' . ?match2 pf:textMatch 'text2' }", 2);
        }

        [TestMethod]
        public void FullTextHelperExtractPatterns5()
        {
            this.TextExtractPatterns("SELECT * WHERE { (?match ?score) pf:textMatch 'text' . (?match2 ?score2) pf:textMatch 'text2' }", 2);
        }

        [TestMethod]
        public void FullTextHelperExtractPatterns6()
        {
            this.TextExtractPatterns("SELECT * WHERE { ?s pf:textMatch ('text' 0.75) }", 1);
        }

        [TestMethod]
        public void FullTextHelperExtractPatterns7()
        {
            this.TextExtractPatterns("SELECT * WHERE { ?s pf:textMatch ('text' 0.75 25) }", 1);
        }

        [TestMethod]
        public void FullTextHelperExtractPatterns8()
        {
            this.TextExtractPatterns("SELECT * WHERE { ?s pf:textMatch ('text' 25) }", 1);
        }
    }
}
