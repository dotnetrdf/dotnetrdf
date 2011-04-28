using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Update;

namespace VDS.RDF.Test.Update
{
    [TestClass]
    public class UpdateTimeouts
    {
        private SparqlUpdateParser _parser = new SparqlUpdateParser();

        [TestMethod]
        public void SparqlUpdateTimeout()
        {
            String update = "CREATE GRAPH <http://example.org/1>; LOAD <http://www.dotnetrdf.org/configuration#>; CREATE GRAPH <http://example.org/2>";
            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(update);
            cmds.Timeout = 1;

            TripleStore store = new TripleStore();
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
            try
            {
                processor.ProcessCommandSet(cmds);
                Assert.Fail("Expected a SparqlUpdateTimeoutException");
            }
            catch (SparqlUpdateTimeoutException timeoutEx)
            {
                TestTools.ReportError("Timeout", timeoutEx, false);
                Console.WriteLine();
                Console.WriteLine("Execution Time: " + cmds.UpdateExecutionTime.Value.ToString());

                Assert.IsFalse(store.HasGraph(new Uri("http://example.org/1")), "Graph 1 should not exist");
                Assert.IsFalse(store.HasGraph(new Uri("http://example.org/2")), "Graph 2 should not exist");

            }
        }
    }
}
