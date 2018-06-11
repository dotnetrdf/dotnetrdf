namespace Dynamic
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Nodes;

    internal class DynamicObjectCollection : ICollection<object>
    {
        private readonly DynamicNode subject;
        private readonly IUriNode predicate;

        internal DynamicObjectCollection(DynamicNode subject, IUriNode predicate)
        {
            this.subject = subject;
            this.predicate = predicate;
        }

        public int Count =>
            this.Get().Count();

        public bool IsReadOnly =>
            false;

        public void Add(object item)
        {
            var objectList = this.Get().ToList();
            objectList.Add(item);

            this.Set(objectList);
        }

        public void Clear() =>
            this.Set(null);

        public bool Contains(object item) =>
            this.Get().Contains(item);

        public void CopyTo(object[] array, int index) =>
            this.Get().ToArray().CopyTo(array, index);

        public IEnumerator<object> GetEnumerator() =>
            this.Get().GetEnumerator();

        public bool Remove(object item)
        {
            var objectList = this.Get().ToList();

            if (!objectList.Remove(item))
                return false;

            this.Set(objectList);

            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            this.GetEnumerator();

        private void Set(object value) =>
            (this.subject as IDynamicObject).SetIndex(new[] { this.predicate }, value);

        private IEnumerable<object> Get() =>
            from triple
            in this.subject.Graph.GetTriplesWithSubjectPredicate(this.subject, this.predicate)
            select this.ConvertToObject(triple.Object);

        private object ConvertToObject(INode objectNode)
        {
            switch (objectNode.AsValuedNode())
            {
                case IUriNode uriNode:
                case IBlankNode blankNode:
                    return objectNode.AsDynamic(this.subject.BaseUri);

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
                    return objectNode.ToString();
            }
        }
    }
}
