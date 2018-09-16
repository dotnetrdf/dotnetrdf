namespace Dynamic
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using VDS.RDF;

    [TestClass]
    public class GraphDictionaryTests
    {
        [TestMethod]
        public void Handles_nodes_from_other_graphs()
        {
            var expected = new Graph();
            expected.LoadFromString("<urn:s> <urn:p> <urn:o>.");

            var actual = new DynamicGraph(subjectBaseUri: new Uri("urn:"));

            var s = expected.Triples.SubjectNodes.Single();
            var o = expected.Triples.ObjectNodes.Single();

            actual[s] = new { p = o };

            Assert.AreEqual(expected, actual);
        }
    }
}
