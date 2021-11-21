using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace dotNetRdf.TestSupport
{
    /// <summary>
    /// The base class for test classes that provide one or more methods for running RDF test suite tests.
    /// </summary>
    public class RdfTestSuite
    {
        private readonly Dictionary<string, MethodInfo> _runners;
        public RdfTestSuite()
        {
            _runners = new Dictionary<string, MethodInfo>();
            foreach (MethodInfo m in GetType().GetMethods())
            {
                if (m.GetCustomAttribute(typeof(ManifestTestRunnerAttribute)) is ManifestTestRunnerAttribute runnerAttr)
                {
                    _runners[runnerAttr.TestType] = m;
                }
            }
        }

        /// <summary>
        /// Locates the runner method that handles the type of test that <paramref name="t"/> is and then invokes that
        /// runner method with <paramref name="t"/>.
        /// </summary>
        /// <param name="t">The manifest data for the test to be executed.</param>
        /// <remarks>This method asserts that a runner for the test exists in the class, if it does not the test will fail.</remarks>
        public void InvokeTestRunner(ManifestTestData t)
        {
            Assert.True(_runners.ContainsKey(t.Type.AbsoluteUri));
            MethodInfo runner = _runners[t.Type.AbsoluteUri];
            runner.Invoke(this, new[] { t });
        }
    }
}