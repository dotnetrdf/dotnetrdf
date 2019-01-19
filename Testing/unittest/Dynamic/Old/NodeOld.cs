namespace VDS.RDF.Dynamic.Old
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

    public class NodeTests
    {
        private static readonly Uri exampleBase = UriFactory.Create("http://example.com/");
        private static readonly Uri exampleSubjectUri = new Uri(exampleBase, "s");
        private static readonly Uri ex_p_u = new Uri(exampleBase, "p");
        private static readonly Uri exampleObjectUri = new Uri(exampleBase, "o");
        private static readonly IUriNode ex_s = new NodeFactory().CreateUriNode(exampleSubjectUri);
        private static readonly IUriNode ex_p = new NodeFactory().CreateUriNode(ex_p_u);
        private static readonly IUriNode example_o = new NodeFactory().CreateUriNode(exampleObjectUri);
        private static readonly IGraph spoGraph = GenerateSPOGraph();

        private static IGraph GenerateSPOGraph()
        {
            var spoGraph = new Graph();
            spoGraph.Assert(ex_s, ex_p, example_o);

            return spoGraph;
        }

        public void Member_names_are_predicate_uris()
        {
            var s = spoGraph.GetTriplesWithSubject(ex_s).Single().Subject;
            var d = s.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "http://example.com/p";

            Assert.Contains(element, collection);
        }

        public void Member_names_reduce_to_qnames()
        {
            var g = GenerateSPOGraph();
            g.NamespaceMap.AddNamespace("ex", exampleBase);
            var s = g.GetTriplesWithSubject(ex_s).Single().Subject;
            var d = s.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "ex:p";

            Assert.Contains(element, collection);
        }

        public void Member_names_reduce_to_qnames_with_empty_prefix()
        {
            var g = GenerateSPOGraph();
            g.NamespaceMap.AddNamespace(string.Empty, exampleBase);
            var s = g.GetTriplesWithSubject(ex_s).Single().Subject;
            var d = s.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = ":p";

            Assert.Contains(element, collection);
        }

        public void Member_names_become_relative_to_base()
        {
            var s = spoGraph.GetTriplesWithSubject(ex_s).Single().Subject;
            var d = s.AsDynamic(exampleBase) as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "p";

            Assert.Contains(element, collection);
        }

        public void Member_names_become_relative_to_hash_base()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/#s> <http://example.com/#p> <http://example.com/#o> .");
            var s = g.Triples.Single().Subject;
            var d = s.AsDynamic(UriFactory.Create("http://example.com/#")) as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "p";

            Assert.Contains(element, collection);
        }
    }
}
