using NUnit.Framework;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    [TestFixture]
    public class PatternToAlgebraTests
    {
        [Test]
        public void SparqlGraphPatternToAlgebra1()
        {
            GraphPattern gp = new GraphPattern();
            gp.IsGraph = true;
            gp.GraphSpecifier = new VariableToken("g", 0, 0, 1);

            ISparqlAlgebra algebra = gp.ToAlgebra();
            Assert.IsInstanceOf(typeof(Graph), algebra);

            Graph g = (Graph) algebra;
            Assert.IsInstanceOf(typeof(IBgp), g.InnerAlgebra);
        }

        [Test]
        public void SparqlGraphPatternToAlgebra2()
        {
            GraphPattern up = new GraphPattern();
            up.IsUnion = true;

            GraphPattern gp = new GraphPattern();
            gp.IsGraph = true;
            gp.GraphSpecifier = new VariableToken("g", 0, 0, 1);
            up.AddGraphPattern(gp);

            GraphPattern empty = new GraphPattern();
            up.AddGraphPattern(empty);

            ISparqlAlgebra algebra = up.ToAlgebra();
            Assert.IsInstanceOf(typeof(Union), algebra);
            Union u = (Union) algebra;

            Assert.IsInstanceOf(typeof(Graph), u.Lhs);

            Graph g = (Graph)u.Lhs;
            Assert.IsInstanceOf(typeof(IBgp), g.InnerAlgebra);

            Assert.IsInstanceOf(typeof(IBgp), u.Rhs);
        }

        [Test]
        public void SparqlGraphPatternToAlgebra3()
        {
            GraphPattern gp = new GraphPattern();
            gp.IsService = true;
            gp.GraphSpecifier = new UriToken("<http://example.org/sparql>", 0, 0, 0);

            ISparqlAlgebra algebra = gp.ToAlgebra();
            Assert.IsInstanceOf(typeof(Service), algebra);
        }

        [Test]
        public void SparqlGraphPatternToAlgebra4()
        {
            GraphPattern up = new GraphPattern();
            up.IsUnion = true;

            GraphPattern gp = new GraphPattern();
            gp.IsService = true;
            gp.GraphSpecifier = new VariableToken("g", 0, 0, 1);
            up.AddGraphPattern(gp);

            GraphPattern empty = new GraphPattern();
            up.AddGraphPattern(empty);

            ISparqlAlgebra algebra = up.ToAlgebra();
            Assert.IsInstanceOf(typeof(Union), algebra);
            Union u = (Union)algebra;

            Assert.IsInstanceOf(typeof(Service), u.Lhs);

            Assert.IsInstanceOf(typeof(IBgp), u.Rhs);
        }

        [Test]
        public void SparqlGraphPatternToAlgebra5()
        {
            GraphPattern gp = new GraphPattern();
            gp.IsGraph = true;
            gp.GraphSpecifier = new VariableToken("g", 0, 0, 1);
            gp.AddInlineData(new BindingsPattern());

            ISparqlAlgebra algebra = gp.ToAlgebra();
            Assert.IsInstanceOf(typeof(IJoin), algebra);

            IJoin join = (IJoin) algebra;

            Assert.IsInstanceOf(typeof(Graph), join.Lhs);
            Graph g = (Graph)join.Lhs;
            Assert.IsInstanceOf(typeof(IBgp), g.InnerAlgebra);

            Assert.IsInstanceOf(typeof(Bindings), join.Rhs);
        }

        [Test]
        public void SparqlGraphPatternToAlgebra6()
        {
            GraphPattern gp = new GraphPattern();
            gp.IsService = true;
            gp.GraphSpecifier = new VariableToken("g", 0, 0, 1);
            gp.AddInlineData(new BindingsPattern());

            ISparqlAlgebra algebra = gp.ToAlgebra();
            Assert.IsInstanceOf(typeof(IJoin), algebra);

            IJoin join = (IJoin)algebra;
            Assert.IsInstanceOf(typeof(Service), join.Lhs);
            Assert.IsInstanceOf(typeof(Bindings), join.Rhs);
        }

        [Test]
        public void SparqlGraphPatternToAlgebra7()
        {
            GraphPattern gp = new GraphPattern();
            gp.IsGraph = true;
            gp.GraphSpecifier = new VariableToken("g", 0, 0, 1);

            ISparqlAlgebra algebra = gp.ToAlgebra();
            Assert.IsInstanceOf(typeof(Graph), algebra);

            Graph g = (Graph)algebra;
            Assert.IsInstanceOf(typeof(IBgp), g.InnerAlgebra);

            // Nest in another graph pattern with same specifier
            GraphPattern parent = new GraphPattern();
            parent.IsGraph = true;
            parent.GraphSpecifier = gp.GraphSpecifier;
            parent.AddGraphPattern(gp);

            // Resulting algebra will collapse the two graph clauses into a single one
            algebra = parent.ToAlgebra();
            Assert.IsInstanceOf(typeof(Graph), algebra);

            g = (Graph)algebra;
            Assert.IsInstanceOf(typeof(IBgp), g.InnerAlgebra);
        }

        [Test]
        public void SparqlGraphPatternToAlgebra8()
        {
            GraphPattern gp = new GraphPattern();
            gp.IsGraph = true;
            gp.GraphSpecifier = new VariableToken("g", 0, 0, 1);

            ISparqlAlgebra algebra = gp.ToAlgebra();
            Assert.IsInstanceOf(typeof(Graph), algebra);

            Graph g = (Graph)algebra;
            Assert.IsInstanceOf(typeof(IBgp), g.InnerAlgebra);

            // Nest in another graph pattern with different specifier
            GraphPattern parent = new GraphPattern();
            parent.IsGraph = true;
            parent.GraphSpecifier = new UriToken("<http://example.org>", 0, 0, 0);
            parent.AddGraphPattern(gp);

            // Resulting algebra must nest the graph clauses
            algebra = parent.ToAlgebra();
            Assert.IsInstanceOf(typeof(Graph), algebra);

            g = (Graph)algebra;
            Assert.AreEqual(parent.GraphSpecifier.Value, g.GraphSpecifier.Value);
            Assert.IsInstanceOf(typeof(Graph), g.InnerAlgebra);

            g = (Graph) g.InnerAlgebra;
            Assert.AreEqual(gp.GraphSpecifier.Value, g.GraphSpecifier.Value);
            Assert.IsInstanceOf(typeof(IBgp), g.InnerAlgebra);
        }

        [Test]
        public void SparqlGraphPatternToAlgebra9()
        {
            GraphPattern gp = new GraphPattern();
            gp.IsGraph = true;
            gp.GraphSpecifier = new VariableToken("g", 0, 0, 1);

            ISparqlAlgebra algebra = gp.ToAlgebra();
            Assert.IsInstanceOf(typeof(Graph), algebra);

            Graph g = (Graph)algebra;
            Assert.IsInstanceOf(typeof(IBgp), g.InnerAlgebra);

            // Nest in another graph pattern with same specifier but also with another BGP
            GraphPattern parent = new GraphPattern();
            parent.IsGraph = true;
            parent.GraphSpecifier = gp.GraphSpecifier;
            parent.AddGraphPattern(gp);
            ITriplePattern tp = new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o"));
            parent.AddTriplePattern(tp);

            // Resulting algebra will keep both graph clauses because of the join
            algebra = parent.ToAlgebra();

            Assert.IsInstanceOf(typeof(Graph), algebra);
            g = (Graph) algebra;

            Assert.IsInstanceOf(typeof(Join), g.InnerAlgebra);

            Join join = (Join) g.InnerAlgebra;
            Assert.IsInstanceOf(typeof(Graph), join.Lhs);

            g = (Graph)join.Lhs;
            Assert.IsInstanceOf(typeof(IBgp), g.InnerAlgebra);
        }
    }
}
