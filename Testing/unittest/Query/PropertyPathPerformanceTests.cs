using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Dynamic;
using Xunit;

namespace VDS.RDF.Query
{
    public class PropertyPathPerformanceTests
    {
        [Fact]
        public void OptimisedZeroOrMorePathsEvaluation()
        {
            var queryString = @"SELECT ?concept WHERE {
   ?scheme <http://www.w3.org/2004/02/skos/core#hasTopConcept> ?topConcept.
   ?topConcept ^<http://www.w3.org/2004/02/skos/core#broader>* ?concept.
}";
            var g = new Graph();
            g.LoadFromFile(Path.Combine("resources", "issue-363", "fibo-vD.ttl.gz"));
            g.ExecuteQuery(queryString);
        }
    }
}
