using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Utilities.GraphBenchmarker.Test.Actual;

namespace VDS.RDF.Utilities.GraphBenchmarker.Test
{
    public delegate void TestSuiteProgressHandler();

    public class TestSuite
    {
        private BindingList<ITest> _tests = new BindingList<ITest>();
        private BindingList<TestResult> _results = new BindingList<TestResult>();
        private BindingList<TestCase> _cases = new BindingList<TestCase>();
        private int _currTest = 0, _currTestCase = 0;
        private bool _cancelled = false;
        private String _data;
        private int _iterations;

        public TestSuite(IEnumerable<TestCase> testCases, String data, int iterations)
        {
            this._data = data;
            this._iterations = iterations;

            foreach (TestCase c in testCases)
            {
                this._cases.Add(c);
            }

            //Firstly add the Initial Memory Usage Check
            this._tests.Add(new InitialMemoryUsageCheck());

            //Then do a Load Test and a further Memory Usage Check
            this._tests.Add(new LoadDataTest(data));
            this._tests.Add(new MemoryUsageCheck());

            //Then add the actual tests
            this._tests.Add(new EnumerateTriplesTest(iterations));
            this._tests.Add(new SubjectLookupTest(iterations));
            this._tests.Add(new PredicateLookupTest(iterations));
            this._tests.Add(new ObjectLookupTest(iterations));

            //Do an Enumerate Test again to see if index population has changed performance
            this._tests.Add(new EnumerateTriplesTest(iterations));

            //Finally add the final Memory Usage Check
            this._tests.Add(new MemoryUsageCheck());
        }

        public String Data
        {
            get
            {
                return this._data;
            }
        }

        public int Iterations
        {
            get
            {
                return this._iterations;
            }
        }

        public BindingList<TestCase> TestCases
        {
            get
            {
                return this._cases;
            }
        }

        public BindingList<ITest> Tests
        {
            get
            {
                return this._tests;
            }
        }

        public int CurrentTest
        {
            get
            {
                return this._currTest;
            }
        }

        public int CurrentTestCase
        {
            get
            {
                return this._currTestCase;
            }
        }

        public void Run()
        {
            for (int c = 0; c < this._cases.Count; c++)
            {
                if (this._cancelled) break;

                this._currTestCase = c;
                this._currTest = 0;
                this._cases[c].Reset(true);

                this.RaiseProgress();

                //Get the Initial Memory Usage allowing the GC to clean up as necessary
                this._cases[c].InitialMemory = GC.GetTotalMemory(true);

                //Do this to ensure we've created the Graph instance
                IGraph temp = this._cases[c].Instance;

                this.RaiseProgress();

                for (int t = 0; t < this._tests.Count; t++)
                {
                    if (this._cancelled) break;

                    this._currTest = t;
                    this.RaiseProgress();

                    //Run the Test and remember the Results
                    TestResult r = this._tests[t].Run(this._cases[c]);
                    this._cases[c].Results.Add(r);
                }
            }
            this.RaiseProgress();
            if (this._cancelled)
            {
                this.RaiseCancelled();
            }
            this._cancelled = false;

            foreach (TestCase c in this._cases)
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
