namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GraphDictionaryTests
    {
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
            var o = d.Triples.ObjectNodes.Single();

            Assert.IsTrue(d.ContainsKey(s));
            Assert.IsFalse(d.ContainsKey(o));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Remove_requires_key()
        {
            var d = new DynamicGraph();

            d.Remove(null as INode);
        }

        [TestMethod]
        public void Remove_retracts_statements_with_subject()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s2> <urn:s> <urn:o5> .
<urn:s2> <urn:p3> <urn:s> .
");

            var d = new DynamicGraph();
            d.LoadFromString(@"
<urn:s> <urn:p1> <urn:o1> .
<urn:s> <urn:p1> <urn:o2> .
<urn:s> <urn:p2> <urn:o3> .
<urn:s> <urn:p2> <urn:o4> .
<urn:s2> <urn:s> <urn:o5> .
<urn:s2> <urn:p3> <urn:s> .
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_requires_key()
        {
            var d = new DynamicGraph();

            d.Add(null as INode, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_requires_value()
        {
            var d = new DynamicGraph();

            d.Add(d.CreateBlankNode(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_cant_add_existing_key()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = d.Triples.SubjectNodes.Single();

            d.Add(s, 0);
        }

        [TestMethod]
        public void Add_andles_nodes_from_other_graphs()
        {
            var other = new Graph();
            other.LoadFromString("<urn:s> <urn:p> <urn:o>.");

            var d = new DynamicGraph(subjectBaseUri: new Uri("urn:"));

            var s = other.Triples.SubjectNodes.Single();
            var o = other.Triples.ObjectNodes.Single();

            d[s] = new { p = o };

            Assert.AreEqual(other, d);
        }

        [TestMethod]
        public void Keys_are_IRI_subject_nodes()
        {
            var d = new DynamicGraph();
            d.LoadFromString(@"
<urn:s1> <urn:p> <urn:o> .
<urn:s2> <urn:p> <urn:o> .
_:s <urn:p> <urn:o> .
");

            var dict = d as IDictionary<INode, object>;

            CollectionAssert.AreEqual(d.Triples.SubjectNodes.UriNodes().ToArray(), dict.Keys.ToArray());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Set_index_requires_key()
        {
            var d = new DynamicGraph();

            d[null as INode] = null;
        }

        [TestMethod]
        public void Set_index_null_value_removes_statements_with_subject()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s2> <urn:s> <urn:o5> .
<urn:s2> <urn:p3> <urn:s> .
");

            var d = new DynamicGraph();
            d.LoadFromString(@"
<urn:s> <urn:p1> <urn:o1> .
<urn:s> <urn:p1> <urn:o2> .
<urn:s> <urn:p2> <urn:o3> .
<urn:s> <urn:p2> <urn:o4> .
<urn:s2> <urn:s> <urn:o5> .
<urn:s2> <urn:p3> <urn:s> .
");

            var s = d.Triples.First().Subject;

            d[s] = null;

            Assert.AreEqual(g, d);
        }

        [TestMethod]
        public void Set_index_replaces_statements_with_subject()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
<urn:s1> <urn:p> <urn:o> .
<urn:s2> <urn:p> <urn:o> .
");

            var d = new DynamicGraph(predicateBaseUri: new Uri("urn:"));
            d.LoadFromString(@"
<urn:s> <urn:p> <urn:o1> .
<urn:s> <urn:p> <urn:o2> .
<urn:s> <urn:p1> <urn:o3> .
<urn:s> <urn:p2> <urn:o4> .
<urn:s1> <urn:p> <urn:o> .
<urn:s2> <urn:p> <urn:o> .
");

            var s = d.Triples.First().Subject;

            d[s] = new { p = new Uri("urn:o") };

            Assert.AreEqual(g, d);
        }

        [TestMethod]
        public void Contains_rejects_null_keys()
        {
            var d = new DynamicGraph();
            var dict = ((IDictionary<INode, object>)d);
            var o = d.CreateBlankNode();

            var condition = dict.Contains(new KeyValuePair<INode, object>(null, o));

            Assert.IsFalse(condition);
        }


        [TestMethod]
        public void Contains_rejects_null_values()
        {
            var d = new DynamicGraph();
            var dict = ((IDictionary<INode, object>)d);
            var s = d.CreateBlankNode();

            var condition = dict.Contains(new KeyValuePair<INode, object>(s, null));

            Assert.IsFalse(condition);
        }

        [TestMethod]
        public void Contains_rejects_missing_key()
        {
            var d = new DynamicGraph(predicateBaseUri: new Uri("urn:"));
            var dict = ((IDictionary<INode, object>)d);
            var s = d.CreateBlankNode();

            var condition = dict.Contains(new KeyValuePair<INode, object>(s, new { p = "o" }));

            Assert.IsFalse(condition);
        }

        [TestMethod]
        public void Contains_rejects_missing_statement()
        {
            var d = new DynamicGraph(predicateBaseUri: new Uri("urn:"));
            d.LoadFromString("<urn:s> <urn:p> \"o\" .");

            var dict = ((IDictionary<INode, object>)d);
            var s = d.Triples.First().Subject;

            var condition = dict.Contains(new KeyValuePair<INode, object>(s, new { p = "o1" }));

            Assert.IsFalse(condition);
        }
    }
}
