using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;

namespace VDS.RDF.Utilities.GraphBenchmarker.Test.Actual
{
    public class EnumerateTriplesTest : IterationTest
    {
        public EnumerateTriplesTest(int iterations)
            : base("Enumerate Triples", "Enumerates Triples by foreach over the Triple Collection of the Graph", iterations, "Triples/Second") { }

        protected override long RunIteration(TestCase testCase)
        {
            foreach (Triple t in testCase.Instance.Triples)
            {
                //Do Nothing
            }
            return testCase.Instance.Count;
        }
    }
}
