namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF.Nodes;

    internal class DynamicObjectCollection : ICollection<object>
    {
        private readonly DynamicNode subject;
        private readonly INode predicate;

        internal DynamicObjectCollection(DynamicNode subject, INode predicate)
        {
            this.subject = subject;
            this.predicate = predicate;
        }

        public int Count => this.Get().Count();

        public bool IsReadOnly => false;

        public void Add(object item)
        {
            this.subject.Add(this.predicate, item);
        }

        public void Clear()
        {
            this.subject.Remove(this.predicate);
        }

        public bool Contains(object item)
        {
            return ((IDictionary<INode, object>)this.subject).Contains(new KeyValuePair<INode, object>(this.predicate, item));
        }

        public void CopyTo(object[] array, int index)
        {
            this.Get().ToArray().CopyTo(array, index);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return this.Get().GetEnumerator();
        }

        public bool Remove(object item)
        {
            return ((IDictionary<INode, object>)this.subject).Remove(new KeyValuePair<INode, object>(this.predicate, item));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private IEnumerable<object> Get()
        {
            if (this.subject.Graph is null)
            {
                throw new InvalidOperationException("graph missing");
            }

            return
                from triple
                in this.subject.Graph.GetTriplesWithSubjectPredicate(this.subject, this.predicate)
                select ConvertToObject(triple.Object);
        }

        private object ConvertToObject(INode node)
        {
            switch (node.AsValuedNode())
            {
                case IUriNode uriNode:
                case IBlankNode blankNode:
                    return new DynamicNode(node, subject.BaseUri);

                case DoubleNode doubleNode:
                    return doubleNode.AsDouble();

                case FloatNode floatNode:
                    return floatNode.AsFloat();

                case DecimalNode decimalNode:
                    return decimalNode.AsDecimal();

                case BooleanNode booleanNode:
                    return booleanNode.AsBoolean();

                case DateTimeNode dateTimeNode:
                    return dateTimeNode.AsDateTimeOffset();

                case TimeSpanNode timeSpanNode:
                    return timeSpanNode.AsTimeSpan();

                case NumericNode numericNode:
                    return numericNode.AsInteger();

                default:
                    // TODO: What happens with language tags?
                    return node.ToString();
            }
        }
    }
}
