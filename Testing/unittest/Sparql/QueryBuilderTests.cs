using System.Linq;
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
            Assert.Inconclusive("Need to refator the API");

            //IQueryBuilder builder = QueryBuilder.SelectAll();
            //builder.Where("s", "p", "o")
            //     .Optional(builder.CreateVariableNode("s"), builder.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType)), builder.CreateVariableNode("type"));

            //var q = builder.GetExecutableQuery();
            //Assert.AreEqual(SparqlQueryType.SelectAll, q.QueryType);
            //Assert.IsNotNull(q.RootGraphPattern);
            //Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count());
            //Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns.Count);
            //Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns[0].IsOptional);
        }

        [TestMethod]
        public void SparqlBuilderSelect5()
        {
            Assert.Inconclusive("Need to refator the API");

            //IQueryBuilder builder = QueryBuilder.SelectAll();
            //builder.Where("s", "p", "o")
            //     .Optional(builder.CreateVariableNode("s"), builder.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType)), builder.CreateVariableNode("type"))
            //     .Where("x", "y", "z");

            //var q = builder.GetExecutableQuery();
            //Assert.AreEqual(SparqlQueryType.SelectAll, q.QueryType);
            //Assert.IsNotNull(q.RootGraphPattern);
            //Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count());
            //Assert.AreEqual(2, q.RootGraphPattern.ChildGraphPatterns.Count);
            //Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns[0].IsOptional);
            //Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns[1].TriplePatterns.Count());
        }

        [TestMethod]
        public void GetExectuableQueryReturnsNewInstance()
        {
            // given
            IQueryBuilder builder = QueryBuilder.SelectAll().Where("s", "p", "o");

            // when
            SparqlQuery query1 = builder.GetExecutableQuery();
            SparqlQuery query2 = builder.GetExecutableQuery();

            // then
            Assert.AreNotSame(query1, query2);
        }

        [TestMethod]
        public void CanStartQueryWithGivenVariablesStrings()
        {
            // given
            IQueryBuilder queryBuilder = QueryBuilder.Select("s", "p", "o");

            // when
            SparqlQuery query = queryBuilder.GetExecutableQuery();

            // then
            Assert.AreEqual(3, query.Variables);
            Assert.IsTrue(query.Variables.Contains(new SparqlVariable("s")));
            Assert.IsTrue(query.Variables.Contains(new SparqlVariable("p")));
            Assert.IsTrue(query.Variables.Contains(new SparqlVariable("o")));
            Assert.IsTrue(query.Variables.All(var => var.IsResultVariable));
        }

        [TestMethod]
        public void CanStartQueryWithGivenVariables()
        {
            // given
            var s = new SparqlVariable("s", true);
            var p = new SparqlVariable("p", true);
            var o = new SparqlVariable("o", true);
            IQueryBuilder queryBuilder = QueryBuilder.Select(s, p ,o);

            // when
            SparqlQuery query = queryBuilder.GetExecutableQuery();

            // then
            Assert.AreEqual(3, query.Variables.Count());
            Assert.IsTrue(query.Variables.Contains(s));
            Assert.IsTrue(query.Variables.Contains(p));
            Assert.IsTrue(query.Variables.Contains(o));
        }
    }
}
