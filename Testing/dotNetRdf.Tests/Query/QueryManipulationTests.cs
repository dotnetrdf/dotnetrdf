using System.Collections.Generic;
using FluentAssertions;
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Patterns;
using Xunit;

namespace VDS.RDF.Query;

public class QueryManipulationTests
{
    [Fact]
    public void InsertValuesClause()
    {
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "rvesse.ttl"));
        var parser = new SparqlQueryParser();
        SparqlQuery startingQuery =
            parser.ParseFromString("SELECT * WHERE { <http://www.dotnetrdf.org/people#rvesse> ?p ?o }");
        var bindingVars = new List<string> {"p"};
        var binding = new BindingsPattern(bindingVars);
        binding.AddTuple(new BindingTuple(bindingVars, new List<PatternItem>
        {
            new NodeMatchPattern(g.CreateUriNode(UriFactory.Root.Create("http://xmlns.com/foaf/0.1/name")))
        }));
        binding.AddTuple(new BindingTuple(bindingVars, new List<PatternItem>
        {
            new NodeMatchPattern(g.CreateUriNode(UriFactory.Root.Create("http://xmlns.com/foaf/0.1/givenname")))
        }));
        startingQuery.RootGraphPattern.AddInlineData(binding);
        
        g.ExecuteQuery(startingQuery).Should().BeAssignableTo<SparqlResultSet>().Which.Count.Should().Be(2);
    }
}
