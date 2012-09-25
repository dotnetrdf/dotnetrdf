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
    public class QueryBuilderTests
    {
        [TestMethod]
        public void SparqlBuilderSelect1()
        {
            SparqlQuery q = QueryBuilder
                .SelectAll()
                .GetExecutableQuery();
            Assert.AreEqual(SparqlQueryType.SelectAll, q.QueryType);
            Assert.IsNull(q.RootGraphPattern);
        }

        [TestMethod]
        public void SparqlBuilderSelect2()
        {
            SparqlQuery q = QueryBuilder
                .SelectAll()
                .Distinct().GetExecutableQuery();
            Assert.AreEqual(SparqlQueryType.SelectAllDistinct, q.QueryType);
            Assert.IsTrue(q.HasDistinctModifier);
            Assert.IsNull(q.RootGraphPattern);
        }


        [TestMethod]
        public void SparqlBuilderSelect3()
        {
            SparqlQuery q = QueryBuilder
                .SelectAll()
                .Where("s", "p", "o")
                .GetExecutableQuery();
            Assert.AreEqual(SparqlQueryType.SelectAll, q.QueryType);
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count());
        }

        [TestMethod]
        public void SparqlBuilderSelect4()
        {
            IQueryBuilder builder = QueryBuilder.SelectAll();
            builder.Where("s", "p", "o")
                 .Optional(builder.CreateVariableNode("s"), builder.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType)), builder.CreateVariableNode("type"));

            var q = builder.GetExecutableQuery();
            Assert.AreEqual(SparqlQueryType.SelectAll, q.QueryType);
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count());
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns.Count);
            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns[0].IsOptional);
        }

        [TestMethod]
        public void SparqlBuilderSelect5()
        {
            IQueryBuilder builder = QueryBuilder.SelectAll();
            builder.Where("s", "p", "o")
                 .Optional(builder.CreateVariableNode("s"), builder.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType)), builder.CreateVariableNode("type"))
                 .Where("x", "y", "z");

            var q = builder.GetExecutableQuery();
            Assert.AreEqual(SparqlQueryType.SelectAll, q.QueryType);
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count());
            Assert.AreEqual(2, q.RootGraphPattern.ChildGraphPatterns.Count);
            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns[0].IsOptional);
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns[1].TriplePatterns.Count());
        }
    }
}
