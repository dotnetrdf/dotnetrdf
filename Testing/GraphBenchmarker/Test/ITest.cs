using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.GraphBenchmarker.Test
{
    public enum TestType
    {
        SingleRun,
        Iterations
    }

    public interface ITest
    {
        String Name
        {
            get;
        }

        String Description
        {
            get;
        }

        TestType Type
        {
            get;
        }

        TestResult Run(TestCase testCase);
    }
}
