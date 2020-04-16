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
#if !NO_FULLTEXT
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.PropertyFunctions;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.FullText
{

    public class FullTextHelperTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        private void TestExtractPatterns(String query, int expectedPatterns)
        {
            this.TestExtractPatterns(query, expectedPatterns, 0, 0);
        }

        private void TestExtractPatterns(String query, int expectedPatterns, int expectedSubjArgs, int expectedObjArgs)
        {
            FullTextPropertyFunctionFactory factory = new FullTextPropertyFunctionFactory();
            try
            {
                PropertyFunctionFactory.AddFactory(factory);

                SparqlParameterizedString queryString = new SparqlParameterizedString(query);
                queryString.Namespaces.AddNamespace("pf", new Uri(FullTextHelper.FullTextMatchNamespace));
                SparqlQuery q = this._parser.ParseFromString(queryString);
                SparqlFormatter formatter = new SparqlFormatter(queryString.Namespaces);

                Console.WriteLine(formatter.Format(q));
                Console.WriteLine();

                List<IPropertyFunctionPattern> ps = PropertyFunctionHelper.ExtractPatterns(q.RootGraphPattern.TriplePatterns);
                Console.WriteLine(ps.Count + " Pattern(s) extracted");
                foreach (IPropertyFunctionPattern propFunc in ps.Where(p => p.PropertyFunction is FullTextMatchPropertyFunction))
                {
                    Console.WriteLine("Match Variable: " + propFunc.SubjectArgs.First().ToString());
                    Console.WriteLine("Score Variable: " + (propFunc.SubjectArgs.Count() > 1 ? propFunc.SubjectArgs.Skip(1).First().ToString() : "N/A"));
                    Console.WriteLine("Search Term: " + propFunc.ObjectArgs.First().ToString());
                    Console.WriteLine("Threshold/Limit: " + (propFunc.ObjectArgs.Count() > 1 ? propFunc.ObjectArgs.Skip(1).First().ToString() : "N/A"));
                    Console.WriteLine("Limit: " + (propFunc.ObjectArgs.Count() > 2 ? propFunc.ObjectArgs.Skip(2).First().ToString() : "N/A"));
                    Console.WriteLine();

                    if (expectedSubjArgs > 0) Assert.Equal(expectedSubjArgs, propFunc.SubjectArgs.Count());
                    if (expectedObjArgs > 0) Assert.Equal(expectedObjArgs, propFunc.ObjectArgs.Count());
                }

                Assert.Equal(expectedPatterns, ps.Count);
            }
            finally
            {
                PropertyFunctionFactory.RemoveFactory(factory);
            }
        }

        [Fact]
        public void FullTextHelperExtractPatterns1()
        {
            this.TestExtractPatterns("SELECT * WHERE { ?s pf:textMatch 'text' }", 1, 1, 1);
        }

        [Fact]
        public void FullTextHelperExtractPatterns2()
        {
            this.TestExtractPatterns("SELECT * WHERE { ?s pf:textMatch 'text' . ?s2 pf:textMatch 'text2' }", 2, 1, 1);
        }

        [Fact]
        public void FullTextHelperExtractPatterns3()
        {
            this.TestExtractPatterns("SELECT * WHERE { (?match ?score) pf:textMatch 'text' }", 1, 2, 1);
        }

        [Fact]
        public void FullTextHelperExtractPatterns4()
        {
            this.TestExtractPatterns("SELECT * WHERE { (?match ?score) pf:textMatch 'text' . ?match2 pf:textMatch 'text2' }", 2, 0, 1);
        }

        [Fact]
        public void FullTextHelperExtractPatterns5()
        {
            this.TestExtractPatterns("SELECT * WHERE { (?match ?score) pf:textMatch 'text' . (?match2 ?score2) pf:textMatch 'text2' }", 2, 2, 1);
        }

        [Fact]
        public void FullTextHelperExtractPatterns6()
        {
            this.TestExtractPatterns("SELECT * WHERE { ?s pf:textMatch ('text' 0.75) }", 1, 1, 2);
        }

        [Fact]
        public void FullTextHelperExtractPatterns7()
        {
            this.TestExtractPatterns("SELECT * WHERE { ?s pf:textMatch ('text' 0.75 25) }", 1, 1, 3);
        }

        [Fact]
        public void FullTextHelperExtractPatterns8()
        {
            this.TestExtractPatterns("SELECT * WHERE { ?s pf:textMatch ('text' 25) }", 1, 1, 2);
        }
    }
}
#endif