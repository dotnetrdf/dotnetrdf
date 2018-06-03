namespace Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using VDS.RDF;

    public class DynamicObjectCollection : ICollection<object>, ICollection
    {
        private readonly dynamic subject;
        private readonly IUriNode predicate;
        private IEnumerable<object> objects;

        public DynamicObjectCollection(dynamic subject, IUriNode predicate, IEnumerable<object> objects)
        {
            this.subject = subject;
            this.predicate = predicate;
            this.objects = objects;
        }

        public int Count => this.objects.Count();

        public bool IsReadOnly => false;

        public void Add(object item)
        {
            var objectList = this.objects.ToList();
            objectList.Add(item);

            this.Set(objectList);
        }

        public void Clear()
        {
            this.Set(null);
        }

        public bool Contains(object item) => this.objects.Contains(item);

        public void CopyTo(object[] array, int index) => (this as ICollection).CopyTo(array, index);

        public IEnumerator<object> GetEnumerator() => this.objects.GetEnumerator();

        public bool Remove(object item)
        {
            var objectList = this.objects.ToList();

            if (!objectList.Remove(item))
            {
                return false;
            }

            this.Set(objectList);

            return true;
        }

        object ICollection.SyncRoot => throw new NotImplementedException();

        bool ICollection.IsSynchronized => throw new NotImplementedException();

        void ICollection.CopyTo(Array array, int index) => this.objects.ToArray().CopyTo(array, index);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        private void Set(object value)
        {
            this.subject[this.predicate] = value;

            this.objects = (this.subject[this.predicate] as DynamicObjectCollection).objects;
        }
    }
}
