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
        private readonly DynamicGraph dynamicGraph;

        public DynamicGraphMetaObject(Expression parameter, DynamicGraph dynamicGraph) : base(parameter, BindingRestrictions.Empty, dynamicGraph)
        {
            this.dynamicGraph = dynamicGraph;
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
            if (this.dynamicGraph.subjectBaseUri == null)
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
            if (this.dynamicGraph.subjectBaseUri == null)
            {
                throw new InvalidOperationException("Can't set member without baseUri.");
            }

            this.SetIndex(binder.Name, value.Value);

            return new DynamicMetaObject(Expression.Convert(Expression.Constant(value.Value), binder.ReturnType), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var subjects = this.dynamicGraph
                .Triples
                .Select(triple => triple.Subject)
                .UriNodes()
                .Distinct();

            return DynamicHelper.ConvertToNames(subjects, this.dynamicGraph.subjectBaseUri);
        }

        private DynamicUriNode GetIndex(object subject)
        {
            var subjectNode = DynamicHelper.ConvertToNode(subject, this.dynamicGraph, this.dynamicGraph.subjectBaseUri);

            return this.dynamicGraph.Triples.SubjectNodes
                .UriNodes()
                .Where(node => node.Equals(subjectNode))
                .Select(node => new DynamicUriNode(node, this.dynamicGraph.predicateBaseUri, this.dynamicGraph.collapseSingularArrays))
                .SingleOrDefault();
        }

        private void SetIndex(object subjectIndex, object value)
        {
            var result = this.GetIndex(subjectIndex);
            if (result == null)
            {
                var subjectNode = DynamicHelper.ConvertToNode(subjectIndex, this.dynamicGraph, this.dynamicGraph.subjectBaseUri);
                result = new DynamicUriNode(subjectNode, this.dynamicGraph.predicateBaseUri, this.dynamicGraph.collapseSingularArrays);
            }

            if (value == null)
            {
                this.dynamicGraph.Retract(this.dynamicGraph.GetTriplesWithSubject(result).ToArray());
            }
            else
            {
                if (!(value is IDictionary valueDictionary))
                {
                    valueDictionary = new Dictionary<object, object>();

                    var properties = value.GetType()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                        .Where(p => p.GetIndexParameters().Count() == 0);

                    if (!properties.Any())
                    {
                        throw new ArgumentException($"Value type {value.GetType()} for subject {subjectIndex} lacks readable public instance properties.", "value");
                    }

                    foreach (var property in properties)
                    {
                        valueDictionary[property.Name] = property.GetValue(value);
                    }
                }

                foreach (var key in valueDictionary.Keys)
                {
                    var a = result.GetMetaObject(Expression.Empty()) as DynamicNodeMetaObject;
                    a.SetIndex(key, valueDictionary[key]);
                }
            }
        }
    }
}
