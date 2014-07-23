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
            Assert.IsInstanceOf(typeof(Bgp), algebra);

            Bgp bgp = (Bgp) algebra;
            Assert.AreEqual(1, bgp.TriplePatterns.Count);
            Assert.IsTrue(bgp.TriplePatterns.Contains(t));
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
