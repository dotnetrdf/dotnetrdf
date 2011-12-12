using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace dotNetRDFTest
{
    public static class DawgTests
    {
        public static void Main(string[] args)
        {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            SparqlEvaluationTestSuite sparql10 = new SparqlEvaluationTestSuite();
            sparql10.RunTests();
            Sparql11EvaluationTestSuite sparql11 = new Sparql11EvaluationTestSuite();
            sparql11.RunTests();
        }
    }
}
