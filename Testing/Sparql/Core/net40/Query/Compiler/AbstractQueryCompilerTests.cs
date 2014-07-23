using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Elements;

namespace VDS.RDF.Query.Compiler
{
    [TestFixture]
    public abstract class AbstractQueryCompilerTests
    {
        /// <summary>
        /// Creates a new query compiler instance to use for testing
        /// </summary>
        /// <returns></returns>
        protected abstract IQueryCompiler CreateInstance();

        [Test]
        public void QueryCompilerEmptyBgp()
        {
            IQueryCompiler compiler = this.CreateInstance();

            IQuery query = new Query();
            query.WhereClause = new TripleBlockElement(Enumerable.Empty<Triple>());

            IAlgebra algebra = compiler.Compile(query);
            Console.WriteLine(algebra.ToString());
            Assert.IsInstanceOf(typeof(Table), algebra);

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
            Assert.IsInstanceOf(typeof(Bgp), algebra);

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
            Assert.IsInstanceOf(typeof(Union), algebra);

            Union u = (Union) algebra;
            Assert.IsInstanceOf(typeof(Bgp), u.Lhs);
            Assert.IsInstanceOf(typeof(Bgp), u.Rhs);
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
            Assert.IsInstanceOf(typeof(Union), algebra);

            Union u = (Union)algebra;
            Assert.IsInstanceOf(typeof(Bgp), u.Lhs);

            Bgp bgp = (Bgp) u.Lhs;
            Assert.AreEqual(1, bgp.TriplePatterns.Count);
            Assert.IsTrue(bgp.TriplePatterns.Contains(t1));

            Assert.IsInstanceOf(typeof(Union), u.Rhs);
            u = (Union) u.Rhs;
            Assert.IsInstanceOf(typeof(Bgp), u.Lhs);

            bgp = (Bgp) u.Lhs;
            Assert.AreEqual(1, bgp.TriplePatterns.Count);
            Assert.IsTrue(bgp.TriplePatterns.Contains(t2));

            bgp = (Bgp) u.Rhs;
            Assert.AreEqual(1, bgp.TriplePatterns.Count);
            Assert.IsTrue(bgp.TriplePatterns.Contains(t3));
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
