using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Patterns;
using Xunit;

namespace VDS.RDF.Query
{
    public class QueryManipulationTests
    {
        [Fact]
        public void InsertValuesClause()
        {
            var g = new Graph();
            g.LoadFromFile("resources\\rvesse.ttl");
            var parser = new SparqlQueryParser();
            SparqlQuery startingQuery =
                parser.ParseFromString("SELECT * WHERE { <http://www.dotnetrdf.org/people#rvesse> ?p ?o }");
            var bindingVars = new List<string> {"p"};
            var binding = new BindingsPattern(bindingVars);
            binding.AddTuple(new BindingTuple(bindingVars, new List<PatternItem>
            {
                new NodeMatchPattern(g.CreateUriNode(UriFactory.Create("http://xmlns.com/foaf/0.1/name")))
            }));
            binding.AddTuple(new BindingTuple(bindingVars, new List<PatternItem>
            {
                new NodeMatchPattern(g.CreateUriNode(UriFactory.Create("http://xmlns.com/foaf/0.1/givenname")))
            }));
            startingQuery.RootGraphPattern.AddInlineData(binding);
            
            g.ExecuteQuery(startingQuery).Should().BeAssignableTo<SparqlResultSet>().Which.Count.Should().Be(2);
        }
    }
}
