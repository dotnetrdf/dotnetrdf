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

namespace VDS.RDF.Query
{

    public class QNameEscapingTests
    {
        List<char> _cs = new List<char>()
            {
                 '_',
                 '~',
                 '-',
                 '.',
                 '!',
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
            Assert.False(SparqlSpecsHelper.IsValidQName(input, SparqlQuerySyntax.Sparql_1_0), "Expected QName to be invalid in SPARQL 1.0 syntax");
            Assert.True(SparqlSpecsHelper.IsValidQName(input, SparqlQuerySyntax.Sparql_1_1), "Expected a Valid QName as test input");

            Console.WriteLine("Output = " + SparqlSpecsHelper.UnescapeQName(input));
            Console.WriteLine("Expected = " + expected);
            Assert.Equal(expected, SparqlSpecsHelper.UnescapeQName(input));
        }

        [Fact]
        public void SparqlQNameUnescapingPercentEncodingSimple()
        {
            this.TestQNameUnescaping("ex:a%20space", "ex:a%20space");
        }

#if NET40
        [Fact]
        public void SparqlQNameUnescapingPercentEncodingComplex()
        {
            for (int i = 0; i <= 255; i++)
            {
                this.TestQNameUnescaping("ex:" + Uri.HexEscape((char)i), "ex:" + Uri.HexEscape((char)i));
            }
        }
#endif

        [Fact]
        public void SparqlQNameUnescapingSpecialCharactersSimple()
        {
            foreach (char c in this._cs)
            {
                this.TestQNameUnescaping("ex:a\\" + c + "character", "ex:a" + c + "character");
            }
        }

        [Fact]
        public void SparqlQNameUnescapingSpecialCharactersComplex()
        {
            foreach (char c in this._cs)
            {
                this.TestQNameUnescaping("ex:\\" + c, "ex:" + c);
            }
        }

        [Fact]
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

        [Fact]
        public void SparqlQNameUnescapingDawg1()
        {
            this.TestQNameUnescaping("og:audio:title", "og:audio:title");
        }

        [Fact]
        public void SparqlQNameUnescapingDawg2()
        {
            this.TestQNameUnescaping(@":c\~z\.", ":c~z.");
        }

        [Fact]
        public void SparqlBNodeValidation1()
        {
            Assert.True(SparqlSpecsHelper.IsValidBNode("_:a"));
        }
    }
}
