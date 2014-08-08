using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NUnit.Framework;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Elements;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Results;
using VDS.RDF.Query.Sorting;

namespace VDS.RDF.Query.Processors
{
    [TestFixture]
    public abstract class AbstractQueryProcessorTests
    {
        private static IGraph CreateGraph()
        {
            IGraph g = new Graph();
            g.Namespaces.AddNamespace(String.Empty, new Uri("http://test/"));

            INode s1 = g.CreateUriNode(":s");
            INode s2 = g.CreateBlankNode();
            INode p1 = g.CreateUriNode(":p1");
            INode p2 = g.CreateUriNode(":p2");
            INode o1 = g.CreateUriNode(":o");
            INode o2 = g.CreateLiteralNode("object");

            g.Assert(s1, p1, o1);
            g.Assert(s1, p1, o2);
            g.Assert(s1, p2, o1);
            g.Assert(s1, p2, o1);
            g.Assert(s1, p1, s2);

            g.Assert(s2, p1, o1);

            return g;
        }

        /// <summary>
        /// Creates a processor that can operate over the given graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        protected abstract IQueryProcessor CreateProcessor(IGraph g);

        [Test]
        public void QueryProcessorAskEmptyWhere()
        {
            IQuery query = new Query();
            query.QueryType = QueryType.Ask;

            IQueryProcessor processor = CreateProcessor(CreateGraph());
            IQueryResult result = processor.Execute(query);

            Assert.IsTrue(result.IsBoolean);
            Assert.IsTrue(result.Boolean.HasValue);
            Assert.IsTrue(result.Boolean.Value);
        }

        [Test]
        public void QueryProcessorAskWhereNoMatches()
        {
            IGraph g = CreateGraph();

            IQuery query = new Query();
            query.QueryType = QueryType.Ask;
            INode noSuchThing = g.CreateUriNode(":nosuchthing");
            Triple t = new Triple(noSuchThing, noSuchThing, noSuchThing);
            query.WhereClause = new TripleBlockElement(t.AsEnumerable());

            IQueryProcessor processor = CreateProcessor(g);
            IQueryResult result = processor.Execute(query);

            Assert.IsTrue(result.IsBoolean);
            Assert.IsTrue(result.Boolean.HasValue);
            Assert.IsFalse(result.Boolean.Value);
        }

        [Test]
        public void QueryProcessorAskWhereAnyMatches()
        {
            IGraph g = CreateGraph();

            IQuery query = new Query();
            query.QueryType = QueryType.Ask;
            Triple t = new Triple(g.CreateVariableNode("s"), g.CreateVariableNode("p"), g.CreateVariableNode("o"));
            query.WhereClause = new TripleBlockElement(t.AsEnumerable());

            IQueryProcessor processor = CreateProcessor(g);
            IQueryResult result = processor.Execute(query);

            Assert.IsTrue(result.IsBoolean);
            Assert.IsTrue(result.Boolean.HasValue);
            Assert.IsTrue(result.Boolean.Value);
        }

        [Test]
        public void QueryProcessorAskWhereConcreteMatch()
        {
            IGraph g = CreateGraph();

            IQuery query = new Query();
            query.QueryType = QueryType.Ask;
            Triple t = g.Triples.FirstOrDefault(x => x.IsGround);
            Assert.IsNotNull(t);
            query.WhereClause = new TripleBlockElement(t.AsEnumerable());

            IQueryProcessor processor = CreateProcessor(g);
            IQueryResult result = processor.Execute(query);

            Assert.IsTrue(result.IsBoolean);
            Assert.IsTrue(result.Boolean.HasValue);
            Assert.IsTrue(result.Boolean.Value);
        }

        [Test]
        public void QueryProcessorSelectEmptyWhere()
        {
            IQuery query = new Query();
            query.QueryType = QueryType.SelectAll;

            IQueryProcessor processor = CreateProcessor(CreateGraph());
            IQueryResult result = processor.Execute(query);

            Assert.IsTrue(result.IsTabular);
            IRandomAccessTabularResults results = new RandomAccessTabularResults(result.Table);
            Assert.AreEqual(1, results.Count);

            IResultRow row = results[0];
            Assert.IsTrue(row.IsEmpty);
        }

        [Test]
        public void QueryProcessorSelectWhereNoMatches()
        {
            IGraph g = CreateGraph();

            IQuery query = new Query();
            query.QueryType = QueryType.SelectAll;
            INode noSuchThing = g.CreateUriNode(":nosuchthing");
            Triple t = new Triple(noSuchThing, noSuchThing, noSuchThing);
            query.WhereClause = new TripleBlockElement(t.AsEnumerable());

            IQueryProcessor processor = CreateProcessor(g);
            IQueryResult result = processor.Execute(query);

            Assert.IsTrue(result.IsTabular);
            IRandomAccessTabularResults results = new RandomAccessTabularResults(result.Table);
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void QueryProcessorSelectWhereAnyMatches()
        {
            IGraph g = CreateGraph();

            IQuery query = new Query();
            query.QueryType = QueryType.SelectAll;
            Triple t = new Triple(g.CreateVariableNode("s"), g.CreateVariableNode("p"), g.CreateVariableNode("o"));
            query.WhereClause = new TripleBlockElement(t.AsEnumerable());

            IQueryProcessor processor = CreateProcessor(g);
            IQueryResult result = processor.Execute(query);

            Assert.IsTrue(result.IsTabular);
            IRandomAccessTabularResults results = new RandomAccessTabularResults(result.Table);
            Assert.AreEqual(g.Count, results.Count);

            Assert.IsTrue(results.All(r => r.HasBoundValue("s") && r.HasBoundValue("p") && r.HasBoundValue("o")));
        }

        [Test]
        public void QueryProcessorSelectWhereConcreteMatch()
        {
            IGraph g = CreateGraph();

            IQuery query = new Query();
            query.QueryType = QueryType.SelectAll;
            Triple t = g.Triples.FirstOrDefault(x => x.IsGround);
            Assert.IsNotNull(t);
            query.WhereClause = new TripleBlockElement(t.AsEnumerable());

            IQueryProcessor processor = CreateProcessor(g);
            IQueryResult result = processor.Execute(query);

            Assert.IsTrue(result.IsTabular);
            IRandomAccessTabularResults results = new RandomAccessTabularResults(result.Table);
            Assert.AreEqual(1, results.Count);

            IResultRow row = results[0];
            Assert.IsTrue(row.IsEmpty);
        }

        [Test]
        public void QueryProcessorProject1()
        {
            IGraph g = new Graph();

            IQuery query = new Query();
            query.QueryType = QueryType.Select;
            query.AddProjectVariable("x");

            // Form a VALUES clause for the WHERE
            INode ten = new LongNode(10);
            INode hundred = new LongNode(100);
            IMutableResultRow r1 = new MutableResultRow(new String[] { "x", "y"});
            r1.Set("x", ten);
            r1.Set("y", hundred);
            IMutableTabularResults data = new MutableTabularResults(r1.Variables, new IMutableResultRow[] { r1 });
            query.WhereClause = new DataElement(data);

            IQueryProcessor processor = CreateProcessor(g);
            IQueryResult result = processor.Execute(query);

            Assert.IsTrue(result.IsTabular);
            IRandomAccessTabularResults results = new RandomAccessTabularResults(result.Table);
            Assert.AreEqual(1, results.Count);

            IResultRow row = results[0];
            Assert.IsTrue(row.HasBoundValue("x"));
            Assert.AreEqual(ten, row["x"]);
        }

        [Test]
        public void QueryProcessorProject2()
        {
            IGraph g = new Graph();

            IQuery query = new Query();
            query.QueryType = QueryType.Select;
            query.AddProjectVariable("x");

            // Form a VALUES clause for the WHERE
            INode ten = new LongNode(10);
            INode hundred = new LongNode(100);
            IMutableResultRow r1 = new MutableResultRow(new String[] { "x", "y" });
            r1.Set("x", ten);
            r1.Set("y", hundred);
            IMutableResultRow r2 = new MutableResultRow(new String[] { "x" });
            r2.Set("x", ten);
            IMutableResultRow r3 = new MutableResultRow(new String[] { "y"});
            r3.Set("y", hundred);
            IMutableResultRow r4 = new MutableResultRow();
            IMutableTabularResults data = new MutableTabularResults(r1.Variables, new IMutableResultRow[] { r1, r2, r3, r4 });
            query.WhereClause = new DataElement(data);

            IQueryProcessor processor = CreateProcessor(g);
            IQueryResult result = processor.Execute(query);

            Assert.IsTrue(result.IsTabular);
            IRandomAccessTabularResults results = new RandomAccessTabularResults(result.Table);
            Assert.AreEqual(data.Count, results.Count);

            foreach (IResultRow row in results)
            {
                Assert.IsTrue(row.HasValue("x"));
                if (row.HasBoundValue("x"))
                {
                    Assert.AreEqual(ten, row["x"]);
                }
                else
                {
                    Assert.IsFalse(row.HasBoundValue("x"));
                }
            }
        }

        [Test]
        public void QueryProcessorOrderBy1()
        {
            IGraph g = new Graph();

            IQuery query = new Query();
            query.QueryType = QueryType.Select;
            query.SortConditions = new ISortCondition[] { new SortCondition(new VariableTerm("x")) };

            // Form a VALUES clause for the WHERE
            INode ten = new LongNode(10);
            INode hundred = new LongNode(100);
            IMutableResultRow r1 = new MutableResultRow(new String[] { "x" });
            r1.Set("x", ten);
            IMutableResultRow r2 = new MutableResultRow(new String[] { "x" });
            r2.Set("x", hundred);
            IMutableTabularResults data = new MutableTabularResults(r1.Variables, new IMutableResultRow[] { r1, r2 });
            query.WhereClause = new DataElement(data);

            IQueryProcessor processor = CreateProcessor(g);
            IQueryResult result = processor.Execute(query);

            Assert.IsTrue(result.IsTabular);
            IRandomAccessTabularResults results = new RandomAccessTabularResults(result.Table);
            Assert.AreEqual(data.Count, results.Count);

            IResultRow first = results[0];
            IResultRow second = results[1];
            Assert.IsTrue(first.HasBoundValue("x"));
            Assert.IsTrue(second.HasBoundValue("x"));
            Assert.AreEqual(ten, first["x"]);
            Assert.AreEqual(hundred, second["x"]);
        }

        [Test]
        public void QueryProcessorOrderBy2()
        {
            IGraph g = new Graph();

            IQuery query = new Query();
            query.QueryType = QueryType.Select;
            query.SortConditions = new ISortCondition[] { new SortCondition(new VariableTerm("x"), false) };

            // Form a VALUES clause for the WHERE
            INode ten = new LongNode(10);
            INode hundred = new LongNode(100);
            IMutableResultRow r1 = new MutableResultRow(new String[] { "x" });
            r1.Set("x", ten);
            IMutableResultRow r2 = new MutableResultRow(new String[] { "x" });
            r2.Set("x", hundred);
            IMutableTabularResults data = new MutableTabularResults(r1.Variables, new IMutableResultRow[] { r1, r2 });
            query.WhereClause = new DataElement(data);

            IQueryProcessor processor = CreateProcessor(g);
            IQueryResult result = processor.Execute(query);

            Assert.IsTrue(result.IsTabular);
            IRandomAccessTabularResults results = new RandomAccessTabularResults(result.Table);
            Assert.AreEqual(data.Count, results.Count);

            IResultRow first = results[0];
            IResultRow second = results[1];
            Assert.IsTrue(first.HasBoundValue("x"));
            Assert.IsTrue(second.HasBoundValue("x"));
            Assert.AreEqual(hundred, first["x"]);
            Assert.AreEqual(ten, second["x"]);
        }
    }
}
