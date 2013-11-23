using System;
using System.Collections.Generic;
using System.ComponentModel;
using VDS.RDF.Graphs;
using VDS.RDF.Utilities.GraphBenchmarker.Test.Actual;

namespace VDS.RDF.Utilities.GraphBenchmarker.Test
{
    public delegate void TestSuiteProgressHandler();

    public class TestSuite
    {
        private bool _cancelled = false;

        public TestSuite(IEnumerable<TestCase> testCases, String data, int iterations, TestSet set)
        {
            Tests = new BindingList<ITest>();
            TestCases = new BindingList<TestCase>();
            CurrentTest = 0;
            CurrentTestCase = 0;
            this.Data = data;
            this.Iterations = iterations;

            foreach (TestCase c in testCases)
            {
                this.TestCases.Add(c);
            }

            //Firstly add the Initial Memory Usage Check
            this.Tests.Add(new InitialMemoryUsageCheck());

            //Then do a Load Test and a further Memory Usage Check
            this.Tests.Add(new LoadDataTest(data));
            this.Tests.Add(new CountTriplesTest());
            this.Tests.Add(new MemoryUsageCheck());
            this.Tests.Add(new MemoryUsagePerTripleCheck());

            //Then add the actual tests
            if (set == TestSet.Standard)
            {
                this.Tests.Add(new EnumerateTriplesTest(iterations));
                this.Tests.Add(new SubjectLookupTest(iterations));
                this.Tests.Add(new PredicateLookupTest(iterations));
                this.Tests.Add(new ObjectLookupTest(iterations));

                //Do an Enumerate Test again to see if index population has changed performance
                this.Tests.Add(new EnumerateTriplesTest(iterations));

                //Finally add the final Memory Usage Check
                this.Tests.Add(new MemoryUsageCheck());
            }
        }

        public string Data { get; private set; }

        public int Iterations { get; private set; }

        public BindingList<TestCase> TestCases { get; private set; }

        public BindingList<ITest> Tests { get; private set; }

        public int CurrentTest { get; private set; }

        public int CurrentTestCase { get; private set; }

        public void Run()
        {
            for (int c = 0; c < this.TestCases.Count; c++)
            {
                if (this._cancelled) break;

                this.CurrentTestCase = c;
                this.CurrentTest = 0;
                this.TestCases[c].Reset(true);

                this.RaiseProgress();

                //Get the Initial Memory Usage allowing the GC to clean up as necessary
                this.TestCases[c].InitialMemory = GC.GetTotalMemory(true);

                //Do this to ensure we've created the Graph instance
// ReSharper disable UnusedVariable
                IGraph temp = this.TestCases[c].Instance;
// ReSharper restore UnusedVariable

                this.RaiseProgress();

                for (int t = 0; t < this.Tests.Count; t++)
                {
                    if (this._cancelled) break;

                    this.CurrentTest = t;
                    this.RaiseProgress();

                    //Run the Test and remember the Results
                    TestResult r = this.Tests[t].Run(this.TestCases[c]);
                    this.TestCases[c].Results.Add(r);
                }

                //Clear URI Factory after Tests to return memory usage to base levels
                UriFactory.Clear();
            }
            this.RaiseProgress();
            if (this._cancelled)
            {
                this.RaiseCancelled();
            }
            this._cancelled = false;

            foreach (TestCase c in this.TestCases)
            {
                c.Reset(false);
            }
        }

        public void Cancel()
        {
            this._cancelled = true;
        }

        public event TestSuiteProgressHandler Progress;

        public event TestSuiteProgressHandler Cancelled;

        private void RaiseProgress()
        {
            TestSuiteProgressHandler d = this.Progress;
            if (d != null)
            {
                d();
            }
        }

        private void RaiseCancelled()
        {
            TestSuiteProgressHandler d = this.Cancelled;
            if (d != null)
            {
                d();
            }
        }
    }
}
