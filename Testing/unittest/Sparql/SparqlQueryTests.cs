using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Test
{
    [TestClass]
    public class SparqlQueryTests
    {
        [TestMethod]
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
            Assert.IsTrue(algebra.InnerAlgebra is Select);
            Assert.IsTrue(((Select)algebra.InnerAlgebra).InnerAlgebra is Filter);
        }
    }
}