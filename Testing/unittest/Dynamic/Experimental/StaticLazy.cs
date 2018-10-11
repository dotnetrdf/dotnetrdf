namespace VDS.RDF.Dynamic.Experimental
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF.Dynamic;
    using Xunit;

    public class C1 : DynamicNode
    {
        private readonly Uri baseUri;

        public C1(INode node, Uri baseUri) : base(node, baseUri)
        {
            this.baseUri = baseUri;
        }

        public ICollection<string> Names => new x<string>(this, "name", null);

        public ICollection<C2> Children => new x<C2>(this, "child", this.baseUri);
    }

    public class C2 : DynamicNode
    {
        public C2(INode node, Uri baseUri) : base(node, baseUri) { }

        public ICollection<string> Names => new x<string>(this, "name", null);
    }

    internal class x<T> : ICollection<T>
    {
        private readonly dynamic subject;
        private readonly string predicate;
        private readonly Uri baseUri;

        public x(dynamic subject, string predicate, Uri baseUri)
        {
            this.subject = subject;
            this.predicate = predicate;
            this.baseUri = baseUri;
        }

        public int Count => this.Objects.Count();

        public bool IsReadOnly => this.Objects.IsReadOnly;

        public void Add(T item)
        {
            this.Objects.Add(item);
        }

        public void Clear()
        {
            this.Objects.Clear();
        }

        public bool Contains(T item)
        {
            return this.Objects.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.Objects.CopyTo(array.Cast<object>().ToArray(), arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.Objects.Select(this.Convert).GetEnumerator();
        }

        public bool Remove(T item)
        {
            return this.Objects.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private T Convert(object value)
        {
            var type = typeof(T);

            if (type.IsSubclassOf(typeof(DynamicNode)))
            {
                var ctor = type.GetConstructor(new[] { typeof(INode), typeof(Uri) });
                value = ctor.Invoke(new[] { value, this.baseUri });
            }

            return (T)value;
        }

        private ICollection<object> Objects => this.subject[this.predicate];
    }

    public class StaticLazy
    {
        [Fact]
        public void MyTestMethod()
        {
            var g1 = new Graph();
            g1.LoadFromString(@"
<http://example.com/s> <http://example.com/name> ""0"" .
<http://example.com/s> <http://example.com/child> _:x .
_:x <http://example.com/name> ""1"" .
");

            var c1 = new C1(g1.Triples.SubjectNodes.First(), new Uri("http://example.com/"));

            Assert.Equal("0", c1.Names.First());
            Assert.Equal(g1.Nodes.BlankNodes().Single(), c1.Children.First());
            Assert.Equal("1", c1.Children.First().Names.Single());

            var g2 = new Graph();
            g2.LoadFromString(@"
<http://example.com/s> <http://example.com/child> _:x .
_:x <http://example.com/name> ""1"" .
");

            c1.Names.Clear();
            Assert.Equal(g2, g1);

            var g3 = new Graph();
            g3.LoadFromString(@"
<http://example.com/s> <http://example.com/name> ""n1"" .
<http://example.com/s> <http://example.com/name> ""n2"" .
<http://example.com/s> <http://example.com/child> _:x .
_:x <http://example.com/name> ""1"" .
");

            c1.Names.Add("n1");
            c1.Names.Add("n2");
            Assert.Equal(g3, g1);
        }
    }
}
