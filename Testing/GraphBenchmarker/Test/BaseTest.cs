using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.GraphBenchmarker.Test
{
    public abstract class BaseTest : ITest
    {
        private TestType _type;

        public BaseTest(String name, String description, TestType type)
        {
            this.Name = name;
            this.Description = description;
            this._type = type;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Description
        {
            get;
            private set;
        }

        public TestType Type
        {
            get
            {
                return this._type;
            }
        }

        public abstract TestResult Run(TestCase testCase);
    }

    public abstract class SingleRunTest : BaseTest
    {
        public SingleRunTest(String name, String description)
            : base(name, description, TestType.SingleRun)
        { }
    }

    public abstract class IterationTest : BaseTest
    {
        private int _iterations;
        private String _unit;

        public IterationTest(String name, String description, int iterations, String unit)
            : base(name, description, TestType.Iterations)
        {
            this._iterations = Math.Max(1000, iterations);
            this._unit = unit;
        }

        /// <summary>
        /// Allows for actions to be taken prior to iterations which don't count towards the Benchmarked score
        /// </summary>
        /// <param name="testCase"></param>
        protected virtual void PreIterationSetup(TestCase testCase)
        {

        }

        public override TestResult Run(TestCase testCase)
        {
            this.PreIterationSetup(testCase);

            DateTime start = DateTime.Now;
            int actions = 0;
            for (int i = 0; i < this._iterations; i++)
            {
                actions += this.RunIteration(testCase);
            }
            TimeSpan elapsed = DateTime.Now - start;

            return new TestResult(elapsed, actions, this._unit);
        }

        protected abstract int RunIteration(TestCase testCase);
    }
}
