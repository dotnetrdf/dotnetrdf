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
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query
{

    public class SparqlQueryTests
    {
        [Fact]
        public void SparqlFilterShouldGetPlaced()
        {
            // given
            var query = new SparqlQuery { QueryType = SparqlQueryType.SelectDistinct };
            query.AddVariable(new SparqlVariable("s", true));
            query.RootGraphPattern = new GraphPattern();
            var subj = new VariablePattern("s");
            var rdfType = new NodeMatchPattern(new UriNode(null, new Uri(RdfSpecsHelper.RdfType)));
            var type = new VariablePattern("type");
            var triplePattern = new TriplePattern(subj, rdfType, type);
            query.RootGraphPattern.AddTriplePattern(triplePattern);
            query.RootGraphPattern.AddFilter(new UnaryExpressionFilter(new InFunction(new VariableTerm("type"), new[]
                {
                    new ConstantTerm(new UriNode(null, new Uri("http://example.com/Type1"))), 
                    new ConstantTerm(new UriNode(null, new Uri("http://example.com/Type2"))), 
                    new ConstantTerm(new UriNode(null, new Uri("http://example.com/Type3")))
                })));

            // when
            var algebra = (Distinct)query.ToAlgebra();

            // then
            Assert.True(algebra.InnerAlgebra is Select);
            Assert.True(((Select)algebra.InnerAlgebra).InnerAlgebra is Filter);
        }

        [Fact]
        public void SparqlJoinExplosion()
        {
            IGraph g = new Graph();
            g.LoadFromFile("resources\\LearningStyles.rdf");
            SparqlQuery query = new SparqlQueryParser().ParseFromFile("resources\\learning-problem.rq");

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(new InMemoryDataset(g));
            SparqlResultSet results = processor.ProcessQuery(query) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.False(results.IsEmpty);
            Assert.Equal(176, results.Count);
        }
    }
}