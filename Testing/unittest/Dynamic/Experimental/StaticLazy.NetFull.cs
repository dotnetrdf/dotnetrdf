namespace VDS.RDF.Dynamic.Experimental
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF.Dynamic;
    using Xunit;

    public class Container : DynamicNode
    {
        public Container(INode node, Uri baseUri) : base(node, baseUri) { }

        public ICollection<string> Names => new DynamicObjectCollection<string>(this, "name");

        public ICollection<Item> Children => new DynamicObjectCollection<Item>(this, "child");
    }

    public class Item : DynamicNode
    {
        public Item(INode node, Uri baseUri) : base(node, baseUri) { }

        public ICollection<string> Names => new DynamicObjectCollection<string>(this, "name");
    }

    internal class DynamicObjectCollection<T> : ICollection<T>
    {
        private readonly DynamicNode subject;
        private readonly string predicate;

        public DynamicObjectCollection(DynamicNode subject, string predicate)
        {
            this.subject = subject;
            this.predicate = predicate;
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
                value = ctor.Invoke(new[] { value, this.subject.BaseUri });
            }

            return (T)value;
        }

        private ICollection<object> Objects => (ICollection<object>)this.subject[this.predicate];
    }

    public class StaticLazy
    {
        public void MyTestMethod()
        {
            var g1 = new Graph();
            g1.LoadFromString(@"
<http://example.com/s> <http://example.com/name> ""0"" .
<http://example.com/s> <http://example.com/child> _:x .
_:x <http://example.com/name> ""1"" .
");

            var container = new Container(g1.Triples.SubjectNodes.First(), new Uri("http://example.com/"));

            Assert.Equal("0", container.Names.First());
            Assert.Equal(g1.Nodes.BlankNodes().Single(), container.Children.First());
            Assert.Equal("1", container.Children.First().Names.Single());

            var g2 = new Graph();
            g2.LoadFromString(@"
<http://example.com/s> <http://example.com/child> _:x .
_:x <http://example.com/name> ""1"" .
");

            container.Names.Clear();
            Assert.Equal(g2, g1);

            var g3 = new Graph();
            g3.LoadFromString(@"
<http://example.com/s> <http://example.com/name> ""n1"" .
<http://example.com/s> <http://example.com/name> ""n2"" .
<http://example.com/s> <http://example.com/child> _:x .
_:x <http://example.com/name> ""1"" .
");

            container.Names.Add("n1");
            container.Names.Add("n2");
            Assert.Equal(g3, g1);
        }
    }
}
