﻿using Xunit;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{

    public class PatternToAlgebraTests
    {
        [Fact]
        public void SparqlGraphPatternToAlgebra1()
        {
            GraphPattern gp = new GraphPattern();
            gp.IsGraph = true;
            gp.GraphSpecifier = new VariableToken("g", 0, 0, 1);

            ISparqlAlgebra algebra = gp.ToAlgebra();
            Assert.IsType<Graph>(algebra);

            Graph g = (Graph) algebra;
            Assert.IsAssignableFrom<IBgp>(g.InnerAlgebra);
        }

        [Fact]
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
            Assert.IsType<Union>(algebra);
            Union u = (Union) algebra;

            Assert.IsType<Graph>(u.Lhs);

            Graph g = (Graph)u.Lhs;
            Assert.IsAssignableFrom<IBgp>(g.InnerAlgebra);

            Assert.IsAssignableFrom<IBgp>(u.Rhs);
        }

        [Fact]
        public void SparqlGraphPatternToAlgebra3()
        {
            GraphPattern gp = new GraphPattern();
            gp.IsService = true;
            gp.GraphSpecifier = new UriToken("<http://example.org/sparql>", 0, 0, 0);

            ISparqlAlgebra algebra = gp.ToAlgebra();
            Assert.IsType<Service>(algebra);
        }

        [Fact]
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
            Assert.IsType<Union>(algebra);
            Union u = (Union)algebra;

            Assert.IsType<Service>(u.Lhs);

            Assert.IsAssignableFrom<IBgp>(u.Rhs);
        }

        [Fact]
        public void SparqlGraphPatternToAlgebra5()
        {
            GraphPattern gp = new GraphPattern();
            gp.IsGraph = true;
            gp.GraphSpecifier = new VariableToken("g", 0, 0, 1);
            gp.AddInlineData(new BindingsPattern());

            ISparqlAlgebra algebra = gp.ToAlgebra();
            Assert.IsAssignableFrom<IJoin>(algebra);

            IJoin join = (IJoin) algebra;

            Assert.IsType<Graph>(join.Lhs);
            Graph g = (Graph)join.Lhs;
            Assert.IsAssignableFrom<IBgp>(g.InnerAlgebra);

            Assert.IsAssignableFrom<Bindings>(join.Rhs);
        }

        [Fact]
        public void SparqlGraphPatternToAlgebra6()
        {
            GraphPattern gp = new GraphPattern();
            gp.IsService = true;
            gp.GraphSpecifier = new VariableToken("g", 0, 0, 1);
            gp.AddInlineData(new BindingsPattern());

            ISparqlAlgebra algebra = gp.ToAlgebra();
            Assert.IsAssignableFrom<IJoin>(algebra);

            IJoin join = (IJoin)algebra;
            Assert.IsType<Service>(join.Lhs);
            Assert.IsType<Bindings>(join.Rhs);
        }

        [Fact]
        public void SparqlGraphPatternToAlgebra7()
        {
            GraphPattern gp = new GraphPattern();
            gp.IsGraph = true;
            gp.GraphSpecifier = new VariableToken("g", 0, 0, 1);

            ISparqlAlgebra algebra = gp.ToAlgebra();
            Assert.IsType<Graph>(algebra);

            Graph g = (Graph)algebra;
            Assert.IsAssignableFrom<IBgp>(g.InnerAlgebra);

            // Nest in another graph pattern with same specifier
            GraphPattern parent = new GraphPattern();
            parent.IsGraph = true;
            parent.GraphSpecifier = gp.GraphSpecifier;
            parent.AddGraphPattern(gp);

            // Resulting algebra will collapse the two graph clauses into a single one
            algebra = parent.ToAlgebra();
            Assert.IsType<Graph>(algebra);

            g = (Graph)algebra;
            Assert.IsAssignableFrom<IBgp>(g.InnerAlgebra);
        }

        [Fact]
        public void SparqlGraphPatternToAlgebra8()
        {
            GraphPattern gp = new GraphPattern();
            gp.IsGraph = true;
            gp.GraphSpecifier = new VariableToken("g", 0, 0, 1);

            ISparqlAlgebra algebra = gp.ToAlgebra();
            Assert.IsType<Graph>(algebra);

            Graph g = (Graph)algebra;
            Assert.IsAssignableFrom<IBgp>(g.InnerAlgebra);

            // Nest in another graph pattern with different specifier
            GraphPattern parent = new GraphPattern();
            parent.IsGraph = true;
            parent.GraphSpecifier = new UriToken("<http://example.org>", 0, 0, 0);
            parent.AddGraphPattern(gp);

            // Resulting algebra must nest the graph clauses
            algebra = parent.ToAlgebra();
            Assert.IsType<Graph>(algebra);

            g = (Graph)algebra;
            Assert.Equal(parent.GraphSpecifier.Value, g.GraphSpecifier.Value);
            Assert.IsType<Graph>(g.InnerAlgebra);

            g = (Graph) g.InnerAlgebra;
            Assert.Equal(gp.GraphSpecifier.Value, g.GraphSpecifier.Value);
            Assert.IsAssignableFrom<IBgp>(g.InnerAlgebra);
        }

        [Fact]
        public void SparqlGraphPatternToAlgebra9()
        {
            GraphPattern gp = new GraphPattern();
            gp.IsGraph = true;
            gp.GraphSpecifier = new VariableToken("g", 0, 0, 1);

            ISparqlAlgebra algebra = gp.ToAlgebra();
            Assert.IsType<Graph>(algebra);

            Graph g = (Graph)algebra;
            Assert.IsAssignableFrom<IBgp>(g.InnerAlgebra);

            // Nest in another graph pattern with same specifier but also with another BGP
            GraphPattern parent = new GraphPattern();
            parent.IsGraph = true;
            parent.GraphSpecifier = gp.GraphSpecifier;
            parent.AddGraphPattern(gp);
            ITriplePattern tp = new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o"));
            parent.AddTriplePattern(tp);

            // Resulting algebra will keep both graph clauses because of the join
            algebra = parent.ToAlgebra();

            Assert.IsType<Graph>(algebra);
            g = (Graph) algebra;

            Assert.IsType<Join>(g.InnerAlgebra);

            Join join = (Join) g.InnerAlgebra;
            Assert.IsType<Graph>(join.Lhs);

            g = (Graph)join.Lhs;
            Assert.IsAssignableFrom<IBgp>(g.InnerAlgebra);
        }
    }
}
