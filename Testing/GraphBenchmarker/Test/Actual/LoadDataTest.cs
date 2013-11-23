using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Utilities.GraphBenchmarker.Test.Actual
{
    public class LoadDataTest : SingleRunTest
    {
        private String _data;

        public LoadDataTest(String data)
            : base("Load Data", "Loads the Test Data into the Graph")
        {
            this._data = data;
        }

        public override TestResult Run(TestCase testCase)
        {
            DateTime start = DateTime.Now;
            FileLoader.Load(testCase.Instance, this._data);
            TimeSpan elapsed = DateTime.Now - start;

            return new TestResult(elapsed, testCase.Instance.Count, "Triples/Second", TestMetricType.Speed);
        }
    }
}
