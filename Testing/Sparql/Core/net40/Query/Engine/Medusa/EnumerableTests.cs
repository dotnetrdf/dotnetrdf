using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace VDS.RDF.Query.Engine.Medusa
{
    [TestFixture]
    public class EnumerableTests
    {
        private void Check<T>(IEnumerable<T> expected, IEnumerable<T> actual) where T : struct
        {
            Console.WriteLine("Expected:");
            TestTools.PrintEnumerableStruct(expected, ",");
            Console.WriteLine();
            Console.WriteLine("Actual:");
            TestTools.PrintEnumerableStruct(actual, ",");
            Console.WriteLine();

            IEnumerator<T> expectedEnumerator = expected.GetEnumerator();
            IEnumerator<T> actualEnumerator = actual.GetEnumerator();

            int i = 0;
            while (expectedEnumerator.MoveNext())
            {
                T expectedItem = expectedEnumerator.Current;
                i++;
                if (!actualEnumerator.MoveNext()) Assert.Fail(String.Format("Actual enumerator was exhaused at Item {0} when next Item {1} was expected", i, expectedItem));
                T actualItem = actualEnumerator.Current;

                Assert.AreEqual(expectedItem, actualItem, String.Format("Enumerators mismatched at Item {0}", i));
            }
            if (actualEnumerator.MoveNext()) Assert.Fail("Actual enumerator has additional unexpected items");
        }

        private void Exhaust<T>(IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext()) { }
        }

        private static Object[] SkipAndTakeData = new object[]
                                                  {
                                                      new object[] { 1, 50, 10 },
                                                      new object[] { 1, 10, 10 },
                                                      new object[] { 1, 10, 5 }
                                                  };

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void EnumeratorBeforeFirstElement1()
        {
            IEnumerable<int> data = Enumerable.Range(1, 10);
            IEnumerator<int> enumerator =  new LongTakeEnumerator<int>(data.GetEnumerator(), 1);
            int i = enumerator.Current;
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void EnumeratorBeforeFirstElement2()
        {
            IEnumerable<int> data = Enumerable.Range(1, 10);
            IEnumerator<int> enumerator = new LongSkipEnumerator<int>(data.GetEnumerator(), 1);
            int i = enumerator.Current;
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void EnumeratorBeforeFirstElement3()
        {
            IEnumerable<String> data = new String[] {"a", "b", "c"};
            IEnumerator<String> enumerator =  new LongTakeEnumerator<String>(data.GetEnumerator(), 1);
            String i = enumerator.Current;
            Assert.AreEqual(default(String), i);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void EnumeratorBeforeFirstElement4()
        {
            IEnumerable<String> data = new String[] { "a", "b", "c" };
            IEnumerator<String> enumerator = new LongSkipEnumerator<String>(data.GetEnumerator(), 1);
            String i = enumerator.Current;
            Assert.AreEqual(default(String), i);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void EnumeratorAfterLastElement1()
        {
            IEnumerable<int> data = Enumerable.Range(1, 10);
            IEnumerator<int> enumerator = new LongTakeEnumerator<int>(data.GetEnumerator(), 1);
            this.Exhaust(enumerator);
            int i = enumerator.Current;
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void EnumeratorAfterLastElement2()
        {
            IEnumerable<String> data = new String[] { "a", "b", "c" };
            IEnumerator<String> enumerator = new LongTakeEnumerator<String>(data.GetEnumerator(), 1);
            this.Exhaust(enumerator);
            String i = enumerator.Current;
            Assert.AreEqual(default(String), i);
        }
            
        [TestCaseSource("SkipAndTakeData")]
        public void LongSkipEnumerable(int start, int count, int skip)
        {
            IEnumerable<int> data = Enumerable.Range(start, count);
            IEnumerable<int> expected = data.Skip(skip);
            IEnumerable<int> actual = new LongSkipEnumerable<int>(data, skip);

            Assert.AreEqual(expected.Count(), actual.Count());
            Check(expected, actual);
        }

        [TestCaseSource("SkipAndTakeData")]
        public void LongTakeEnumerable(int start, int count, int take)
        {
            IEnumerable<int> data = Enumerable.Range(start, count);
            IEnumerable<int> expected = data.Take(take);
            IEnumerable<int> actual = new LongTakeEnumerable<int>(data, take);

            Assert.AreEqual(expected.Count(), actual.Count());
            Check(expected, actual);
        }
    }
}
