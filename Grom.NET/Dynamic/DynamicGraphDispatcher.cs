namespace Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using VDS.RDF;

    internal class DynamicGraphDispatcher : DynamicObject
    {
        private DynamicGraph graph;

        internal DynamicGraphDispatcher(DynamicGraph graph)
        {
            this.graph = graph;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            var subjectIndex = indexes[0];

            if (subjectIndex == null)
            {
                throw new ArgumentNullException("Can't work with null index", "indexes");
            }

            result = this.GetIndex(subjectIndex);

            if (result == null)
            {
                return false;
            }

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this.graph.subjectBaseUri == null)
            {
                throw new InvalidOperationException("Can't get member without baseUri.");
            }

            result = this.GetIndex(binder.Name);

            if (result == null)
            {
                return false;
            }
            
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            this.SetIndex(indexes[0], value);

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (this.graph.subjectBaseUri == null)
            {
                throw new InvalidOperationException("Can't set member without baseUri.");
            }

            this.SetIndex(binder.Name, value);

            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var subjects = this.graph
                .Triples
                .Select(triple => triple.Subject)
                .UriNodes()
                .Distinct();

            return DynamicHelper.ConvertToNames(subjects, this.graph.subjectBaseUri);
        }

        private DynamicUriNode GetIndex(object subject)
        {
            var subjectNode = DynamicHelper.ConvertToNode(subject, this.graph, this.graph.subjectBaseUri);

            return this.graph.Triples
                .SubjectNodes
                .UriNodes()
                .Where(node => node.Equals(subjectNode))
                .Select(node => node.AsDynamic(this.graph.predicateBaseUri, this.graph.collapseSingularArrays))
                .SingleOrDefault();
        }

        private void SetIndex(object index, object value)
        {
            var targetNode = this.GetDynamicNodeByIndexOrCreate(index);

            if (value == null)
            {
                this.RetractWithSubject(targetNode);

                return;
            }

            var valueDictionary = DynamicGraphDispatcher.ConvertToDictionary(value);
            var dynamicTarget = targetNode as dynamic;

            foreach (DictionaryEntry entry in valueDictionary)
            {
                dynamicTarget[entry.Key] = entry.Value;
            }
        }

        private DynamicUriNode GetDynamicNodeByIndexOrCreate(object subjectIndex)
        {
            var result = this.GetIndex(subjectIndex);

            if (result == null)
            {
                var subjectNode = DynamicHelper.ConvertToNode(subjectIndex, this.graph, this.graph.subjectBaseUri);

                result = subjectNode.AsDynamic(this.graph.predicateBaseUri, this.graph.collapseSingularArrays);
            }

            return result;
        }

        private void RetractWithSubject(DynamicUriNode targetNode)
        {
            var triples = this.graph.GetTriplesWithSubject(targetNode).ToArray();

            this.graph.Retract(triples);
        }

        private static IDictionary ConvertToDictionary(object value)
        {
            if (value is IDictionary valueDictionary)
            {
                return valueDictionary;
            }

            valueDictionary = new Dictionary<object, object>();

            var properties = value.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Where(p => p.GetIndexParameters().Count() == 0);

            if (!properties.Any())
            {
                throw new ArgumentException($"Value type {value.GetType()} lacks readable public instance properties.", "value");
            }

            foreach (var property in properties)
            {
                valueDictionary[property.Name] = property.GetValue(value);
            }

            return valueDictionary;
        }
    }
}
