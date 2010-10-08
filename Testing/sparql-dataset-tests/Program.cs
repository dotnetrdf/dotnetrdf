using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using dotNetRDFTest;

namespace sparql_dataset_tests
{
    class Program
    {
        public static void Main(string[] args)
        {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            SparqlEvaluationTestSuite tests = new SparqlEvaluationTestSuite();
            tests.RunTests();
        }
    }
}
