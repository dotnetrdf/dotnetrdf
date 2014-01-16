/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using org.topbraid.spin.arq;
using org.topbraid.spin.model;
using org.topbraid.spin.model.update;
using org.topbraid.spin.progress;
using org.topbraid.spin.statistics;
using org.topbraid.spin.system;
using org.topbraid.spin.util;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.org.topbraid.spin.util;
using VDS.RDF.Query.Spin.Util;

namespace org.topbraid.spin.inference
{

    /**
     * A service to execute inference rules based on the spin:rule property.
     * 
     * @author Holger Knublauch
     */
    public class SPINInferences
    {

        /**
         * The globally registered optimizers
         */
        private static List<ISPINInferencesOptimizer> optimizers = new List<ISPINInferencesOptimizer>();

        public static void addOptimizer(ISPINInferencesOptimizer optimizer)
        {
            optimizers.Add(optimizer);
        }

        public static void removeOptimizer(ISPINInferencesOptimizer optimizer)
        {
            optimizers.Remove(optimizer);
        }


        /**
         * Checks if a given property is a SPIN rule property.
         * This is (currently) defined as a property that has type spin:RuleProperty
         * or is a sub-property of spin:rule.  The latter condition may be removed
         * at some later stage after people have upgraded to SPIN 1.1 conventions.
         * @param property  the property to check
         * @return true if property is a rule property
         */
        public static bool isRuleProperty(IResource property)
        {
            if (RDFUtil.sameTerm(SPIN.rule, property))
            {
                return true;
            }
            else if (property.hasProperty(RDFS.subPropertyOf, SPIN.rule))
            {
                return true;
            }
            else
            {
                return property.hasProperty(RDFS.subClassOf, SPIN.RuleProperty);
            }
        }


        /**
         * See the other run method for help - this is using spin:rule as rulePredicate.
         * @param queryModel  the Model to query
         * @param newTriples  the Model to add the new triples to 
         * @param explanations  an optional object to write explanations to
         * @param statistics  optional list to add statistics about which queries were slow
         * @param singlePass  true to just do a single pass (don't iterate)
         * @param monitor  an optional IProgressMonitor
         * @return the number of iterations (1 with singlePass)
         * @see #run(Model, INode, Model, SPINExplanations, List, boolean, IProgressMonitor)
         */
        public static int run(
                Model queryModel,
                IGraph newTriples,
                SPINExplanations explanations,
                List<SPINStatistics> statistics,
                bool singlePass,
                IProgressMonitor monitor)
        {
            return run(queryModel, Resource.Get(SPIN.rule), newTriples, explanations, statistics, singlePass, monitor);
        }


        /**
         * Iterates over all SPIN rules in a (query) Model and adds all constructed
         * triples to a given Model (newTriples) until no further changes have been
         * made within one iteration.
         * Note that in order to iterate more than single pass, the newTriples Model
         * must be a sub-model of the queryModel (which likely has to be an OntModel).
         * The supplied rulePredicate is usually spin:rule, but can also be a sub-
         * property of spin:rule to exercise finer control over which rules to fire.
         * @param queryModel  the Model to query
         * @param rulePredicate  the rule predicate (spin:rule or a sub-property thereof)
         * @param newTriples  the Model to add the new triples to 
         * @param explanations  an optional object to write explanations to
         * @param statistics  optional list to add statistics about which queries were slow
         * @param singlePass  true to just do a single pass (don't iterate)
         * @param monitor  an optional IProgressMonitor
         * @return the number of iterations (1 with singlePass)
         */
        public static int run(
                Model queryModel,
                IResource rulePredicate,
                IGraph newTriples,
                SPINExplanations explanations,
                List<SPINStatistics> statistics,
                bool singlePass,
                IProgressMonitor monitor)
        {
            Dictionary<IResource, List<CommandWrapper>> cls2Query = SPINQueryFinder.GetClass2QueryMap(queryModel, queryModel, rulePredicate, true, false);
            Dictionary<IResource, List<CommandWrapper>> cls2Constructor = SPINQueryFinder.GetClass2QueryMap(queryModel, queryModel, SPIN.constructor, true, false);
            ISPINRuleComparer comparator = new DefaultSPINRuleComparer(queryModel);
            return run(queryModel, newTriples, cls2Query, cls2Constructor, explanations, statistics, singlePass, rulePredicate, comparator, monitor);
        }


        /**
         * Iterates over a provided collection of SPIN rules and adds all constructed
         * triples to a given Model (newTriples) until no further changes have been
         * made within one iteration.
         * Note that in order to iterate more than single pass, the newTriples Model
         * must be a sub-model of the queryModel (which likely has to be an OntModel).
         * @param queryModel  the Model to query
         * @param newTriples  the Model to add the new triples to 
         * @param class2Query  the map of queries to run (see SPINQueryFinder)
         * @param class2Constructor  the map of constructors to run
         * @param explanations  an optional object to write explanations to
         * @param statistics  optional list to add statistics about which queries were slow
         * @param singlePass  true to just do a single pass (don't iterate)
         * @param rulePredicate  the predicate used (e.g. spin:rule)
         * @param comparator  optional comparator to determine the order of rule execution
         * @param monitor  an optional IProgressMonitor
         * @return the number of iterations (1 with singlePass)
         */
        public static int run(
                Model queryModel,
                IGraph newTriples,
                Dictionary<IResource, List<CommandWrapper>> class2Query,
                Dictionary<IResource, List<CommandWrapper>> class2Constructor,
                SPINExplanations explanations,
                List<SPINStatistics> statistics,
                bool singlePass,
                IResource rulePredicate,
                ISPINRuleComparer comparator,
                IProgressMonitor monitor)
        {

            // Run optimizers (if available)
            foreach (ISPINInferencesOptimizer optimizer in optimizers)
            {
                class2Query = optimizer.optimize(class2Query);
            }

            // Get sorted list of Rules and remember where they came from
            List<CommandWrapper> rulesList = new List<CommandWrapper>();
            Dictionary<CommandWrapper, IResource> rule2Class = new Dictionary<CommandWrapper, IResource>();
            foreach (IResource cls in class2Query.Keys)
            {
                List<CommandWrapper> queryWrappers = class2Query[cls];
                foreach (CommandWrapper queryWrapper in queryWrappers)
                {
                    rulesList.Add(queryWrapper);
                    rule2Class[queryWrapper] = cls;
                }
            }
            if (comparator != null)
            {
                rulesList.Sort(comparator);
            }

            // Make sure the rulePredicate has a Model attached to it
            if (rulePredicate.getModel() == null)
            {
                rulePredicate = Resource.Get(RDFUtil.CreateUriNode(rulePredicate.Uri()), queryModel);
            }

            // Iterate
            int iteration = 1;
            bool changed;
            do
            {
                HashSet<Triple> newRules = new HashSet<Triple>();
                changed = false;
                foreach (CommandWrapper arqWrapper in rulesList)
                {

                    // Skip rule if needed
                    if (arqWrapper.getStatement() != null)
                    {
                        IResource predicate = Resource.Get(arqWrapper.getStatement().Predicate);
                        int? maxIterationCount = predicate.getInteger(SPIN.rulePropertyMaxIterationCount);
                        if (maxIterationCount != null)
                        {
                            if (iteration > maxIterationCount)
                            {
                                continue;
                            }
                        }
                    }

                    IResource cls = rule2Class[arqWrapper];

                    if (monitor != null)
                    {

                        if (monitor.isCanceled())
                        {
                            return iteration - 1;
                        }

                        StringBuilder sb = new StringBuilder("TopSPIN iteration ");
                        sb.Append(iteration);
                        sb.Append(" at ");
                        sb.Append(SPINLabels.getLabel(cls));
                        sb.Append(", rule ");
                        sb.Append(arqWrapper.getLabel() != null ? arqWrapper.getLabel() : arqWrapper.getText());
                        monitor.subTask(sb.ToString());
                    }

                    StringBuilder explanationSb = new StringBuilder();
                    explanationSb.Append("Inferred by ");
                    explanationSb.Append(SPINLabels.getLabel(rulePredicate));
                    explanationSb.Append(" at class ");
                    explanationSb.Append(SPINLabels.getLabel(cls));
                    explanationSb.Append(":\n\n" + arqWrapper.getText());
                    String explanationText = explanationSb.ToString();
                    bool thisUnbound = arqWrapper.isThisUnbound();
                    changed |= runCommandOnClass(arqWrapper, arqWrapper.getLabel(), queryModel, newTriples, cls, true, class2Constructor, statistics, explanations, explanationText, newRules, thisUnbound, monitor);
                    if (!SPINUtil.isRootClass(cls) && !thisUnbound)
                    {
                        IEnumerable<IResource> subClasses = queryModel.GetAllSubClasses(cls);
                        foreach (IResource subClass in subClasses)
                        {
                            changed |= runCommandOnClass(arqWrapper, arqWrapper.getLabel(), queryModel, newTriples, subClass, true, class2Constructor, statistics, explanations, explanationText, newRules, thisUnbound, monitor);
                        }
                    }
                }
                iteration++;

                if (newRules.Count>0 && !singlePass)
                {
                    foreach (Triple s in newRules)
                    {
                        SPINQueryFinder.Add(class2Query, s, queryModel, true, false);
                    }
                }
            }
            while (!singlePass && changed);

            return iteration - 1;
        }

        // TODO relocate processing into the underlying engine and capture new rdf:type triples.
        private static bool runCommandOnClass(
                CommandWrapper commandWrapper,
                String queryLabel,
            /*sealed*/ Model queryModel,
                IGraph newTriples,
                IResource cls,
                bool checkContains,
                Dictionary<IResource, List<CommandWrapper>> class2Constructor,
                List<SPINStatistics> statistics,
                SPINExplanations explanations,
                String explanationText,
                HashSet<Triple> newRules,
                bool thisUnbound,
                IProgressMonitor monitor)
        {

            // Check if query is needed at all => We let the underlying engine do that
            if (true || thisUnbound || SPINUtil.isRootClass(cls) /*|| queryModel.GetTriplesWithPredicateObject(null, RDF.type, cls).Any()*/)
            {
                bool changed = false;
                Dictionary<String, INode> bindings = new Dictionary<String, INode>();
                bool needsClass = !SPINUtil.isRootClass(cls) && !thisUnbound;
                Dictionary<String, IResource> initialBindings = commandWrapper.getTemplateBinding();
                if (initialBindings != null)
                {
                    foreach (String varName in initialBindings.Keys)
                    {
                        INode value = initialBindings[varName];
                        bindings.Add(varName, value);
                    }
                }
                DateTime startTime = DateTime.Now;
                /*sealed*/
                Dictionary<IResource, INode> newInstances = new Dictionary<IResource, INode>();
                if (commandWrapper is QueryWrapper)
                {
                    SparqlParameterizedString arq = ((QueryWrapper)commandWrapper).getQuery();
                    IGraph cm = new Graph();
                    if (commandWrapper.isThisDeep() && needsClass)
                    {

                        // If there is no simple way to bind ?this inside of the query then
                        // do the iteration over all instances in an "outer" loop
                        cm = new Graph();
                        IEnumerator<Triple> it = queryModel.GetTriplesWithPredicateObject(RDF.type, cls);
                        while (it.MoveNext())
                        {
                            INode instance = it.Current.Subject;
                            QueryExecution qexec = ARQFactory.get().createQueryExecution(arq, queryModel);
                            bindings.Add(SPIN.THIS_VAR_NAME, instance);
                            qexec.setInitialBinding(bindings);
                            qexec.execConstruct(cm);
                            qexec.close();
                        }
                    }
                    else
                    {
                        if (needsClass)
                        {
                            bindings.Add(SPINUtil.TYPE_CLASS_VAR_NAME, cls);
                        }
                        QueryExecution qexec = ARQFactory.get().createQueryExecution(arq, queryModel, bindings);
                        cm = qexec.execConstruct();
                        qexec.close();
                    }
                    IEnumerator<Triple> cit = cm.Triples.GetEnumerator();
                    while (cit.MoveNext())
                    {
                        Triple s = cit.Current;
                        if (!checkContains || !queryModel.ContainsTriple(s))
                        {
                            changed = true;
                            newTriples.Assert(s);
                            if (explanations != null && commandWrapper.getStatement() != null)
                            {
                                INode source = commandWrapper.getStatement().Subject;
                                explanations.put(s, explanationText, source);
                            }

                            // New rdf:type triple -> run constructors later
                            if (RDFUtil.sameTerm(RDF.type, s.Predicate) && !(s.Object is ILiteralNode))
                            {
                                IResource subject = Resource.Get(s.Subject, queryModel);
                                newInstances[subject] = s.Object;
                            }

                            if (RDFUtil.sameTerm(SPIN.rule, s.Predicate))
                            {
                                newRules.Add(s);
                            }
                        }
                    }
                }
                else
                {
                    UpdateWrapper updateWrapper = (UpdateWrapper)commandWrapper;
                    Dictionary<String, INode> templateBindings = commandWrapper.getTemplateBinding();
                    Dataset dataset = ARQFactory.get().getDataset(queryModel);
                    IUpdate update = updateWrapper.getUpdate();
                    IEnumerable<Graph> updateGraphs = UpdateUtil.getUpdatedGraphs(update, dataset, templateBindings);
                    ControlledUpdateGraphStore cugs = new ControlledUpdateGraphStore(dataset, updateGraphs);

                    if (commandWrapper.isThisDeep() && needsClass)
                    {
                        foreach (Triple s in queryModel.GetTriplesWithPredicateObject(RDF.type, cls).ToList())
                        {
                            INode instance = s.Subject;
                            bindings.Add(SPIN.THIS_VAR_NAME, instance);
                            UpdateProcessor up = UpdateExecutionFactory.create(update, cugs, JenaUtil.asBinding(bindings));
                            up.execute();
                        }
                    }
                    else
                    {
                        if (needsClass)
                        {
                            bindings.Add(SPINUtil.TYPE_CLASS_VAR_NAME, cls);
                        }
                        UpdateProcessor up = UpdateExecutionFactory.create(update, cugs, JenaUtil.asBinding(bindings));
                        up.execute();
                    }

                    foreach (ControlledUpdateGraph cug in cugs.getControlledUpdateGraphs())
                    {
                        changed |= cug.isChanged();
                        foreach (Triple triple in cug.getAddedTriples())
                        {
                            if (RDFUtil.sameTerm(RDF.type, triple.Predicate) && !triple.Object is ILiteralNode)
                            {
                                INode subject = (INode)queryModel.asRDFNode(triple.Subject);
                                newInstances[subject] = (INode)queryModel.asRDFNode(triple.Object);
                            }
                        }
                    }
                }

                if (statistics != null)
                {
                    DateTime endTime = DateTime.Now;
                    TimeSpan duration = (endTime - startTime);
                    String queryText = SPINLabels.getLabel(commandWrapper.getSPINCommand());
                    if (queryLabel == null)
                    {
                        queryLabel = queryText;
                    }
                    statistics.Add(new SPINStatistics(queryLabel, queryText, duration, startTime, cls));
                }

                if (newInstances.Count>0)
                {
                    List<IResource> newRs = new List<IResource>(newInstances.Keys);
                    SPINConstructors.construct(
                            queryModel,
                            newRs,
                            newTriples,
                            new HashSet<IResource>(),
                            class2Constructor,
                            statistics,
                            explanations,
                            monitor);
                }

                return changed;
            }
            else
            {
                return false;
            }
        }


        /**
         * Runs a given Jena Query on a given instance and adds the inferred triples
         * to a given Model.
         * @param queryWrapper  the wrapper of the CONSTRUCT query to execute
         * @param queryModel  the query Model
         * @param newTriples  the Model to write the triples to
         * @param instance  the instance to run the inferences on
         * @param checkContains  true to only call add if a Triple wasn't there yet
         * @return true if changes were done (only meaningful if checkContains == true)
         */
        public static bool runQueryOnInstance(QueryWrapper queryWrapper, Model queryModel, IGraph newTriples, INode instance, bool checkContains)
        {
            bool changed = false;
            QueryExecution qexec = ARQFactory.get().createQueryExecution(queryWrapper.getQuery(), queryModel);
            Dictionary<String, INode> bindings = new Dictionary<String, INode>();
            bindings.Add(SPIN.THIS_VAR_NAME, instance);
            Dictionary<String, IResource> initialBindings = queryWrapper.getTemplateBinding();
            if (initialBindings != null)
            {
                foreach (String varName in initialBindings.Keys)
                {
                    INode value = initialBindings[varName];
                    bindings.Add(varName, value);
                }
            }
            qexec.setInitialBinding(bindings);
            IGraph cm = qexec.execConstruct();
            IEnumerator<Triple> cit = cm.Triples.GetEnumerator();
            while (cit.MoveNext())
            {
                Triple s = cit.Current;
                if (!checkContains || !queryModel.ContainsTriple(s))
                {
                    changed = true;
                    newTriples.Assert(s);
                }
            }
            return changed;
        }
    }
}