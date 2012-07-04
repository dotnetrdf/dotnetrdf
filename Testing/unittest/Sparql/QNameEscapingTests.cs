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
    public class QNameEscapingTests
    {
        List<char> _cs = new List<char>()
            {
                 '_',
                 '-',
                 '.',
                 '|',
                 '$',
                 '&',
                 '\'',
                 '(',
                 ')',
                 '*',
                 '+',
                 ',',
                 ';',
                 '=',
                 ',',
                 '/',
                 '?',
                 '#',
                 '@',
                 '%'
            };

        private void TestQNameUnescaping(String input, String expected)
        {
            Console.WriteLine("Input = " + input);
            Assert.IsFalse(SparqlSpecsHelper.IsValidQName(input, SparqlQuerySyntax.Sparql_1_0), "Expected QName to be invalid in SPARQL 1.0 syntax");
            Assert.IsTrue(SparqlSpecsHelper.IsValidQName(input, SparqlQuerySyntax.Sparql_1_1), "Expected a Valid QName as test input");

            Console.WriteLine("Output = " + SparqlSpecsHelper.UnescapeQName(input));
            Console.WriteLine("Expected = " + expected);
            Assert.AreEqual(expected, SparqlSpecsHelper.UnescapeQName(input));
        }

        [TestMethod]
        public void SparqlQNameUnescapingPercentEncodingSimple()
        {
            this.TestQNameUnescaping("ex:a%20space", "ex:a space");
        }

        [TestMethod]
        public void SparqlQNameUnescapingPercentEncodingComplex()
        {
            for (int i = 0; i <= 255; i++)
            {
                this.TestQNameUnescaping("ex:" + Uri.HexEscape((char)i), "ex:" + (char)i);
            }
        }

        [TestMethod]
        public void SparqlQNameUnescapingSpecialCharactersSimple()
        {
            foreach (char c in this._cs)
            {
                this.TestQNameUnescaping("ex:a\\" + c + "character", "ex:a" + c + "character");
            }
        }

        [TestMethod]
        public void SparqlQNameUnescapingSpecialCharactersComplex()
        {
            foreach (char c in this._cs)
            {
                this.TestQNameUnescaping("ex:\\" + c, "ex:" + c);
            }
        }

        [TestMethod]
        public void SparqlQNameUnescapingSpecialCharactersMultiple()
        {
            StringBuilder input = new StringBuilder();
            StringBuilder output = new StringBuilder();
            input.Append("ex:");
            output.Append("ex:");
            foreach (char c in this._cs)
            {
                input.Append('\\');
                input.Append(c);
                output.Append(c);
            }
            this.TestQNameUnescaping(input.ToString(), output.ToString());
        }

        [TestMethod]
        public void SparqlQNameUnescapingDawg1()
        {
            this.TestQNameUnescaping("og:audio:title", "og:audio:title");
        }
    }
}
