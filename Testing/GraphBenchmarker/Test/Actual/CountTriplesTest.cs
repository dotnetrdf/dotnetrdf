using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.GraphBenchmarker.Test.Actual
{
    public class CountTriplesTest
        : SingleRunTest
    {
        public CountTriplesTest()
            : base("Count Triples", "Asks the Graph for the count of it's Triples") { }

        public override TestResult Run(TestCase testCase)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            int triples = testCase.Instance.Triples.Count;
            timer.Stop();
            return new TestResult(timer.Elapsed, triples, "Triple(s)", TestMetricType.Count);
        }
    }
}
