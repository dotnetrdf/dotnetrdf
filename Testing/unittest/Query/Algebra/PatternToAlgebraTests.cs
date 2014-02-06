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
    }
}
