
using VDS.RDF;
using System.Collections.Generic;
using VDS.RDF.Query.Spin;
using org.topbraid.spin.vocabulary;
using System;
using System.Linq;
using VDS.RDF.Query.Spin.Util;

namespace org.topbraid.spin.util
{

    /**
     * A native implementation of swa:localRangeAtClass, to optimize performance.
     * 
     * The original spin:body is
     * 
     * 		SELECT (COALESCE(
     * 				spif:walkObjects(?class, rdfs:subClassOf, swa:allValuesFromFunctor, ?property), 
     * 				spif:walkObjects(?class, rdfs:subClassOf, swa:splValueTypeFunctor, ?property), 
     * 				swa:globalRange(?property), 
     * 				swa:defaultRange(?property)) AS ?result)
     * 		WHERE {
     *      }
     * 
     * @author Holger Knublauch
     */
    public class LocalRangeAtClassFunctionHelper : AbstractFunction2
    {

        override protected INode exec(INode cls, INode property, FunctionEnv env)
        {
            IGraph graph = env.getActiveGraph();
            INode result = run(cls, property, graph);
            return result;
        }


        private static HashSet<INode> datatypeOrAnnotationProperty = new HashSet<INode>();

        static LocalRangeAtClassFunctionHelper()
        {
            datatypeOrAnnotationProperty.Add(RDFUtil.CreateUriNode(OWL.DatatypeProperty));
            datatypeOrAnnotationProperty.Add(RDFUtil.CreateUriNode(OWL.AnnotationProperty));
        }


        public static INode run(INode cls, INode property, IGraph graph)
        {
            INode result = walk(property, cls, graph, new HashSet<INode>());
            if (result == null)
            {
                result = getGlobalRange(property, graph);
                if (result == null)
                {
                    result = getDefaultRange(property, graph);
                }
            }
            return result;
        }


        private static INode getDefaultRange(INode property, IGraph graph)
        {
            if (instanceOf(property, datatypeOrAnnotationProperty, graph))
            {
                return RDFUtil.CreateUriNode(XSD.string_);
            }
            else
            {
                return RDFUtil.CreateUriNode(RDFS.Resource);
            }
        }


        private static INode getGlobalRange(INode property, IGraph graph)
        {
            return getGlobalRangeHelper(property, graph, new HashSet<INode>());
        }


        private static INode getGlobalRangeHelper(INode property, IGraph graph, HashSet<INode> reached)
        {
            reached.Add(property);
            INode range = Model.GetObject(property, RDFUtil.CreateUriNode(RDFS.range));
            if (range != null)
            {
                return range;
            }
            foreach (Triple t in Model.GetTriplesWithSubjectPredicate(property, RDFUtil.CreateUriNode(RDFS.subPropertyOf)).ToList())
            {
                INode superProperty = t.Object;
                if (!reached.Contains(superProperty))
                {
                    INode global = getGlobalRangeHelper(superProperty, graph, reached);
                    if (global != null)
                    {
                        return global;
                    }
                }
            }
            return null;
        }


        private static INode getObject(INode subject, INode predicate, IGraph graph)
        {
            IEnumerator<Triple> it = Model.GetTriplesWithSubjectPredicate(subject, predicate).GetEnumerator();
            if (it.MoveNext())
            {
                INode obj = it.Current.Object;
                it.Dispose();
                return obj;
            }
            return null;
        }


        private static bool instanceOf(INode instance, HashSet<INode> matchTypes, IGraph graph)
        {
            HashSet<INode> reachedTypes = new HashSet<INode>();
            foreach (Triple t in Model.GetTriplesWithSubjectPredicate(instance, RDFUtil.CreateUriNode(RDF.type)).ToList())
            {
                if (instanceOfHelper(matchTypes, t.Object, graph, reachedTypes))
                {
                    return true;
                }
            }
            return false;
        }


        private static bool instanceOfHelper(HashSet<INode> matchTypes, INode type, IGraph graph, HashSet<INode> reachedTypes)
        {
            if (reachedTypes.Contains(type))
            {
                return false;
            }

            if (matchTypes.Contains(type))
            {
                return true;
            }

            reachedTypes.Add(type);

            IEnumerator<Triple> it = Model.GetTriplesWithSubjectPredicate(type, RDFUtil.CreateUriNode(RDFS.subClassOf)).GetEnumerator();
            while (it.MoveNext())
            {
                INode superClass = it.Current.Object;
                if (instanceOfHelper(matchTypes, superClass, graph, reachedTypes))
                {
                    it.Dispose();
                    return true;
                }
            }

            return false;
        }


        private static INode walk(INode property, INode type, IGraph graph, HashSet<INode> classes)
        {

            classes.Add(type);

            List<INode> superClasses = new List<INode>();
            {
                IEnumerator<Triple> it = Model.GetTriplesWithSubjectPredicate(type, RDFUtil.CreateUriNode(RDFS.subClassOf)).GetEnumerator();
                while (it.MoveNext())
                {
                    INode superClass = it.Current.Object;
                    if (superClass is IBlankNode &&
                            Model.ContainsTriple(superClass, RDFUtil.CreateUriNode(OWL.onProperty), property))
                    {
                        INode allValuesFrom = Model.GetObject(superClass, RDFUtil.CreateUriNode(OWL.allValuesFrom));
                        if (allValuesFrom != null)
                        {
                            it.Dispose();
                            return allValuesFrom;
                        }
                    }
                    else if (superClass is IUriNode)
                    {
                        superClasses.Add(superClass);
                    }
                }
            }

            {
                IEnumerator<Triple> it = Model.GetTriplesWithSubjectPredicate(type, RDFUtil.CreateUriNode(SPIN.constraint)).GetEnumerator();
                while (it.MoveNext())
                {
                    INode constraint = it.Current.Object;
                    if (Model.ContainsTriple(constraint, RDFUtil.CreateUriNode(SPL.predicate), property) &&
                            Model.ContainsTriple(constraint, RDFUtil.CreateUriNode(RDF.type), RDFUtil.CreateUriNode(SPL.Argument)))
                    {
                        INode valueType = Model.GetObject(constraint, RDFUtil.CreateUriNode(SPL.valueType));
                        if (valueType != null)
                        {
                            it.Dispose();
                            return valueType;
                        }
                    }
                }
            }

            foreach (INode superClass in superClasses)
            {
                if (!classes.Contains(superClass))
                {
                    INode result = walk(property, superClass, graph, classes);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}