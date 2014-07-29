using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Elements;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Results;

namespace VDS.RDF.Query.Compiler
{
    [TestFixture]
    public abstract class AbstractQueryCompilerTests
    {
        protected INodeFactory NodeFactory { get; set; }

        [SetUp]
        public void Setup()
        {
            if (this.NodeFactory == null) this.NodeFactory = new NodeFactory();
        }

        /// <summary>
        /// Creates a new query compiler instance to use for testing
        /// </summary>
        /// <returns></returns>
        protected abstract IQueryCompiler CreateInstance();

        [Test]
        public void QueryCompilerEmptyWhere()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Table), algebra);

            Table table = (Table) algebra;
            Assert.IsTrue(table.IsUnit);
        }

        [Test]
        public void QueryCompilerEmptyBgp()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            query.WhereClause = new TripleBlockElement(Enumerable.Empty<Triple>());

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Table), algebra);

            Table table = (Table) algebra;
            Assert.IsTrue(table.IsUnit);
        }

        [Test]
        public void QueryCompilerBgp()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            Triple t = new Triple(new VariableNode("s"), new VariableNode("p"), new VariableNode("o"));
            query.WhereClause = new TripleBlockElement(t.AsEnumerable());

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Bgp), algebra);

            Bgp bgp = (Bgp) algebra;
            Assert.AreEqual(1, bgp.TriplePatterns.Count);
            Assert.IsTrue(bgp.TriplePatterns.Contains(t));
        }

        [Test]
        public void QueryCompilerUnion1()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            Triple t = new Triple(new VariableNode("s"), new VariableNode("p"), new VariableNode("o"));
            IElement triples = new TripleBlockElement(t.AsEnumerable());

            IElement union = new UnionElement(triples.AsEnumerable().Concat(triples.AsEnumerable()));
            query.WhereClause = union;

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Union), algebra);

            Union u = (Union) algebra;
            Assert.IsInstanceOf(typeof (Bgp), u.Lhs);
            Assert.IsInstanceOf(typeof (Bgp), u.Rhs);
        }

        [Test]
        public void QueryCompilerUnion2()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            Triple t1 = new Triple(new VariableNode("a"), new VariableNode("b"), new VariableNode("c"));
            Triple t2 = new Triple(new VariableNode("d"), new VariableNode("e"), new VariableNode("f"));
            Triple t3 = new Triple(new VariableNode("g"), new VariableNode("h"), new VariableNode("i"));
            IElement triples1 = new TripleBlockElement(t1.AsEnumerable());
            IElement triples2 = new TripleBlockElement(t2.AsEnumerable());
            IElement triples3 = new TripleBlockElement(t3.AsEnumerable());
            IElement[] elements = {triples1, triples2, triples3};

            IElement union = new UnionElement(elements);
            query.WhereClause = union;

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Union), algebra);

            Union u = (Union) algebra;
            Assert.IsInstanceOf(typeof (Bgp), u.Lhs);

            Bgp bgp = (Bgp) u.Lhs;
            Assert.AreEqual(1, bgp.TriplePatterns.Count);
            Assert.IsTrue(bgp.TriplePatterns.Contains(t1));

            Assert.IsInstanceOf(typeof (Union), u.Rhs);
            u = (Union) u.Rhs;
            Assert.IsInstanceOf(typeof (Bgp), u.Lhs);

            bgp = (Bgp) u.Lhs;
            Assert.AreEqual(1, bgp.TriplePatterns.Count);
            Assert.IsTrue(bgp.TriplePatterns.Contains(t2));

            bgp = (Bgp) u.Rhs;
            Assert.AreEqual(1, bgp.TriplePatterns.Count);
            Assert.IsTrue(bgp.TriplePatterns.Contains(t3));
        }

        [Test]
        public void QueryCompilerInlineEmptyValues()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            IMutableTabularResults data = new MutableTabularResults(Enumerable.Empty<String>(), Enumerable.Empty<IMutableResultRow>());
            query.WhereClause = new DataElement(data);

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Table), algebra);

            Table table = (Table) algebra;
            Assert.IsTrue(table.IsEmpty);
        }

        [Test]
        public void QueryCompilerInlineValues1()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            IMutableTabularResults data = new MutableTabularResults("x".AsEnumerable(), Enumerable.Empty<IMutableResultRow>());
            data.Add(new MutableResultRow("x".AsEnumerable(), new Dictionary<string, INode> {{"x", 1.ToLiteral(this.NodeFactory)}}));
            query.WhereClause = new DataElement(data);

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Table), algebra);

            Table table = (Table) algebra;
            Assert.IsFalse(table.IsEmpty);
            Assert.IsFalse(table.IsUnit);

            Assert.AreEqual(1, table.Data.Count);
            Assert.IsTrue(table.Data.All(s => s.ContainsVariable("x")));
        }

        [Test]
        public void QueryCompilerInlineValues2()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            IMutableTabularResults data = new MutableTabularResults(new String[] {"x", "y"}, Enumerable.Empty<IMutableResultRow>());
            data.Add(new MutableResultRow("x".AsEnumerable(), new Dictionary<string, INode> {{"x", 1.ToLiteral(this.NodeFactory)}}));
            data.Add(new MutableResultRow("y".AsEnumerable(), new Dictionary<string, INode> {{"y", 2.ToLiteral(this.NodeFactory)}}));
            data.Add(new MutableResultRow(data.Variables, new Dictionary<string, INode> {{"x", 3.ToLiteral(this.NodeFactory)}, {"y", 4.ToLiteral(this.NodeFactory)}}));
            query.WhereClause = new DataElement(data);

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Table), algebra);

            Table table = (Table) algebra;
            Assert.IsFalse(table.IsEmpty);
            Assert.IsFalse(table.IsUnit);

            Assert.AreEqual(3, table.Data.Count);
            Assert.IsTrue(table.Data.All(s => s.ContainsVariable("x") || s.ContainsVariable("y")));
            Assert.IsFalse(table.Data.All(s => s.ContainsVariable("x") && s.ContainsVariable("y")));
            Assert.IsTrue(table.Data.Any(s => s.ContainsVariable("x") && s.ContainsVariable("y")));
        }

        [Test]
        public void QueryCompilerValues1()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            IMutableTabularResults data = new MutableTabularResults("x".AsEnumerable(), Enumerable.Empty<IMutableResultRow>());
            data.Add(new MutableResultRow("x".AsEnumerable(), new Dictionary<string, INode> {{"x", 1.ToLiteral(this.NodeFactory)}}));
            query.ValuesClause = data;

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Table), algebra);

            Table table = (Table) algebra;
            Assert.IsFalse(table.IsEmpty);
            Assert.IsFalse(table.IsUnit);

            Assert.AreEqual(1, table.Data.Count);
            Assert.IsTrue(table.Data.All(s => s.ContainsVariable("x")));
        }

        [Test]
        public void QueryCompilerValues2()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            IMutableTabularResults data = new MutableTabularResults(new String[] {"x", "y"}, Enumerable.Empty<IMutableResultRow>());
            data.Add(new MutableResultRow("x".AsEnumerable(), new Dictionary<string, INode> {{"x", 1.ToLiteral(this.NodeFactory)}}));
            data.Add(new MutableResultRow("y".AsEnumerable(), new Dictionary<string, INode> {{"y", 2.ToLiteral(this.NodeFactory)}}));
            data.Add(new MutableResultRow(data.Variables, new Dictionary<string, INode> {{"x", 3.ToLiteral(this.NodeFactory)}, {"y", 4.ToLiteral(this.NodeFactory)}}));
            query.ValuesClause = data;

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Table), algebra);

            Table table = (Table) algebra;
            Assert.IsFalse(table.IsEmpty);
            Assert.IsFalse(table.IsUnit);

            Assert.AreEqual(3, table.Data.Count);
            Assert.IsTrue(table.Data.All(s => s.ContainsVariable("x") || s.ContainsVariable("y")));
            Assert.IsFalse(table.Data.All(s => s.ContainsVariable("x") && s.ContainsVariable("y")));
            Assert.IsTrue(table.Data.Any(s => s.ContainsVariable("x") && s.ContainsVariable("y")));
        }

        [Test]
        public void QueryCompilerEmptyValues()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            IMutableTabularResults data = new MutableTabularResults(Enumerable.Empty<String>(), Enumerable.Empty<IMutableResultRow>());
            query.ValuesClause = data;

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Table), algebra);

            Table table = (Table) algebra;
            Assert.IsTrue(table.IsEmpty);
        }

        [TestCase(0),
         TestCase(100),
         TestCase(Int64.MaxValue),
         TestCase(-1),
         TestCase(Int64.MinValue)]
        public void QueryCompilerLimit(long limit)
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            query.Limit = limit;
            Assert.IsTrue(limit >= 0L ? query.HasLimit : !query.HasLimit);

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());

            if (limit >= 0L)
            {
                Assert.IsInstanceOf(typeof (Slice), algebra);

                Slice slice = (Slice) algebra;
                Assert.AreEqual(limit, slice.Limit);
                Assert.AreEqual(0L, slice.Offset);
            }
            else
            {
                Assert.IsInstanceOf(typeof (Table), algebra);

                Table table = (Table) algebra;
                Assert.IsTrue(table.IsUnit);
            }
        }

        [TestCase(0),
         TestCase(100),
         TestCase(Int64.MaxValue),
         TestCase(-1),
         TestCase(Int64.MinValue)]
        public void QueryCompilerOffset(long offset)
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            query.Offset = offset;
            Assert.IsTrue(offset > 0L ? query.HasOffset : !query.HasOffset);

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());

            if (offset > 0L)
            {
                Assert.IsInstanceOf(typeof (Slice), algebra);

                Slice slice = (Slice) algebra;
                Assert.AreEqual(offset, slice.Offset);
                Assert.AreEqual(-1L, slice.Limit);
            }
            else
            {
                Assert.IsInstanceOf(typeof (Table), algebra);

                Table table = (Table) algebra;
                Assert.IsTrue(table.IsUnit);
            }
        }

        [TestCase(0, 0),
         TestCase(100, 0),
         TestCase(100, 5000),
         TestCase(Int64.MaxValue, 0),
         TestCase(0, Int64.MaxValue),
         TestCase(-1, -1),
         TestCase(-1, 100),
         TestCase(Int64.MinValue, 0),
         TestCase(0, Int64.MinValue)]
        public void QueryCompilerLimitOffset(long limit, long offset)
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            query.Limit = limit;
            query.Offset = offset;
            Assert.IsTrue(limit >= 0L ? query.HasLimit : !query.HasLimit);
            Assert.IsTrue(offset > 0L ? query.HasOffset : !query.HasOffset);

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());

            if (limit >= 0L || offset > 0L)
            {
                Assert.IsInstanceOf(typeof (Slice), algebra);

                Slice slice = (Slice) algebra;
                Assert.AreEqual(limit >= 0L ? limit : -1L, slice.Limit);
                Assert.AreEqual(offset > 0L ? offset : 0L, slice.Offset);
            }
            else
            {
                Assert.IsInstanceOf(typeof (Table), algebra);

                Table table = (Table) algebra;
                Assert.IsTrue(table.IsUnit);
            }
        }

        [TestCase(QueryType.SelectAllDistinct),
         TestCase(QueryType.SelectDistinct)]
        public void QueryCompilerDistinct(QueryType type)
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            query.QueryType = type;

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());

            Assert.IsInstanceOf(typeof (Distinct), algebra);
        }

        [TestCase(QueryType.SelectAllReduced),
         TestCase(QueryType.SelectReduced)]
        public void QueryCompilerReduced(QueryType type)
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            query.QueryType = type;

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());

            Assert.IsInstanceOf(typeof (Reduced), algebra);
        }

        [TestCase("http://example.org", false),
         TestCase("http://example.org", true),
         TestCase("http://foo.bar/faz", false)]
        public void QueryCompilerService(String endpoint, bool silent)
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            Uri endpointUri = new Uri(endpoint);
            query.WhereClause = new ServiceElement(new TripleBlockElement(), endpointUri, silent);

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Service), algebra);

            Service service = (Service) algebra;
            Assert.AreEqual(silent, service.IsSilent);
            Assert.IsTrue(EqualityHelper.AreUrisEqual(endpointUri, service.EndpointUri));
        }

        [Test]
        public void QueryCompilerBind1()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            IExpression expr = new ConstantTerm(true.ToLiteral(this.NodeFactory));
            query.WhereClause = new BindElement(new KeyValuePair<String, IExpression>("x", expr).AsEnumerable());

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Extend), algebra);

            Extend extend = (Extend) algebra;
            Assert.AreEqual(1, extend.Assignments.Count);
            Assert.AreEqual("x", extend.Assignments[0].Key);
            Assert.AreEqual(expr, extend.Assignments[0].Value);
        }

        [Test]
        public void QueryCompilerBind2()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            IExpression expr1 = new ConstantTerm(true.ToLiteral(this.NodeFactory));
            IExpression expr2 = new ConstantTerm(false.ToLiteral(this.NodeFactory));
            IElement[] elements =
            {
                new BindElement(new KeyValuePair<String, IExpression>("x", expr1).AsEnumerable()), new BindElement(new KeyValuePair<String, IExpression>("y", expr2).AsEnumerable())
            };
            query.WhereClause = new GroupElement(elements);

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Extend), algebra);

            Extend extend = (Extend) algebra;
            Assert.AreEqual(2, extend.Assignments.Count);
            Assert.AreEqual("x", extend.Assignments[0].Key);
            Assert.AreEqual(expr1, extend.Assignments[0].Value);
            Assert.AreEqual("y", extend.Assignments[1].Key);
            Assert.AreEqual(expr2, extend.Assignments[1].Value);
        }

        [Test]
        public void QueryCompilerFilter1()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            IExpression expr = new ConstantTerm(true.ToLiteral(this.NodeFactory));
            query.WhereClause = new FilterElement(expr.AsEnumerable());

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Filter), algebra);

            Filter filter = (Filter) algebra;
            Assert.AreEqual(1, filter.Expressions.Count);
            Assert.AreEqual(expr, filter.Expressions[0]);
        }

        [Test]
        public void QueryCompilerMinus1()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            Triple t = new Triple(new VariableNode("s"), new VariableNode("p"), new VariableNode("o"));
            TripleBlockElement triples = new TripleBlockElement(t.AsEnumerable());
            query.WhereClause = new MinusElement(triples);

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof (Minus), algebra);

            Minus minus = (Minus) algebra;
            Assert.IsInstanceOf(typeof(Table), minus.Lhs);
            Assert.IsInstanceOf(typeof(Bgp), minus.Rhs);

            Table lhs = (Table) minus.Lhs;
            Assert.IsTrue(lhs.IsUnit);

            Bgp rhs = (Bgp) minus.Rhs;
            Assert.AreEqual(1, rhs.TriplePatterns.Count);
            Assert.IsTrue(rhs.TriplePatterns.Contains(t));
        }

        [Test]
        public void QueryCompilerMinus2()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            Triple t1 = new Triple(new VariableNode("s"), new VariableNode("p"), new VariableNode("o"));
            Triple t2 = new Triple(new VariableNode("s"), new BlankNode(Guid.NewGuid()), new LiteralNode("test"));
            TripleBlockElement matchTriples = new TripleBlockElement(t1.AsEnumerable());
            TripleBlockElement minusTriples = new TripleBlockElement(t2.AsEnumerable());
            query.WhereClause = new GroupElement(new IElement[] { matchTriples, new MinusElement(minusTriples) });

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof(Minus), algebra);

            Minus minus = (Minus)algebra;
            Assert.IsInstanceOf(typeof(Bgp), minus.Lhs);
            Assert.IsInstanceOf(typeof(Bgp), minus.Rhs);

            Bgp lhs = (Bgp)minus.Lhs;
            Assert.AreEqual(1, lhs.TriplePatterns.Count);
            Assert.IsTrue(lhs.TriplePatterns.Contains(t1));

            Bgp rhs = (Bgp)minus.Rhs;
            Assert.AreEqual(1, rhs.TriplePatterns.Count);
            Assert.IsTrue(rhs.TriplePatterns.Contains(t2));
        }

        [Test]
        public void QueryCompilerGroup1()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            Triple t = new Triple(new VariableNode("s"), new VariableNode("p"), new VariableNode("o"));
            IMutableTabularResults data = new MutableTabularResults("x".AsEnumerable(), Enumerable.Empty<IMutableResultRow>());
            data.Add(new MutableResultRow("x".AsEnumerable(), new Dictionary<string, INode> {{"x", 1.ToLiteral(this.NodeFactory)}}));
            TripleBlockElement tripleBlock = new TripleBlockElement(t.AsEnumerable());
            DataElement inlineData = new DataElement(data);
            query.WhereClause = new GroupElement(new IElement[] {tripleBlock, inlineData});

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());

            Assert.IsInstanceOf(typeof (Join), algebra);
            Join join = (Join) algebra;

            Assert.IsInstanceOf(typeof (Bgp), join.Lhs);
            Bgp bgp = (Bgp) join.Lhs;
            Assert.AreEqual(1, bgp.TriplePatterns.Count);
            Assert.IsTrue(bgp.TriplePatterns.Contains(t));

            Assert.IsInstanceOf(typeof (Table), join.Rhs);
            Table table = (Table) join.Rhs;
            Assert.IsFalse(table.IsEmpty);
            Assert.IsFalse(table.IsUnit);

            Assert.AreEqual(1, table.Data.Count);
            Assert.IsTrue(table.Data.All(s => s.ContainsVariable("x")));
        }


        [Test]
        public void QueryCompilerGroup2()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            Triple t = new Triple(new VariableNode("s"), new VariableNode("p"), new VariableNode("o"));
            TripleBlockElement tripleBlock = new TripleBlockElement(t.AsEnumerable());

            IMutableTabularResults data = new MutableTabularResults("x".AsEnumerable(), Enumerable.Empty<IMutableResultRow>());
            data.Add(new MutableResultRow("x".AsEnumerable(), new Dictionary<string, INode> {{"x", 1.ToLiteral(this.NodeFactory)}}));
            DataElement inlineData = new DataElement(data);

            IExpression expr = new ConstantTerm(true.ToLiteral(this.NodeFactory));
            FilterElement filter = new FilterElement(expr.AsEnumerable());

            query.WhereClause = new GroupElement(new IElement[] {tripleBlock, filter, inlineData});

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());

            Assert.IsInstanceOf(typeof (Filter), algebra);
            Filter f = (Filter) algebra;
            Assert.AreEqual(1, f.Expressions.Count);
            Assert.AreEqual(expr, f.Expressions[0]);

            Assert.IsInstanceOf(typeof (Join), f.InnerAlgebra);
            Join join = (Join) f.InnerAlgebra;

            Assert.IsInstanceOf(typeof (Bgp), join.Lhs);
            Bgp bgp = (Bgp) join.Lhs;
            Assert.AreEqual(1, bgp.TriplePatterns.Count);
            Assert.IsTrue(bgp.TriplePatterns.Contains(t));

            Assert.IsInstanceOf(typeof (Table), join.Rhs);
            Table table = (Table) join.Rhs;
            Assert.IsFalse(table.IsEmpty);
            Assert.IsFalse(table.IsUnit);

            Assert.AreEqual(1, table.Data.Count);
            Assert.IsTrue(table.Data.All(s => s.ContainsVariable("x")));
        }
    }

    /// <summary>
    /// Tests for the <see cref="DefaultQueryCompiler"/>
    /// </summary>
    public class DefaultQueryCompilerTests
        : AbstractQueryCompilerTests
    {
        protected override IQueryCompiler CreateInstance()
        {
            return new DefaultQueryCompiler();
        }
    }
}