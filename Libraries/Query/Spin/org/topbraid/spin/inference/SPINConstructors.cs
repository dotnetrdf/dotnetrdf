/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Storage;
using System.Collections.Generic;
using VDS.RDF;
using org.topbraid.spin.util;

using System;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Spin;
using org.topbraid.spin.statistics;
using VDS.RDF.Query.Spin.org.topbraid.spin.util;
using org.topbraid.spin.vocabulary;
using VDS.RDF.Query.Spin.Util;
using org.topbraid.spin.model;
using org.topbraid.spin.system;
using org.topbraid.spin.progress;
using org.topbraid.spin.arq;

namespace org.topbraid.spin.inference
{
    /**
     * Static methods to find and execute spin:constructors for a given
     * set of Resources.
     * 
     * @author Holger Knublauch
     */
    public class SPINConstructors
    {

        /**
         * Runs the constructors on a List of Resources.
         * @param queryModel  the model to query over
         * @param instances  the instances to run the constructors of
         * @param targetModel  the model that shall receive the new triples
         * @param monitor  an optional progress monitor
         */
        public static void construct(Model queryModel, List<IResource> instances, IGraph targetModel, IProgressMonitor monitor)
        {
            Dictionary<IResource, List<CommandWrapper>> class2Constructor = SPINQueryFinder.GetClass2QueryMap(queryModel, queryModel, SPIN.constructor, true, false);
            construct(queryModel, instances, targetModel, new HashSet<IResource>(), class2Constructor, monitor);
        }


        /**
         * Runs the constructors on a List of Resources.
         * @param queryModel  the model to query over
         * @param instances  the instances to run the constructors of
         * @param targetModel  the model that shall receive the new triples
         * @param reached  the Set of already reached Resources
         * @param monitor  an optional progress monitor
         */
        public static void construct(
                Model queryModel,
                List<IResource> instances,
                IGraph targetModel,
                HashSet<IResource> reached,
                Dictionary<IResource, List<CommandWrapper>> class2Constructor,
                IProgressMonitor monitor)
        {
            construct(queryModel, instances, targetModel, reached, class2Constructor, null, null, monitor);
        }


        /**
         * Runs the constructors on a List of Resources.
         * @param queryModel  the model to query over
         * @param instances  the instances to run the constructors of
         * @param targetModel  the model that shall receive the new triples
         * @param reached  the Set of already reached Resources 
         * @param explanations  an (optional) explanations object
         * @param monitor  an optional progress monitor
         */
        public static void construct(
                Model queryModel,
                List<IResource> instances,
                IGraph targetModel,
                HashSet<IResource> reached,
                Dictionary<IResource, List<CommandWrapper>> class2Constructor,
                List<SPINStatistics> statistics,
                SPINExplanations explanations,
                IProgressMonitor monitor)
        {
            if (instances.Count != 0)
            {
                List<IResource> newResources = new List<IResource>();
                foreach (IResource instance in instances)
                {
                    if (!reached.Contains(instance))
                    {
                        reached.Add(instance);
                        constructInstance(queryModel, instance, targetModel, newResources, class2Constructor, statistics, explanations, monitor);
                    }
                }
                construct(queryModel, newResources, targetModel, reached, class2Constructor, statistics, explanations, monitor);
            }
        }


        /**
         * Runs constructors for a single instance.
         * @param queryModel  the model to query
         * @param instance  the instance to run the constructors of
         * @param targetModel  the model that will receive the new triples
         * @param newResources  will hold the newly constructed instances
         * @param monitor  an optional progress monitor
         */
        public static void constructInstance(
                Model queryModel,
                IResource instance,
                IGraph targetModel,
                List<IResource> newResources,
                Dictionary<IResource, List<CommandWrapper>> class2Constructor,
                List<SPINStatistics> statistics,
                SPINExplanations explanations,
                IProgressMonitor monitor)
        {
            foreach (Triple s in instance.listProperties(RDF.type).ToList())
            {
                IResource type = Resource.Get(s.Object, instance.getModel());
                constructInstance(queryModel, instance, type, targetModel, newResources, new HashSet<IResource>(), class2Constructor, statistics, explanations, monitor);
            }
        }


        /**
         * Runs all constructors defined for a given type on a given instance.
         * @param queryModel  the model to query
         * @param instance  the instance to run the constructors of
         * @param type  the class to run the constructors of
         * @param targetModel  the model that will receive the new triples
         * @param newResources  will hold the newly constructed instances
         * @param reachedTypes  contains the already reached types
         * @param monitor  an optional progress monitor
         */
        public static void constructInstance(Model queryModel, INode instance, INode type, IGraph targetModel, List<INode> newResources, HashSet<INode> reachedTypes, IProgressMonitor monitor)
        {
            constructInstance(queryModel, instance, type, targetModel, newResources, reachedTypes, monitor);
        }


        /**
         * Runs all constructors defined for a given type on a given instance.
         * @param queryModel  the model to query
         * @param instance  the instance to run the constructors of
         * @param type  the class to run the constructors of
         * @param targetModel  the model that will receive the new triples
         * @param newResources  will hold the newly constructed instances
         * @param reachedTypes  contains the already reached types
         * @param explanations  the explanations (optional)
         * @param monitor  an optional progress monitor
         */
        public static void constructInstance(
                Model queryModel,
                IResource instance,
                IResource type,
                IGraph targetModel,
                List<IResource> newResources,
                HashSet<IResource> reachedTypes,
                Dictionary<IResource, List<CommandWrapper>> class2Constructor,
                List<SPINStatistics> statistics,
                SPINExplanations explanations,
                IProgressMonitor monitor)
        {

            // Run superclass constructors first
            foreach (Triple s in type.listProperties(RDFS.subClassOf).ToList())
            {
                IResource superClass = Resource.Get(s.Object, type.getModel());
                if (!reachedTypes.Contains(superClass))
                {
                    reachedTypes.Add(superClass);
                    constructInstance(
                            queryModel,
                            instance,
                            superClass,
                            targetModel,
                            newResources,
                            reachedTypes,
                            class2Constructor,
                            statistics,
                            explanations,
                            monitor);
                }
            }

            if (class2Constructor.ContainsKey(type))
            {
                List<CommandWrapper> commandWrappers = class2Constructor[type];
                if (commandWrappers != null)
                {

                    foreach (CommandWrapper commandWrapper in commandWrappers)
                    {

                        QuerySolutionMap bindings = new QuerySolutionMap();
                        if (instance != null)
                        {
                            bindings.Add(SPIN.THIS_VAR_NAME, instance);
                        }
                        Dictionary<String, INode> initialBindings = commandWrapper.getTemplateBinding();
                        if (initialBindings != null)
                        {
                            foreach (String varName in initialBindings.Keys)
                            {
                                INode value = initialBindings[varName];
                                bindings.Add(varName, value);
                            }
                        }

                        if (monitor != null)
                        {
                            monitor.subTask("TopSPIN constructor at " + SPINLabels.getLabel(instance) + ": " + commandWrapper.getText());
                        }

                        DateTime startTime = DateTime.Now;

                        if (commandWrapper is QueryWrapper)
                        {

                            /*sealed*/
                            List<Triple> triples = new List<Triple>();
                            AbstractGraphListener listener = new AbstractGraphListener();// {

                            //    override 					public void notifyAddTriple(Graph g, Triple t) {
                            //        triples.Add(t);
                            //    }

                            //    override 					public void notifyDeleteTriple(Graph g, Triple t) {
                            //    }

                            //    override 					protected void notifyRemoveAll(Graph source, Triple pattern) {
                            //    }
                            //};

                            QueryWrapper queryWrapper = (QueryWrapper)commandWrapper;
                            Query arqQuery = queryWrapper.getQuery();
                            if (arqQuery.isConstructType())
                            {

                                QueryExecution qexec = ARQFactory.get().createQueryExecution(arqQuery, queryModel);
                                qexec.setInitialBinding(bindings);

                                // Execute construct and remember the order in which triples were inserted
                                // Note that this does not work yet since Jena appears to have random order
                                Model resultModel = ModelFactory.createDefaultModel();
                                resultModel.getGraph().getEventManager().register(listener);
                                qexec.execConstruct(resultModel);
                                qexec.close();

                                StringBuilder sb = new StringBuilder();
                                sb.Append("Inferred by SPIN constructor at class ");
                                sb.Append(SPINLabels.getLabel(type));
                                sb.Append(":\n\n" + commandWrapper.getText());
                                String explanationText = sb.ToString();

                                // Add all new triples and any new resources
                                foreach (Triple triple in triples)
                                {
                                    Triple rs = queryModel.asStatement(triple);
                                    if (!targetModel.Contains(rs))
                                    {
                                        targetModel.Add(rs);
                                        if (RDFUtil.sameTerm(RDF.type, rs.Predicate))
                                        {
                                            INode subject = rs.Subject;
                                            if (!newResources.Contains(subject))
                                            {
                                                newResources.Add(subject);
                                            }
                                        }
                                        if (explanations != null)
                                        {
                                            INode source = commandWrapper.getStatement().Subject;
                                            explanations.put(triple, explanationText, source);
                                        }
                                    }
                                }
                            }
                        }
                        else if (commandWrapper is UpdateWrapper)
                        {
                            Update update = ((UpdateWrapper)commandWrapper).getUpdate();
                            Dataset dataset = ARQFactory.get().getDataset(queryModel);
                            HashSet<Graph> updateGraphs = UpdateUtil.getUpdatedGraphs(update, dataset, initialBindings);
                            ControlledUpdateGraphStore cugs = new ControlledUpdateGraphStore(dataset, updateGraphs);
                            UpdateProcessor up = UpdateExecutionFactory.create(update, cugs, JenaUtil.asBinding(bindings));
                            up.execute();
                            foreach (ControlledUpdateGraph cug in cugs.getControlledUpdateGraphs())
                            {
                                foreach (Triple triple in cug.getAddedTriples())
                                {
                                    Triple rs = queryModel.asStatement(triple);
                                    if (RDFUtil.sameTerm(RDF.type, rs.Predicate))
                                    {
                                        INode subject = rs.Subject;
                                        if (!newResources.Contains(subject))
                                        {
                                            newResources.Add(subject);
                                        }
                                    }
                                }
                            }
                        }

                        DateTime endTime = DateTime.Now;
                        if (statistics != null)
                        {
                            String queryText = SPINLabels.getLabel(commandWrapper.getSPINCommand());
                            String label = commandWrapper.getLabel();
                            if (label == null)
                            {
                                label = queryText;
                            }
                            statistics.Add(new SPINStatistics(label, queryText, endTime - startTime, startTime, instance));
                        }
                    }
                }
            }
        }


        /**
         * Runs all constructors on all instances in a given model.
         * @param queryModel  the query model
         * @param targetModel  the model to write the new triples to
         * @param monitor  an optional progress monitor
         */
        public static void constructAll(Model queryModel, IGraph targetModel, IProgressMonitor monitor)
        {
            HashSet<IResource> classes = getClassesWithConstructor(queryModel);
            List<IResource> instances = new List<IResource>(getInstances(classes));
            OntModel ontModel = JenaUtil.createOntologyModel(OntModelSpec.OWL_MEM, queryModel);
            if (targetModel != queryModel)
            {
                ontModel.addSubModel(targetModel);
            }
            Dictionary<IResource, List<CommandWrapper>> class2Constructor = SPINQueryFinder.GetClass2QueryMap(queryModel, queryModel, SPIN.constructor, true, false);
            construct(ontModel, instances, targetModel, new HashSet<IResource>(), class2Constructor, monitor);
        }


        /**
         * Finds all classes that directly have a spin:constructor attached
         * to it.
         * @param model  the Model to operate on
         * @return a Set of classes
         */
        public static HashSet<IResource> getClassesWithConstructor(Model model)
        {
            HashSet<IResource> results = new HashSet<IResource>();
            foreach (IResource property in getConstructorProperties(model))
            {
                IEnumerator<Triple> it = model._spinConfiguration.GetTriplesWithPredicate(property.getSource()).GetEnumerator();
                while (it.MoveNext())
                {
                    results.Add(Resource.Get(it.Current.Subject, model));
                }
            }
            return results;
        }


        private static IEnumerable<IResource> getConstructorProperties(Model model)
        {
            List<IResource> results = new List<IResource>();
            foreach (IResource r in model.GetAllSubProperties(SPIN.constructor))
            {
                results.Add(r);
            }
            results.Add(Resource.Get(SPIN.constructor, model));
            return results;
        }


        private static HashSet<IResource> getInstances(HashSet<IResource> classes)
        {
            HashSet<IResource> results = new HashSet<IResource>();
            foreach (IResource cls in classes)
            {
                results.addAll(JenaUtil.getAllInstances(cls));
            }
            return results;
        }


        /**
         * Checks whether a given class or a superclass thereof has a
         * constructor.
         * @param cls  the class to check
         * @return true if cls has a constructor
         */
        public static bool hasConstructor(IResource cls)
        {
            foreach (IResource property in getConstructorProperties(cls.getModel()))
            {
                if (cls.hasProperty(property))
                {
                    return true;
                }
                else
                {
                    foreach (IResource superClass in cls.getModel().GetAllSuperClasses(cls))
                    {
                        if (superClass.hasProperty(property))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}