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
            var g = new Graph();
            g.LoadFromString("<urn:s> <urn:p> <urn:o>.");

            var d = new DynamicGraph(subjectBaseUri: new Uri("urn:"));

            var s = g.Triples.SubjectNodes.Single();
            var o = g.Triples.ObjectNodes.Single();

            d[s] = new { p = o };

            Assert.AreEqual(g, d);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ContainsKey_requires_key()
        {
            var d = new DynamicGraph();

            d.ContainsKey(null as INode);
        }

        [TestMethod]
        public void ContainsKey_searches_subject_nodes()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = d.Triples.SubjectNodes.Single();

            Assert.IsTrue(d.ContainsKey(s));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Remove_requires_key()
        {
            var d = new DynamicGraph();

            d.Remove(null as INode);
        }

        [TestMethod]
        public void Remove_retracts_all_statements_with_subject()
        {
            var g = new Graph();
            g.LoadFromString("<urn:s2> <urn:p3> <urn:o5> .");

            var d = new DynamicGraph();
            d.LoadFromString(@"
<urn:s1> <urn:p1> <urn:o1> .
<urn:s1> <urn:p1> <urn:o2> .
<urn:s1> <urn:p2> <urn:o3> .
<urn:s1> <urn:p2> <urn:o4> .
<urn:s2> <urn:p3> <urn:o5> .
");

            var s = d.Triples.First().Subject;

            d.Remove(s);

            Assert.AreEqual(g, d);
        }

        [TestMethod]
        public void Remove_reports_retraction_success()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s2> <urn:p3> <urn:o5> .");

            var s = d.Triples.First().Subject;

            Assert.IsTrue(d.Remove(s));
            Assert.IsFalse(d.Remove(s));
        }
    }
}
