namespace Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using VDS.RDF;

    public class DynamicGraphMetaObject : DynamicMetaObject
    {
        private readonly DynamicGraph graph;

        public DynamicGraphMetaObject(Expression parameter, DynamicGraph graph) : base(parameter, BindingRestrictions.Empty, graph)
        {
            this.graph = graph;
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            var subjectIndex = indexes[0].Value;

            if (subjectIndex == null)
            {
                throw new ArgumentNullException("Can't work with null index", "indexes");
            }

            var result = GetIndex(subjectIndex);

            if (result == null)
            {
                return base.BindGetIndex(binder, indexes);
            }

            return new DynamicMetaObject(Expression.Convert(Expression.Constant(result), binder.ReturnType), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            if (this.graph.subjectBaseUri == null)
            {
                throw new InvalidOperationException("Can't get member without baseUri.");
            }

            var result = this.GetIndex(binder.Name);

            if (result == null)
            {
                return base.BindGetMember(binder);
            }

            return new DynamicMetaObject(Expression.Convert(Expression.Constant(result), binder.ReturnType), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            this.SetIndex(indexes[0].Value, value.Value);

            return new DynamicMetaObject(Expression.Convert(Expression.Constant(value.Value), binder.ReturnType), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            if (this.graph.subjectBaseUri == null)
            {
                throw new InvalidOperationException("Can't set member without baseUri.");
            }

            this.SetIndex(binder.Name, value.Value);

            return new DynamicMetaObject(Expression.Convert(Expression.Constant(value.Value), binder.ReturnType), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
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

            var valueDictionary = DynamicGraphMetaObject.ConvertToDictionary(value);
            //var targetMeta = targetNode.GetMetaObject(this.Expression) as DynamicNodeMetaObject;

            foreach (DictionaryEntry entry in valueDictionary)
            {
                //targetMeta.SetIndex(key, valueDictionary[key]);
                (targetNode as dynamic)[entry.Key] = entry.Value;
            }
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
    }
}
