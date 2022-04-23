using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VDS.RDF.Query
{
    public class Issue466Tests
    {
        [Fact]
        public void IndexingShouldNotAffectResults()
        {
            var data = @"@prefix : <urn:edge/>.
@prefix al: <urn:al/>.
_:autos1005 a al:Foo;
            :name ""bla"".
_:autos1004 a al:Bar;
            :name ""bla"".";
            var query = @"PREFIX al: <urn:al/>
PREFIX : <urn:edge/>
SELECT * WHERE { ?p a al:Bar; :name 'bla' }";

            Options.FullTripleIndexing = false;
            var unindexedGraph = new Graph(new TreeIndexedTripleCollection());
            unindexedGraph.LoadFromString(data);
            var unindexedGraphResults = unindexedGraph.ExecuteQuery(query) as SparqlResultSet;
            Assert.Equal(1, unindexedGraphResults.Count);

            Options.FullTripleIndexing = true;
            var indexedGraph = new Graph(new TreeIndexedTripleCollection());
            indexedGraph.LoadFromString(data);
            var indexedGraphResults = indexedGraph.ExecuteQuery(query) as SparqlResultSet;

            Assert.Equal(indexedGraphResults.Count, unindexedGraphResults.Count);
        }
    }
}
