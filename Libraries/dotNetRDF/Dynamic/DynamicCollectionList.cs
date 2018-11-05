namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class DynamicCollectionList : IList<object>, IRdfCollection
    {
        private readonly INode node;
        private readonly Uri baseUri;

        public DynamicCollectionList(INode node, Uri baseUri = null)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (node.Graph is null)
            {
                throw new InvalidOperationException("Node must have graph");
            }

            if (!node.IsListRoot(node.Graph))
            {
                throw new InvalidOperationException("root must be list root");
            }

            this.node = node;
            this.baseUri = baseUri;
        }

        private IEnumerable<INode> Nodes
        {
            get
            {
                return node.Graph.GetListItems(node);
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
                if (index < 0 || !(index < Nodes.Count()))
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

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
            node.Graph.AddToList(node, DynamicHelper.ConvertObject(item, node.Graph).AsEnumerable());
        }

        public void Clear()
        {
            node.Graph.RetractList(node);
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
            nodes.Insert(index, DynamicHelper.ConvertObject(item, node.Graph));
            node.Graph.AssertList(nodes);
        }

        public bool Remove(object item)
        {
            if (Contains(item))
            {
                var nodes = Nodes.ToList();
                Clear();
                nodes.Remove(DynamicHelper.ConvertObject(item, node.Graph));
                node.Graph.AssertList(nodes);

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
