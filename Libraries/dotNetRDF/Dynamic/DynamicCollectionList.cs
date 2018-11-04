namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class DynamicCollectionList : IList<object>
    {
        private readonly INode root;
        private readonly Uri baseUri;

        public DynamicCollectionList(INode root, Uri baseUri)
        {
            if (root is null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (root.Graph is null)
            {
                throw new InvalidOperationException("Node must have graph");
            }

            this.root = root;
            this.baseUri = baseUri;
        }

        private IEnumerable<INode> Nodes
        {
            get
            {
                return root.Graph.GetListItems(root);
            }
        }

        private IEnumerable<object> Objects
        {
            get
            {
                return Nodes.Select(n => DynamicHelper.ConvertNode(n, baseUri));
            }
        }

        public object this[int index]
        {
            get
            {
                return Objects.ToList()[index];
            }

            set
            {
                RemoveAt(index);
                Insert(index, value);
            }
        }

        public int Count
        {
            get
            {
                return Nodes.Count();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(object item)
        {
            root.Graph.AddToList(root, DynamicHelper.ConvertObject(item, root.Graph).AsEnumerable());
        }

        public void Clear()
        {
            root.Graph.RetractList(root);
        }

        public bool Contains(object item)
        {
            return Objects.Contains(item);
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            Objects.ToArray().CopyTo(array, arrayIndex);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return Objects.GetEnumerator();
        }

        public int IndexOf(object item)
        {
            return Objects.ToList().IndexOf(item);
        }

        public void Insert(int index, object item)
        {
            var nodes = Nodes.ToList();
            Clear();
            nodes.Insert(index, DynamicHelper.ConvertObject(item, root.Graph));
            root.Graph.AssertList(nodes);
        }

        public bool Remove(object item)
        {
            if (Contains(item))
            {
                var nodes = Nodes.ToList();
                Clear();
                nodes.Remove(DynamicHelper.ConvertObject(item, root.Graph));
                root.Graph.AssertList(nodes);

                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            Remove(this[index]);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
