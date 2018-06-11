namespace Dynamic
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;

    [TestClass]
    public class ObjectCollectionTests
    {
        [TestMethod]
        public void Supports_add()
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
        public void Supports_blank_nodes()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> _:o .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();

            var actual = ((dynamic_s["http://example.com/p"] as IEnumerable<object>).Single() as IBlankNode).InternalID;
            var expected = "o";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Supports_clear()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();

            var objects = dynamic_s["http://example.com/p"] as ICollection<object>;
            objects.Clear();

            Assert.IsTrue(g.IsEmpty);
        }

        [TestMethod]
        public void Supports_contains()
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
        public void Supports_copy_to()
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
        public void Supports_count()
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
        public void Supports_get_enumerator()
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

        [TestMethod]
        public void Supports_native_data_types()
        {
            var g = new Graph();
            g.LoadFromString(@"
<http://example.com/s> <http://example.com/xsdDouble> ""0""^^<http://www.w3.org/2001/XMLSchema#double> .
<http://example.com/s> <http://example.com/xsdFloat> ""0""^^<http://www.w3.org/2001/XMLSchema#float> .
<http://example.com/s> <http://example.com/xsdDecimal> ""0""^^<http://www.w3.org/2001/XMLSchema#decimal> .
<http://example.com/s> <http://example.com/xsdBoolean> ""false""^^<http://www.w3.org/2001/XMLSchema#boolean> .
<http://example.com/s> <http://example.com/xsdDatetime> ""1900-01-01""^^<http://www.w3.org/2001/XMLSchema#dateTime> .
<http://example.com/s> <http://example.com/xsdTimespan> ""P1D""^^<http://www.w3.org/2001/XMLSchema#duration> .
<http://example.com/s> <http://example.com/xsdNumeric> ""0""^^<http://www.w3.org/2001/XMLSchema#int> .
<http://example.com/s> <http://example.com/xsdDefault> """"^^<http://example.com/datatype> .
");

            var dynamic_s = g.Triples.First().Subject.AsDynamic(new Uri("http://example.com/"));

            Assert.IsInstanceOfType((dynamic_s.xsdDouble as IEnumerable<object>).Single(), typeof(double));
            Assert.IsInstanceOfType((dynamic_s.xsdFloat as IEnumerable<object>).Single(), typeof(float));
            Assert.IsInstanceOfType((dynamic_s.xsdDecimal as IEnumerable<object>).Single(), typeof(decimal));
            Assert.IsInstanceOfType((dynamic_s.xsdBoolean as IEnumerable<object>).Single(), typeof(bool));
            Assert.IsInstanceOfType((dynamic_s.xsdDatetime as IEnumerable<object>).Single(), typeof(DateTimeOffset));
            Assert.IsInstanceOfType((dynamic_s.xsdTimespan as IEnumerable<object>).Single(), typeof(TimeSpan));
            Assert.IsInstanceOfType((dynamic_s.xsdNumeric as IEnumerable<object>).Single(), typeof(long));
            Assert.IsInstanceOfType((dynamic_s.xsdDefault as IEnumerable<object>).Single(), typeof(string));
        }

        [TestMethod]
        public void Supports_read_write()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();

            var result = dynamic_s["http://example.com/"] as ICollection<object>;

            Assert.IsFalse(result.IsReadOnly);
        }

        [TestMethod]
        public void Supports_remove()
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
        public void Supports_remove_with_missing_item()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();

            var objects = dynamic_s["http://example.com/p"] as ICollection<object>;
            var result = objects.Remove(0);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Supports_uri_nodes()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();

            var actual = ((dynamic_s["http://example.com/p"] as IEnumerable<object>).Single() as IUriNode).Uri;
            var expected = new Uri("http://example.com/o");

            Assert.AreEqual(expected, actual);
        }
    }
}
