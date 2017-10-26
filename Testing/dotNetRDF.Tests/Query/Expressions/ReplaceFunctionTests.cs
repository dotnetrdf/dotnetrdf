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

using Xunit;
using System;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions
{

    public class ReplaceFunctionTests
    {
        [Fact]
        public void SparqlParsingReplaceExpression()
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT (REPLACE(?term, 'find', 'replace') AS ?test) { }");

            ISparqlExpression expr = q.Variables.First().Projection;
            Assert.IsType<ReplaceFunction>(expr);
        }

        [Fact]
        public void SparqlExpressionsXPathReplaceNullInCanParallelise1()
        {
            // when
            var find = new ConstantTerm(new StringNode(null, "find"));
            var replace = new VariableTerm("replacement");
            ReplaceFunction func = new ReplaceFunction(new VariableTerm("term"), find, replace);

            // then
            var canParallelise = func.CanParallelise;
        }

        [Fact]
        public void SparqlExpressionsXPathReplaceNullInCanParallelise2()
        {
            // when
            var find = new VariableTerm("find");
            var replace = new ConstantTerm(new StringNode(null, "replacement"));
            ReplaceFunction func = new ReplaceFunction(new VariableTerm("term"), find, replace);

            // then
            var canParallelise = func.CanParallelise;
        }
    }
}