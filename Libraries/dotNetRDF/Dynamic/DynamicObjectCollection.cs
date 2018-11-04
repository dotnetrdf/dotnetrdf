namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF.Nodes;

    public class DynamicObjectCollection : ICollection<object>
    {
        private readonly DynamicNode subject;
        private readonly INode predicate;

        public DynamicObjectCollection(DynamicNode subject, INode predicate)
        {
            if (subject is null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            this.subject = subject;
            this.predicate = predicate;
        }

        private IEnumerable<object> Objects
        {
            get
            {
                return
                    from triple
                    in subject.Graph.GetTriplesWithSubjectPredicate(subject, predicate)
                    select ConvertToObject(triple.Object);
            }
        }

        public int Count => Objects.Count();

        public bool IsReadOnly => false;

        public void Add(object item) => subject.Add(predicate, item);

        public void Clear() => subject.Remove(predicate);

        public bool Contains(object item) => ((IDictionary<INode, object>)subject).Contains(new KeyValuePair<INode, object>(predicate, item));

        public void CopyTo(object[] array, int index) => Objects.ToArray().CopyTo(array, index);

        public IEnumerator<object> GetEnumerator() => Objects.GetEnumerator();

        public bool Remove(object item) => ((IDictionary<INode, object>)subject).Remove(new KeyValuePair<INode, object>(predicate, item));

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private object ConvertToObject(INode node)
        {
            if (node.IsListRoot(node.Graph))
            {
                // TODO: Create dynamic list
                return node.Graph.GetListItems(node).Select(ConvertToObject).ToList();
            }

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

                case StringNode stringNode when stringNode.DataType is null && string.IsNullOrEmpty(stringNode.Language):
                    return stringNode.AsString();

                default:
                    return node;
            }
        }
    }
}
