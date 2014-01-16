/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Storage;
using System;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query;
namespace org.topbraid.spin.util
{

    /**
     * Some convenience methods to operate on Jena Models.
     * 
     * These methods are not as stable as the rest of the API, but
     * they may be of general use.
     * 
     * @author Holger Knublauch
     */
    public class JenaUtil
    {

        // Unstable
        private static JenaUtilHelper helper = new JenaUtilHelper();


        private static IUpdateableStorage dummyModel = JenaUtil.createDefaultModel();

        private const String WITH_IMPORTS_PREFIX = "http://rdfex.org/withImports?uri=";



        /**
         * Sets the helper which allows the behavior of some JenaUtil
         * methods to be modified by the system using the SPIN library.
         * Note: Should not be used outside of TopBraid - not stable.
         * @param h  the JenaUtilHelper
         * @return the old helper
         */
        public static JenaUtilHelper setHelper(JenaUtilHelper h)
        {
            JenaUtilHelper old = helper;
            helper = h;
            return old;
        }


        /**
         * Gets the current helper object.
         * Note: Should not be used outside of TopBraid - not stable.
         * @return the helper
         */
        public static JenaUtilHelper getHelper()
        {
            return helper;
        }


        /**
         * Adds all sub-properties of a given property as long as they don't have their own
         * rdfs:domain.  This is useful to determine inheritance.
         * @param property  the property ot add the sub-properties of
         * @param results  the Set to add the results to
         * @param reached  a Set used to track which ones were already reached
         */
        public static void addDomainlessSubProperties(INode property, HashSet<INode> results, HashSet<INode> reached)
        {
            IEnumerator<Triple> subs = property.getModel().listStatements(null, RDFS.subPropertyOf, property);
            while (subs.MoveNext())
            {
                INode subProperty = subs.Current.Subject;
                if (!reached.Contains(subProperty))
                {
                    reached.Contains(subProperty);
                    if (!Model.hasProperty(subProperty, RDFS.domain))
                    {
                        results.Add(subProperty.getModel().getProperty(subProperty.Uri));
                        addDomainlessSubProperties(subProperty, results, reached);
                    }
                }
            }
        }


        /**
         * Populates a result set of resources reachable from a subject via zero or more steps with a given predicate.
         * Implementation note: the results set need only implement {@link HashSet#add(Object)}.
         * @param results   The transitive objects reached from subject via triples with the given predicate
         * @param subject
         * @param predicate
         */
        public static void addTransitiveObjects(HashSet<INode> results, INode subject, INode predicate)
        {
            helper.setGraphReadOptimization(true);
            try
            {
                addTransitiveObjects(results, new HashSet<INode>(), subject, predicate);
            }
            finally
            {
                helper.setGraphReadOptimization(false);
            }
        }


        private static void addTransitiveObjects(HashSet<INode> resources, HashSet<INode> reached,
                INode subject, INode predicate)
        {
            resources.Add(subject);
            reached.Add(subject);
            IEnumerator<Triple> it = subject.listProperties(predicate);
            try
            {
                while (it.MoveNext())
                {
                    INode obj = it.Current.Object;
                    if (obj is INode)
                    {
                        if (!reached.Contains(obj))
                        {
                            addTransitiveObjects(resources, reached, (INode)obj, predicate);
                        }
                    }
                }
            }
            finally
            {
                it.Dispose();
            }
        }


        private static void addTransitiveSubjects(HashSet<INode> reached, INode obj,
                INode predicate, ProgressMonitor monitor)
        {
            if (obj != null)
            {
                reached.Add(obj);
                IEnumerator<Triple> it = obj.getModel().listStatements(null, predicate, obj);
                try
                {
                    while (it.MoveNext())
                    {
                        if (monitor != null && monitor.isCanceled())
                        {
                            it.Dispose();
                            return;
                        }
                        INode subject = it.Current.Subject;
                        if (!reached.Contains(subject))
                        {
                            addTransitiveSubjects(reached, subject, predicate, monitor);
                        }
                    }
                }
                finally
                {
                    it.Dispose();
                }
            }
        }


        /**
         * Turns a SparqlResult into a Binding. 
         * @param map  the input SparqlResult
         * @return a Binding or null if the input is null
         */
        public static Binding asBinding(/*sealed*/ SparqlResult map)
        {
            if (map != null)
            {
                BindingHashMap result = new BindingHashMap();
                IEnumerator<String> varNames = map.varNames();
                while (varNames.MoveNext())
                {
                    String varName = varNames.Current;
                    INode node = map.get(varName);
                    if (node != null)
                    {
                        result.Add(Var.alloc(varName), node);
                    }
                }
                return result;
            }
            else
            {
                return null;
            }
        }


        /**
         * Turns a Binding into a SparqlResultSet.
         * @param binding  the Binding to convert
         * @return a SparqlResultSet
         */
        public static SparqlResultSet asQuerySolutionMap(Binding binding)
        {
            SparqlResultSet map = new SparqlResultSet();
            IEnumerator<Var> vars = binding.vars();
            while (vars.MoveNext())
            {
                Var var = vars.Current;
                INode node = binding.get(var);
                if (node != null)
                {
                    map.Add(var.getName(), dummyModel.asRDFNode(node));
                }
            }
            return map;
        }


        /**
         * Returns a set of resources reachable from an object via one or more reversed steps with a given predicate.
         */
        public static HashSet<INode> getAllTransitiveSubjects(INode obj, INode predicate,
                ProgressMonitor monitor)
        {
            HashSet<INode> set = new HashSet<INode>();
            helper.setGraphReadOptimization(true);
            try
            {
                addTransitiveSubjects(set, obj, predicate, monitor);
            }
            finally
            {
                helper.setGraphReadOptimization(false);
            }
            set.Remove(obj);
            return set;
        }


        /**
         * Casts a INode into a INode.
         * @param resource  the INode to cast
         * @return resource as an instance of INode
         */
        public static INode asProperty(INode resource)
        {
            if (resource is INode)
            {
                return (INode)resource;
            }
            else
            {
                return new PropertyImpl(resource, (IGraph)resource.getModel());
            }
        }


        /**
         * Creates a Set of Properties from a HashSet of Resources.
         * @param resources  the INode to cast
         * @return resource as an instance of INode
         */
        public static HashSet<INode> asProperties(HashSet<INode> resources)
        {
            HashSet<INode> rslt = new HashSet<INode>();
            foreach (INode r in resources)
            {
                rslt.Add(asProperty(r));
            }
            return rslt;
        }


        public static void collectBaseGraphs(Graph graph, HashSet<Graph> baseGraphs)
        {
            if (graph is MultiUnion)
            {
                MultiUnion union = (MultiUnion)graph;
                collectBaseGraphs(union.getBaseGraph(), baseGraphs);
                foreach (Object subGraph in union.getSubGraphs())
                {
                    collectBaseGraphs((Graph)subGraph, baseGraphs);
                }
            }
            else if (graph != null)
            {
                baseGraphs.Add(graph);
            }
        }


        /**
         * Creates a new Graph.  By default this will deliver a plain in-memory graph,
         * but other implementations may deliver graphs with concurrency support and
         * other features.
         * @return a default graph
         * @see #createDefaultModel()
         */
        public static Graph createDefaultGraph()
        {
            return getHelper().createDefaultGraph();
        }


        /**
         * Wraps the result of {@link #createDefaultGraph()} into a IUpdateableStorage and initializes namespaces. 
         * @return a default IUpdateableStorage
         * @see #createDefaultGraph()
         */
        public static IUpdateableStorage createDefaultModel()
        {
            IUpdateableStorage m = ModelFactory.createModelForGraph(createDefaultGraph());
            initNamespaces(m);
            return m;
        }


        /**
         * Creates a memory Graph with no reification.
         * @return a new memory graph
         */
        public static Graph createMemoryGraph()
        {
            return Factory.createDefaultGraph();
        }


        /**
         * Creates a memory IUpdateableStorage with no reification.
         * @return a new memory IUpdateableStorage
         */
        public static IUpdateableStorage createMemoryModel()
        {
            return ModelFactory.createModelForGraph(createMemoryGraph());
        }


        public static MultiUnion createMultiUnion()
        {
            return helper.createMultiUnion();
        }


        public static MultiUnion createMultiUnion(Graph[] graphs)
        {
            return helper.createMultiUnion(graphs);
        }


        public static MultiUnion createMultiUnion(IEnumerator<Graph> graphs)
        {
            return helper.createMultiUnion(graphs);
        }


        /**
         * Gets all instances of a given class and its subclasses.
         * @param cls  the class to get the instances of
         * @return the instances
         */
        public static HashSet<INode> getAllInstances(INode cls)
        {
            JenaUtil.setGraphReadOptimization(true);
            try
            {
                IUpdateableStorage model = cls.getModel();
                HashSet<INode> classes = getAllSubClasses(cls);
                classes.Add(cls);
                HashSet<INode> results = new HashSet<INode>();
                foreach (INode subClass in classes)
                {
                    IEnumerator<Triple> it = model.listStatements(null, RDF.type, subClass);
                    while (it.MoveNext())
                    {
                        results.Add(it.Current.Subject);
                    }
                }
                return results;
            }
            finally
            {
                JenaUtil.setGraphReadOptimization(false);
            }
        }


        public static HashSet<INode> getAllSubClasses(INode cls)
        {
            return getAllTransitiveSubjects(cls, RDFS.subClassOf);
        }


        public static HashSet<INode> getAllSubProperties(INode superProperty)
        {
            return getAllTransitiveSubjects(superProperty, RDFS.subPropertyOf);
        }


        public static HashSet<INode> getAllSuperClasses(INode cls)
        {
            return getAllTransitiveObjects(cls, RDFS.subClassOf);
        }


        public static HashSet<INode> getAllSuperProperties(INode subProperty)
        {
            return getAllTransitiveObjects(subProperty, RDFS.subPropertyOf);
        }


        /**
         * Returns a set of resources reachable from a subject via one or more steps with a given predicate.
         * 
         */
        public static HashSet<INode> getAllTransitiveObjects(INode subject, INode predicate)
        {
            HashSet<INode> set = new HashSet<INode>();
            addTransitiveObjects(set, subject, predicate);
            set.Remove(subject);
            return set;
        }


        private static HashSet<INode> getAllTransitiveSubjects(INode obj, INode predicate)
        {
            return getAllTransitiveSubjects(obj, predicate, null);
        }


        public static HashSet<INode> getAllTypes(INode instance)
        {
            HashSet<INode> types = new HashSet<INode>();
            IEnumerator<Triple> it = instance.listProperties(RDF.type);
            try
            {
                while (it.MoveNext())
                {
                    INode type = it.Current.Object();
                    types.Add(type);
                    types.UnionWith(getAllSuperClasses(type));
                }
            }
            finally
            {
                it.Dispose();
            }
            return types;
        }


        /**
         * Gets the "base graph" of a IUpdateableStorage, walking into MultiUnions if needed.
         * @param model  the IUpdateableStorage to get the base graph of
         * @return the base graph or null if the model contains a MultiUnion that doesn't declare one
         */
        public static Graph getBaseGraph(/*sealed*/ IUpdateableStorage model)
        {
            return getBaseGraph(model.getGraph());
        }


        public static Graph getBaseGraph(Graph graph)
        {
            Graph baseGraph = graph;
            while (baseGraph is MultiUnion)
            {
                baseGraph = ((MultiUnion)baseGraph).getBaseGraph();
            }
            return baseGraph;
        }


        /**
         * Gets the "first" declared rdfs:range of a given property.
         * If multiple ranges exist, the behavior is undefined.
         * Note that this method does not consider ranges defined on
         * super-properties.
         * @param property  the property to get the range of
         * @return the "first" range INode or null
         */
        public static INode getFirstDirectRange(INode property)
        {
            IEnumerator<Triple> it = property.listProperties(RDFS.range);
            while (it.MoveNext())
            {
                INode node = it.Current.Object;
                if (node is INode)
                {
                    it.Dispose();
                    return (INode)node;
                }
            }
            return null;
        }


        private static INode getFirstRange(INode property, HashSet<INode> reached)
        {
            INode directRange = getFirstDirectRange(property);
            if (directRange != null)
            {
                return directRange;
            }
            IEnumerator<Triple> it = property.listProperties(RDFS.subPropertyOf);
            while (it.MoveNext())
            {
                Triple ss = it.Current;
                if (ss.Object is IUriNode)
                {
                    INode superProperty = ss.Object();
                    if (!reached.Contains(superProperty))
                    {
                        reached.Add(superProperty);
                        INode r = getFirstRange(superProperty, reached);
                        if (r != null)
                        {
                            it.Dispose();
                            return r;
                        }
                    }
                }
            }
            return null;
        }


        /**
         * Gets the "first" declared rdfs:range of a given property.
         * If multiple ranges exist, the behavior is undefined.
         * This method walks up to super-properties if no direct match exists.
         * @param property  the property to get the range of
         * @return the "first" range INode or null
         */
        public static INode getFirstRange(INode property)
        {
            return getFirstRange(property, new HashSet<INode>());
        }


        public static int getIntegerProperty(INode subject, INode predicate)
        {
            Triple s = subject.getProperty(predicate);
            if (s != null && s.Object is ILiteralNode)
            {
                return s.getInt();
            }
            else
            {
                return null;
            }
        }


        public static RDFList getListProperty(INode subject, INode predicate)
        {
            Triple s = subject.getProperty(predicate);
            if (s != null && s.Object.canAs(RDFList))
            {
                return s.Object().As(RDFList);
            }
            else
            {
                return null;
            }
        }


        /**
         * Gets the local range of a given property at a given class, considering things like
         * rdfs:range, owl:allValuesFrom restrictions, spl:Argument and others.
         * Returns a suitable default range (rdfs:INode or xsd:string) if no other is defined.
         * @param property  the INode to get the range of
         * @param type  the class to get the range at
         * @param graph  the Graph to operate on
         * @return a suitable range
         */
        public static INode getLocalRange(INode property, INode type, Graph graph)
        {
            return LocalRangeAtClassFunctionHelper.run(type, property, graph);
        }


        /**
         * Overcomes a bug in Jena: if the base model does not declare a default namespace then the
         * default namespace of an import is returned!
         * 
         * @param model the IUpdateableStorage to operate on
         * @param prefix the prefix to get the URI of
         * @return the URI of prefix
         */
        public static String getNsPrefixURI(IUpdateableStorage model, String prefix)
        {
            if ("".Equals(prefix) && model.getGraph() is MultiUnion)
            {
                Graph baseGraph = ((MultiUnion)model.getGraph()).getBaseGraph();
                if (baseGraph != null)
                {
                    return baseGraph.getPrefixMapping().getNsPrefixURI(prefix);
                }
                else
                {
                    return model.getNsPrefixURI(prefix);
                }
            }
            else
            {
                return model.getNsPrefixURI(prefix);
            }
        }


        public static INode getProperty(INode subject, INode predicate)
        {
            Triple s = subject.getProperty(predicate);
            if (s != null)
            {
                return s.Object;
            }
            else
            {
                return null;
            }
        }


        public static List<INode> getReferences(INode predicate, INode obj)
        {
            List<INode> results = new List<INode>();
            IEnumerator<Triple> it = obj.getModel().listStatements(null, predicate, obj);
            while (it.MoveNext())
            {
                Triple s = it.Current;
                results.Add(s.Subject);
            }
            return results;
        }


        public static INode getResourceProperty(INode subject, INode predicate)
        {
            Triple s = subject.getProperty(predicate);
            if (s != null && !(s.Object is ILiteralNode))
            {
                return s.Object();
            }
            else
            {
                return null;
            }
        }


        public static List<INode> getResourceProperties(INode subject, INode predicate)
        {
            List<INode> results = new List<INode>();
            IEnumerator<Triple> it = subject.listProperties(predicate);
            while (it.MoveNext())
            {
                Triple s = it.Current;
                if (!(s.Object is ILiteralNode))
                {
                    results.Add(s.Object());
                }
            }
            return results;
        }


        public static String getStringProperty(INode subject, INode predicate)
        {
            Triple s = subject.getProperty(predicate);
            if (s != null && s.Object is ILiteralNode)
            {
                return s.getString();
            }
            else
            {
                return null;
            }
        }

        public static bool getBooleanProperty(INode subject, INode predicate)
        {
            Triple s = subject.getProperty(predicate);
            if (s != null && s.Object is ILiteralNode)
            {
                return s.getBoolean();
            }
            else
            {
                return false;
            }
        }

        public static List<Graph> getSubGraphs(MultiUnion union)
        {
            List<Graph> results = new List<Graph>();
            results.Add(union.getBaseGraph());
            foreach (Object subGraph in union.getSubGraphs())
            {
                results.Add((Graph)subGraph);
            }
            return results;
        }


        /**
         * Gets a Set of all superclasses (rdfs:subClassOf) of a given INode. 
         * @param subClass  the subClass INode
         * @return a HashSet of class resources
         */
        public static HashSet<INode> getSuperClasses(INode subClass)
        {
            NodeIterator it = subClass.getModel().listObjectsOfProperty(subClass, RDFS.subClassOf);
            HashSet<INode> results = new HashSet<INode>();
            while (it.MoveNext())
            {
                INode node = it.nextNode();
                if (node is INode)
                {
                    results.Add((INode)node);
                }
            }
            return results;
        }


        /**
         * Gets the "first" type of a given INode.
         * @param instance  the instance to get the type of
         * @return the type or null
         */
        public static INode getType(INode instance)
        {
            return getResourceProperty(instance, RDF.type);
        }


        /**
         * Gets a Set of all rdf:types of a given INode. 
         * @param instance  the instance INode
         * @return a HashSet of type resources
         */
        public static HashSet<INode> getTypes(INode instance)
        {
            NodeIterator it = instance.getModel().listObjectsOfProperty(instance, RDF.type);
            HashSet<INode> results = new HashSet<INode>();
            while (it.MoveNext())
            {
                INode node = it.nextNode();
                if (node is INode)
                {
                    results.Add((INode)node);
                }
            }
            return results;
        }


        /**
         * Checks whether a given INode is an instance of a given type, or
         * a subclass thereof.  Make sure that the expectedType parameter is associated
         * with the right IUpdateableStorage, because the system will try to walk up the superclasses
         * of expectedType.  The expectedType may have no IUpdateableStorage, in which case
         * the method will use the instance's IUpdateableStorage.
         * @param instance  the INode to test
         * @param expectedType  the type that instance is expected to have
         * @return true if resource has rdf:type expectedType
         */
        public static bool hasIndirectType(INode instance, INode expectedType)
        {

            if (expectedType.getModel() == null)
            {
                expectedType = expectedType.inModel(instance.getModel());
            }

            IEnumerator<Triple> it = instance.listProperties(RDF.type);
            while (it.MoveNext())
            {
                Triple s = it.Current;
                if (!(s.Object is ILiteralNode))
                {
                    INode actualType = s.Object();
                    if (actualType.Equals(expectedType) || JenaUtil.hasSuperClass(actualType, expectedType))
                    {
                        it.Dispose();
                        return true;
                    }
                }
            }
            return false;
        }


        /**
         * Checks whether a given class has a given (transitive) super class.
         * @param subClass  the sub-class
         * @param superClass  the super-class
         * @return true if subClass has superClass (somewhere up the tree)
         */
        public static bool hasSuperClass(INode subClass, INode superClass)
        {
            return hasSuperClass(subClass, superClass, new HashSet<INode>());
        }


        private static bool hasSuperClass(INode subClass, INode superClass, HashSet<INode> reached)
        {
            foreach (Triple s in subClass.listProperties(RDFS.subClassOf).toList())
            {
                if (superClass.Equals(s.Object))
                {
                    return true;
                }
                else if (!reached.Contains(s.Object()))
                {
                    reached.Add(s.Object());
                    if (hasSuperClass(s.Object(), superClass, reached))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        /**
         * Checks whether a given property has a given (transitive) super property.
         * @param subProperty  the sub-property
         * @param superProperty  the super-property
         * @return true if subProperty has superProperty (somewhere up the tree)
         */
        public static bool hasSuperProperty(INode subProperty, INode superProperty)
        {
            return getAllSuperProperties(subProperty).Contains(superProperty);
        }


        /**
         * Sets the usual default namespaces for rdf, rdfs, owl and xsd.
         * @param graph  the Graph to modify
         */
        public static void initNamespaces(Graph graph)
        {
            PrefixMapping prefixMapping = graph.getPrefixMapping();
            initNamespaces(prefixMapping);
        }


        /**
         * Sets the usual default namespaces for rdf, rdfs, owl and xsd.
         * @param prefixMapping  the IUpdateableStorage to modify
         */
        public static void initNamespaces(PrefixMapping prefixMapping)
        {
            prefixMapping.setNsPrefix("rdf", RDF.Uri);
            prefixMapping.setNsPrefix("rdfs", RDFS.Uri);
            prefixMapping.setNsPrefix("owl", OWL.Uri);
            prefixMapping.setNsPrefix("xsd", XSD.Uri);
        }


        /**
         * Checks whether a given graph (possibly a MultiUnion) only contains
         * GraphMemBase instances.
         * @param graph  the Graph to test
         * @return true  if graph is a memory graph
         */
        public static bool isMemoryGraph(Graph graph)
        {
            if (graph is MultiUnion)
            {
                foreach (Graph subGraph in JenaUtil.getSubGraphs((MultiUnion)graph))
                {
                    if (!isMemoryGraph(subGraph))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return helper.isMemoryGraph(graph);
            }
        }


        /**
         * Checks if a given property is multi-valued according to owl:FunctionalProperty,
         * OWL cardinality restrictions, spl:Argument or spl:ObjectCountPropertyConstraint. 
         * @param property  the INode check
         * @param type  the context class to start traversal at (may be null)
         * @return true  if property may have multiple values
         */
        public static bool isMulti(INode property, INode type)
        {
            return isMulti(property, type != null ? type : null, property.getModel().getGraph());
        }


        public static bool isMulti(INode property, INode type, Graph graph)
        {
            return IsMultiFunctionHelper.isMulti(property, type, graph);
        }


        /**
         * Gets an Iterator over all Statements of a given property or its sub-properties
         * at a given subject instance.  Note that the predicate and subject should be
         * both attached to a IUpdateableStorage to avoid NPEs.
         * @param subject  the subject (may be null)
         * @param predicate  the predicate
         * @return a IEnumerator<Triple>
         */
        public static IEnumerable<Triple> listAllProperties(INode subject, INode predicate)
        {
            List<Triple> results = new List<Triple>();
            helper.setGraphReadOptimization(true);
            try
            {
                listAllProperties(subject, predicate, new HashSet<INode>(), results);
            }
            finally
            {
                helper.setGraphReadOptimization(false);
            }
            return results;
        }


        private static void listAllProperties(INode subject, INode predicate, HashSet<INode> reached,
                List<Triple> results)
        {
            reached.Add(predicate);
            IEnumerator<Triple> sit;
            IUpdateableStorage model;
            if (subject != null)
            {
                sit = subject.listProperties(predicate);
                model = subject.getModel();
            }
            else
            {
                model = predicate.getModel();
                sit = model.listStatements(null, predicate, (INode)null);
            }
            while (sit.MoveNext())
            {
                results.Add(sit.Current);
            }

            // Iterate into direct subproperties
            IEnumerator<Triple> it = model.listStatements(null, RDFS.subPropertyOf, predicate);
            while (it.MoveNext())
            {
                Triple sps = it.Current;
                if (!reached.Contains(sps.Subject))
                {
                    INode subProperty = asProperty(sps.Subject);
                    listAllProperties(subject, subProperty, reached, results);
                }
            }
        }


        public static String withImports(String uri)
        {
            if (!uri.startsWith(WITH_IMPORTS_PREFIX))
            {
                return WITH_IMPORTS_PREFIX + uri;
            }
            else
            {
                return uri;
            }
        }


        public static String withoutImports(String uri)
        {
            if (uri.startsWith(WITH_IMPORTS_PREFIX))
            {
                return uri.substring(WITH_IMPORTS_PREFIX.length());
            }
            else
            {
                return uri;
            }
        }


        /**
         * This indicates that no further changes to the model are needed.
         * Some implementations may give runtime exceptions if this is violated.
         * @param m
         * @return
         */
        public static IUpdateableStorage asReadOnlyModel(IUpdateableStorage m)
        {
            return helper.asReadOnlyModel(m);
        }


        /**
         * This indicates that no further changes to the graph are needed.
         * Some implementations may give runtime exceptions if this is violated.
         * @param m
         * @return
         */
        public static Graph asReadOnlyGraph(Graph g)
        {
            return helper.asReadOnlyGraph(g);
        }


        // Internal to TopBraid only
        public static OntModel createOntologyModel(OntModelSpec spec, IUpdateableStorage baseModel)
        {
            return helper.createOntologyModel(spec, baseModel);
        }


        // Internal to TopBraid only
        public static OntModel createOntologyModel()
        {
            return helper.createOntologyModel();
        }


        // Internal to TopBraid only
        public static OntModel createOntologyModel(OntModelSpec spec)
        {
            return helper.createOntologyModel(spec);
        }


        /**
         * Replacement for {@link INode#getPropertyResourceValue(INode)}
         * which leaves an unclosed iterator.
         * @param subject  the subject
         * @param p  the predicate
         * @return the value or null
         */
        public static INode getPropertyResourceValue(INode subject, INode p)
        {
            IEnumerator<Triple> it = subject.listProperties(p);
            try
            {
                while (it.MoveNext())
                {
                    INode n = it.Current.Object;
                    if (!(n.Object is ILiteralNode))
                    {
                        return (INode)n;
                    }
                }
                return null;
            }
            finally
            {
                it.Dispose();
            }
        }


        /**
         * Allows some environments, e.g. TopBraid, to prioritize
         * a block of code for reading graphs, with no update occurring.
         * The top of the block should call this with <code>true</code>
         * with a matching call with <code>false</code> in a finally
         * block.
         * 
         * Note: Unstable - don't use outside of TopBraid.
         * 
         * @param onOrOff
         */
        public static void setGraphReadOptimization(bool onOrOff)
        {
            getHelper().setGraphReadOptimization(onOrOff);
        }


        /**
         * Ensure that we there is a read-only, thread safe version of the
         * graph.  If the graph is not, then create a deep clone that is
         * both.
         * 
         * Note: Unstable - don't use outside of TopBraid.
         * 
         * @param g The given graph
         * @return A read-only, thread safe version of the given graph.
         */
        public static Graph deepCloneForReadOnlyThreadSafe(Graph g)
        {
            return helper.deepCloneReadOnlyGraph(g);
        }


        /**
         * Calls a SPARQL expression and returns the result, using some initial bindings.
         *
         * @param expression     the expression to execute (must contain absolute URIs)
         * @param initialBinding the initial bindings for the unbound variables
         * @param dataset        the query Dataset or null for default
         * @return the result or null
         */
        public static INode invokeExpression(String expression, SparqlResult initialBinding, Dataset dataset)
        {
            if (dataset == null)
            {
                dataset = ARQFactory.get().getDataset(ModelFactory.createDefaultModel());
            }
            Query query = ARQFactory.get().createExpressionQuery(expression);
            QueryExecution qexec = ARQFactory.get().createQueryExecution(query, dataset, initialBinding);
            SparqlResultSet rs = qexec.execSelect();
            INode result = null;
            if (rs.MoveNext())
            {
                SparqlResult qs = rs.Current;
                String firstVarName = rs.getResultVars().get(0);
                INode rdfNode = qs.get(firstVarName);
                if (rdfNode != null)
                {
                    result = rdfNode;
                }
            }
            qexec.close();
            return result;
        }


        /**
         * Calls a given SPARQL function with one argument.
         *
         * @param function the URI resource of the function to call
         * @param argument the first argument
         * @param dataset  the Dataset to operate on or null for default
         * @return the result of the function call
         */
        public static INode invokeFunction1(INode function, INode argument, Dataset dataset)
        {
            /*sealed*/
            String expression = "<" + function + ">(?arg1)";
            SparqlResultSet initialBinding = new SparqlResultSet();
            initialBinding.Add("arg1", argument);
            return invokeExpression(expression, initialBinding, dataset);
        }


        public static INode toRDFNode(INode node)
        {
            if (node != null)
            {
                return dummyModel.asRDFNode(node);
            }
            else
            {
                return null;
            }
        }


        public static INode invokeFunction1(INode function, INode argument, Dataset dataset)
        {
            return invokeFunction1(function, toRDFNode(argument), dataset);
        }


        /**
         * Calls a given SPARQL function with two arguments.
         *
         * @param function  the URI resource of the function to call
         * @param argument1 the first argument
         * @param argument2 the second argument
         * @param dataset   the Dataset to operate on or null for default
         * @return the result of the function call
         */
        public static INode invokeFunction2(INode function, INode argument1, INode argument2, Dataset dataset)
        {
            /*sealed*/
            String expression = "<" + function + ">(?arg1, ?arg2)";
            SparqlResultSet initialBinding = new SparqlResultSet();
            if (argument1 != null)
            {
                initialBinding.Add("arg1", argument1);
            }
            if (argument2 != null)
            {
                initialBinding.Add("arg2", argument2);
            }
            return invokeExpression(expression, initialBinding, dataset);
        }


        public static INode invokeFunction2(INode function, INode argument1, INode argument2, Dataset dataset)
        {
            return invokeFunction2(function, toRDFNode(argument1), toRDFNode(argument2), dataset);
        }


        public static INode invokeFunction3(INode function, INode argument1, INode argument2, INode argument3, Dataset dataset)
        {

            /*sealed*/
            String expression = "<" + function + ">(?arg1, ?arg2, ?arg3)";
            SparqlResultSet initialBinding = new SparqlResultSet();
            initialBinding.Add("arg1", argument1);
            if (argument2 != null)
            {
                initialBinding.Add("arg2", argument2);
            }
            if (argument3 != null)
            {
                initialBinding.Add("arg3", argument3);
            }
            return invokeExpression(expression, initialBinding, dataset);
        }


        public static INode invokeFunction3(INode function, INode argument1, INode argument2, INode argument3, Dataset dataset)
        {
            return invokeFunction3(function, toRDFNode(argument1), toRDFNode(argument2), toRDFNode(argument3), dataset);
        }
    }
}