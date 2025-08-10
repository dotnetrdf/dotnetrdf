using Xunit;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;


public class PatternToAlgebraTests
{
    [Fact]
    public void SparqlGraphPatternToAlgebra1()
    {
        var gp = new GraphPattern
        {
            IsGraph = true,
            GraphSpecifier = new VariableToken("g", 0, 0, 1)
        };

        ISparqlAlgebra algebra = gp.ToAlgebra();
        Assert.IsType<Graph>(algebra);

        var g = (Graph) algebra;
        Assert.IsType<IBgp>(g.InnerAlgebra, exactMatch: false);
    }

    [Fact]
    public void SparqlGraphPatternToAlgebra2()
    {
        var up = new GraphPattern
        {
            IsUnion = true
        };

        var gp = new GraphPattern
        {
            IsGraph = true,
            GraphSpecifier = new VariableToken("g", 0, 0, 1)
        };
        up.AddGraphPattern(gp);

        var empty = new GraphPattern();
        up.AddGraphPattern(empty);

        ISparqlAlgebra algebra = up.ToAlgebra();
        Assert.IsType<Union>(algebra);
        var u = (Union) algebra;

        Assert.IsType<Graph>(u.Lhs);

        var g = (Graph)u.Lhs;
        Assert.IsType<IBgp>(g.InnerAlgebra, exactMatch: false);

        Assert.IsType<IBgp>(u.Rhs, exactMatch: false);
    }

    [Fact]
    public void SparqlGraphPatternToAlgebra3()
    {
        var gp = new GraphPattern
        {
            IsService = true,
            GraphSpecifier = new UriToken("<http://example.org/sparql>", 0, 0, 0)
        };

        ISparqlAlgebra algebra = gp.ToAlgebra();
        Assert.IsType<Service>(algebra);
    }

    [Fact]
    public void SparqlGraphPatternToAlgebra4()
    {
        var up = new GraphPattern
        {
            IsUnion = true
        };

        var gp = new GraphPattern
        {
            IsService = true,
            GraphSpecifier = new VariableToken("g", 0, 0, 1)
        };
        up.AddGraphPattern(gp);

        var empty = new GraphPattern();
        up.AddGraphPattern(empty);

        ISparqlAlgebra algebra = up.ToAlgebra();
        Assert.IsType<Union>(algebra);
        var u = (Union)algebra;

        Assert.IsType<Service>(u.Lhs);

        Assert.IsType<IBgp>(u.Rhs, exactMatch: false);
    }

    [Fact]
    public void SparqlGraphPatternToAlgebra5()
    {
        var gp = new GraphPattern
        {
            IsGraph = true,
            GraphSpecifier = new VariableToken("g", 0, 0, 1)
        };
        gp.AddInlineData(new BindingsPattern());

        ISparqlAlgebra algebra = gp.ToAlgebra();
        Assert.IsType<IJoin>(algebra, exactMatch: false);

        var join = (IJoin) algebra;

        Assert.IsType<Graph>(join.Lhs);
        var g = (Graph)join.Lhs;
        Assert.IsType<IBgp>(g.InnerAlgebra, exactMatch: false);

        Assert.IsType<Bindings>(join.Rhs, exactMatch: false);
    }

    [Fact]
    public void SparqlGraphPatternToAlgebra6()
    {
        var gp = new GraphPattern
        {
            IsService = true,
            GraphSpecifier = new VariableToken("g", 0, 0, 1)
        };
        gp.AddInlineData(new BindingsPattern());

        ISparqlAlgebra algebra = gp.ToAlgebra();
        Assert.IsType<IJoin>(algebra, exactMatch: false);

        var join = (IJoin)algebra;
        Assert.IsType<Service>(join.Lhs);
        Assert.IsType<Bindings>(join.Rhs);
    }

    [Fact]
    public void SparqlGraphPatternToAlgebra7()
    {
        var gp = new GraphPattern
        {
            IsGraph = true,
            GraphSpecifier = new VariableToken("g", 0, 0, 1)
        };

        ISparqlAlgebra algebra = gp.ToAlgebra();
        Assert.IsType<Graph>(algebra);

        var g = (Graph)algebra;
        Assert.IsType<IBgp>(g.InnerAlgebra, exactMatch: false);

        // Nest in another graph pattern with same specifier
        var parent = new GraphPattern
        {
            IsGraph = true,
            GraphSpecifier = gp.GraphSpecifier
        };
        parent.AddGraphPattern(gp);

        // Resulting algebra will collapse the two graph clauses into a single one
        algebra = parent.ToAlgebra();
        Assert.IsType<Graph>(algebra);

        g = (Graph)algebra;
        Assert.IsType<IBgp>(g.InnerAlgebra, exactMatch: false);
    }

    [Fact]
    public void SparqlGraphPatternToAlgebra8()
    {
        var gp = new GraphPattern
        {
            IsGraph = true,
            GraphSpecifier = new VariableToken("g", 0, 0, 1)
        };

        ISparqlAlgebra algebra = gp.ToAlgebra();
        Assert.IsType<Graph>(algebra);

        var g = (Graph)algebra;
        Assert.IsType<IBgp>(g.InnerAlgebra, exactMatch: false);

        // Nest in another graph pattern with different specifier
        var parent = new GraphPattern
        {
            IsGraph = true,
            GraphSpecifier = new UriToken("<http://example.org>", 0, 0, 0)
        };
        parent.AddGraphPattern(gp);

        // Resulting algebra must nest the graph clauses
        algebra = parent.ToAlgebra();
        Assert.IsType<Graph>(algebra);

        g = (Graph)algebra;
        Assert.Equal(parent.GraphSpecifier.Value, g.GraphSpecifier.Value);
        Assert.IsType<Graph>(g.InnerAlgebra);

        g = (Graph) g.InnerAlgebra;
        Assert.Equal(gp.GraphSpecifier.Value, g.GraphSpecifier.Value);
        Assert.IsType<IBgp>(g.InnerAlgebra, exactMatch: false);
    }

    [Fact]
    public void SparqlGraphPatternToAlgebra9()
    {
        var gp = new GraphPattern
        {
            IsGraph = true,
            GraphSpecifier = new VariableToken("g", 0, 0, 1)
        };

        ISparqlAlgebra algebra = gp.ToAlgebra();
        Assert.IsType<Graph>(algebra);

        var g = (Graph)algebra;
        Assert.IsType<IBgp>(g.InnerAlgebra, exactMatch: false);

        // Nest in another graph pattern with same specifier but also with another BGP
        var parent = new GraphPattern
        {
            IsGraph = true,
            GraphSpecifier = gp.GraphSpecifier
        };
        parent.AddGraphPattern(gp);
        ITriplePattern tp = new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o"));
        parent.AddTriplePattern(tp);

        // Resulting algebra will keep both graph clauses because of the join
        algebra = parent.ToAlgebra();

        Assert.IsType<Graph>(algebra);
        g = (Graph) algebra;

        Assert.IsType<Join>(g.InnerAlgebra);

        var join = (Join) g.InnerAlgebra;
        Assert.IsType<Graph>(join.Lhs);

        g = (Graph)join.Lhs;
        Assert.IsType<IBgp>(g.InnerAlgebra, exactMatch: false);
    }
}
