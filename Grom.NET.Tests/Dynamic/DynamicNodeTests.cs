namespace Dynamic
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;

    [TestClass]
    public class DynamicNodeTests
    {
        public void ObjectCollectionTests()
        {
            var g1 = new Graph();
            g1.LoadFromString(@"
<http://example.com/s> <http://example.com/p> ""1"" .
<http://example.com/s> <http://example.com/p> ""2"" .
");
            var d = g1.AsDynamic(new Uri("http://example.com/"));
            var a = d.s.p as ICollection<object>;

            CollectionAssert.AreEquivalent(new[] { "1", "2" }.ToList(), a as ICollection);

            var g2 = new Graph();
            g2.LoadFromString(@"
<http://example.com/s> <http://example.com/p> ""0"" .
<http://example.com/s> <http://example.com/p> ""2"" .
");

            a.Remove("1");
            a.Add("0");
            Assert.AreEqual(g2, g1);

            var g3 = new Graph();
            g3.LoadFromString(@"
<http://example.com/s> <http://example.com/p> ""2"" .
");

            a.Remove("0");
            Assert.AreEqual(g3, g1);

            var g4 = new Graph();
            g4.LoadFromString(@"
            <http://example.com/s> <http://example.com/p> <http://example.com/o> .
            ");

            a.Remove("2");
            a.Add(new Uri("http://example.com/o"));
            Assert.AreEqual(g4, g1);

            a.Clear();
            Assert.IsTrue(g1.IsEmpty);

            a.Add("2");
            Assert.AreEqual(g3, g1);

            a.Add("0");
            Assert.AreEqual(g2, g1);
        }

        [TestMethod]
        public void Is_read_write()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();

            var result = dynamic_s["http://example.com/"] as ICollection<object>;

            Assert.IsFalse(result.IsReadOnly);
        }

        [TestMethod]
        public void count()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var example_s = g.Triples.Single().Subject;
            var dynamic_s = example_s.AsDynamic();
            var example_p = g.Triples.Single().Predicate;

            var expected = g.GetTriplesWithSubjectPredicate(example_s, example_p).Count();
            var actual = (dynamic_s[example_p] as ICollection<object>).Count();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void contains()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s["http://example.com/p"] as ICollection<object>;
            var result = objects.Contains(example_o);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void copyto()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var nodeObjectArray = g.Triples.ObjectNodes.ToArray();
            var dynamic_s = g.Triples.Single().Subject.AsDynamic();
            var example_o = g.Triples.Single().Object;

            var dynamicObjectArray = new object[1];
            var objects = dynamic_s["http://example.com/p"] as ICollection<object>;
            objects.CopyTo(dynamicObjectArray, 0);

            CollectionAssert.AreEqual(nodeObjectArray, dynamicObjectArray);
        }

        [TestMethod]
        public void clear()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();

            var objects = dynamic_s["http://example.com/p"] as ICollection<object>;
            objects.Clear();

            Assert.IsTrue(g.IsEmpty);
        }

        [TestMethod]
        public void add()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o1> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();

            var objects = dynamic_s["http://example.com/p"] as ICollection<object>;
            objects.Add(new Uri("http://example.com/o2"));

            var control = new Graph();
            control.LoadFromString(@"
<http://example.com/s> <http://example.com/p> <http://example.com/o1> .
<http://example.com/s> <http://example.com/p> <http://example.com/o2> .
");

            Assert.AreEqual(control, g);
        }

        [TestMethod]
        public void remove()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();
            var example_o = g.Triples.Single().Object.AsDynamic();

            var objects = dynamic_s["http://example.com/p"] as ICollection<object>;
            var result = objects.Remove(example_o);

            Assert.IsTrue(result);
            Assert.IsTrue(g.IsEmpty);
        }

        [TestMethod]
        public void removemissing()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();

            var objects = dynamic_s["http://example.com/p"] as ICollection<object>;
            var result = objects.Remove(0);

            Assert.IsFalse(result);

            var control = new Graph();
            control.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            Assert.AreEqual(control, g);
        }

        [TestMethod]
        public void getenumerator()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();

            var objects = dynamic_s["http://example.com/p"] as IEnumerable;

            foreach (var o in objects)
            {
                Assert.IsInstanceOfType(o, typeof(DynamicNode));
            }
        }
    }
}
