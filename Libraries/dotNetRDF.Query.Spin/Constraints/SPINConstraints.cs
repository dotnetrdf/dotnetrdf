/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.Progress;
using VDS.RDF.Query.Spin.Statistics;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.Constraints
{
    /// <summary>
    /// 
    /// </summary>
    public class SPINConstraints
    {
        private static List<ITemplateCall> NO_FIXES = new List<ITemplateCall>();


        internal static void addConstraintViolations(SpinProcessor processor, List<ConstraintViolation> results, SpinWrapperDataset model, INode instance, INode predicate, bool matchValue, List<SPINStatistics> stats, IProgressMonitor monitor)
        {
            List<QueryOrTemplateCall> qots = ((ISPINInstance)Resource.Get(instance, processor).As(typeof(SPINInstanceImpl))).getQueriesAndTemplateCalls(predicate);
            foreach (QueryOrTemplateCall qot in qots)
            {
                if (qot.getTemplateCall() != null)
                {
                    addTemplateCallResults(results, qot, model, instance, matchValue, monitor);
                }
                else if (qot.getQuery() != null)
                {
                    addQueryResults(results, qot, model, instance, matchValue, stats, monitor);
                }
            }
        }


        /** Obsolete since constraints violation are expressed in rdf first
         * Creates an RDF representation (instances of spin:ConstraintViolation) from a
         * collection of ConstraintViolation Java objects. 
         * @param cvs  the violation objects
         * @param result  the Model to add the results to
         * @param createSource  true to also create the spin:violationSource
         */
        public static void addConstraintViolationsRDF(List<ConstraintViolation> cvs, IGraph result, bool createSource)
        {
            foreach (ConstraintViolation cv in cvs)
            {
                INode r = SPIN.ConstraintViolation;
                String message = cv.getMessage();
                if (message != null && message.Length > 0)
                {
                    result.Assert(r, RDFS.label, RDFUtil.CreateLiteralNode(message));
                }
                if (cv.getRoot() != null)
                {
                    result.Assert(r, SPIN.violationRoot, cv.getRoot());
                }
                foreach (SimplePropertyPath path in cv.getPaths())
                {
                    if (path is ObjectPropertyPath)
                    {
                        result.Assert(r, SPIN.violationPath, path.getPredicate());
                    }
                    else
                    {
                        INode p = RDFUtil.nodeFactory.CreateBlankNode(Guid.NewGuid().ToString());
                        result.Assert(p, RDF.type, SP.ReversePath);
                        result.Assert(p, SP.path, path.getPredicate());
                        result.Assert(r, SPIN.violationPath, p);
                    }
                }
                if (createSource && cv.getSource() != null)
                {
                    result.Assert(r, SPIN.violationSource, cv.getSource());
                }
            }
        }


        private static void addConstructedProblemReports(
                IGraph cm,
                List<ConstraintViolation> results,
                SpinWrapperDataset model,
                INode atClass,
                INode matchRoot,
                String label,
                INode source)
        {
            IEnumerator<Triple> it = cm.GetTriplesWithPredicateObject(RDF.type, SPIN.ConstraintViolation).GetEnumerator();
            while (it.MoveNext())
            {
                Triple s = it.Current;
                INode vio = s.Subject;

                INode root = null;
                Triple rootS = cm.GetTriplesWithSubjectPredicate(vio, SPIN.violationRoot).FirstOrDefault();
                if (rootS != null && !(rootS.Object is ILiteralNode))
                {
                    root = rootS.Object;
                }
                if (matchRoot == null || matchRoot.Equals(root))
                {
                    Triple labelS = cm.GetTriplesWithSubjectPredicate(vio, RDFS.label).FirstOrDefault();
                    if (labelS != null && labelS.Object is IValuedNode)
                    {
                        label = ((IValuedNode)labelS.Object).AsString();
                    }
                    else if (label == null)
                    {
                        label = "SPIN constraint at " + SPINLabels.getLabel(atClass);
                    }

                    List<SimplePropertyPath> paths = getViolationPaths(model, vio, root);
                    List<ITemplateCall> fixes = getFixes(cm, model, vio);
                    results.Add(createConstraintViolation(paths, fixes, model, root, label, source));
                }
            }
        }


        private static void addQueryResults(List<ConstraintViolation> results, QueryOrTemplateCall qot, SpinWrapperDataset model, INode resource, bool matchValue, List<SPINStatistics> stats, IProgressMonitor monitor)
        {

            QuerySolutionMap arqBindings = new QuerySolutionMap();

            arqBindings.Add(SPIN.THIS_VAR_NAME, resource);

            IQuery query = qot.getQuery();
            SparqlParameterizedString arq = DatasetUtil.createQuery(query);

            DateTime startTime = DateTime.Now;
            if (query is IAsk)
            {
                if (((SparqlResultSet)model.Query(DatasetUtil.prepare(arq, model, arqBindings).ToString())).Result != matchValue)
                {
                    String message;
                    String comment = qot.getQuery().getComment();
                    if (comment == null)
                    {
                        message = SPINLabels.getLabel(qot.getQuery());
                    }
                    else
                    {
                        message = comment;
                    }
                    message += "\n(SPIN constraint at " + SPINLabels.getLabel(qot.getCls()) + ")";
                    List<SimplePropertyPath> paths = getPropertyPaths(resource, qot.getQuery().getWhere(), null);
                    INode source = getSource(qot);
                    results.Add(createConstraintViolation(paths, NO_FIXES, model, resource, message, source));
                }
            }
            else if (query is IConstruct)
            {
                model.UpdateInternal(DatasetUtil.prepare(DatasetUtil.convertConstructToInsert(arq, query, model._transactionUri), model, arqBindings));
                IGraph cm = new Graph();
                cm.BaseUri = model._transactionUri;
                model.LoadGraph(cm, model._transactionUri);
                addConstructedProblemReports(cm, results, model, qot.getCls(), resource, qot.getQuery().getComment(), getSource(qot));
            }
            DateTime endTime = DateTime.Now;
            if (stats != null)
            {
                TimeSpan duration = startTime - endTime;
                String label = qot.toString();
                String queryText;
                if (qot.getTemplateCall() != null)
                {
                    queryText = SPINLabels.getLabel(qot.getTemplateCall().getTemplate().getBody());
                }
                else
                {
                    queryText = SPINLabels.getLabel(qot.getQuery());
                }
                INode cls = qot.getCls() != null ? qot.getCls() : null;
                stats.Add(new SPINStatistics(label, queryText, duration, startTime, cls));
            }
        }


        private static void addTemplateCallResults(List<ConstraintViolation> results, QueryOrTemplateCall qot,
                SpinWrapperDataset model, INode resource, bool matchValue, IProgressMonitor monitor)
        {
            ITemplateCall templateCall = qot.getTemplateCall();
            ITemplate template = templateCall.getTemplate();
            if (template != null && template.getBody() is IQuery)
            {
                IQuery spinQuery = (IQuery)template.getBody();
                if (spinQuery is IAsk || spinQuery is IConstruct)
                {
                    SparqlParameterizedString arq = DatasetUtil.createQuery(spinQuery);
                    setInitialBindings(resource, templateCall, arq);

                    if (spinQuery is IAsk)
                    {
                        if (((SparqlResultSet)model.Query(DatasetUtil.prepare(arq, model, null).ToString())).Result != matchValue)
                        {
                            List<SimplePropertyPath> paths = getPropertyPaths(resource, spinQuery.getWhere(), templateCall.getArgumentsMapByProperties());
                            String message = SPINLabels.getLabel(templateCall);
                            message += "\n(SPIN constraint at " + SPINLabels.getLabel(qot.getCls()) + ")";
                            results.Add(createConstraintViolation(paths, NO_FIXES, model, resource, message, templateCall));
                        }
                    }
                    else if (spinQuery is IConstruct)
                    {
                        IGraph cm = model.MonitorConstruct(DatasetUtil.prepare(DatasetUtil.convertConstructToInsert(arq, spinQuery, model._transactionUri), model, null), spinQuery, null);
                        INode source = getSource(qot);
                        String label = SPINLabels.getLabel(templateCall);
                        addConstructedProblemReports(cm, results, model, qot.getCls(), resource, label, source);
                    }
                }
            }
        }

        private static SparqlParameterizedString convertAskToConstruct(SparqlParameterizedString ask, IQuery spinQuery, String label)
        {
            if (label == null)
            {
                label = spinQuery.getComment();
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("\nCONSTRUCT {\n");
            sb.Append("\t_:bnode a <" + SPIN.ConstraintViolation.Uri.ToString() + "> . \n");
            sb.Append("\t_:bnode <" + SPIN.violationRoot.Uri.ToString() + "> ?" + SPIN.THIS_VAR_NAME + " . \n");
            if (label != null)
            {
                // TODO handle literal escaping
                sb.Append("\t_:bnode <" + RDFS.label.Uri.ToString() + "> \"\"\"" + RDFUtil.CreateLiteralNode(label).ToString() + "\"\"\" . \n");
            }
            sb.Append("}\n");
            /*
            SparqlQuery construct = SparqlUtil._sparqlParser.ParseFromString(ask);
                //construct.setQueryConstructType();

                List<TriplePattern> list = new List<TriplePattern>();
                INode subject = RDFUtil.nodeFactory.CreateBlankNode();
                list.Add(new TriplePattern(construct., RDF.type, SPIN.ConstraintViolation));
                list.Add(new Triple(subject, SPIN.violationRoot, RDFUtil.CreateLiteralNode(SPIN.THIS_VAR_NAME)));

                Bgp bgp = new Bgp(list);
                construct.ConstructTemplate = new GraphPattern();
            construct.ConstructTemplate.
                //com.hp.hpl.jena.sparql.syntax.Template template = new com.hp.hpl.jena.sparql.syntax.Template(bgp);
                //
                //Element where = construct.getQueryPattern();
                //construct.RootGraphPattern(where);
             * */
            return new SparqlParameterizedString(ask.CommandText.Replace("ASK", sb.ToString()));
        }


        private static ConstraintViolation createConstraintViolation(List<SimplePropertyPath> paths, List<ITemplateCall> fixes, SpinWrapperDataset model, INode instance, String message, INode source)
        {
            return new ConstraintViolation(instance, paths, fixes, message, source);
        }


        private static List<ITemplateCall> getFixes(IGraph cm, SpinProcessor model, INode vio)
        {
            List<ITemplateCall> fixes = new List<ITemplateCall>();
            IEnumerator<Triple> fit = cm.GetTriplesWithSubjectPredicate(vio, SPIN.fix).GetEnumerator();
            while (fit.MoveNext())
            {
                Triple fs = fit.Current;
                if (!(fs.Object is IValuedNode))
                {
                    // ????????????? What is it for ?
                    /*
                    MultiUnion union = JenaUtil.createMultiUnion(new Graph[] {
						model.getGraph(),
						cm.getGraph()
				});
                    Model unionModel = ModelFactory.createModelForGraph(union);
                     */
                    IResource r = Resource.Get(fs.Object, model);
                    ITemplateCall fix = SPINFactory.asTemplateCall(r);
                    fixes.Add(fix);
                }
            }
            return fixes;
        }


        private static List<SimplePropertyPath> getPropertyPaths(INode resource, IElementList where, Dictionary<IResource, IResource> varBindings)
        {
            PropertyPathsGetter getter = new PropertyPathsGetter(where, varBindings);
            getter.run();
            return new List<SimplePropertyPath>(getter.getResults());
        }


        private static INode getSource(QueryOrTemplateCall qot)
        {
            if (qot.getQuery() != null)
            {
                return qot.getQuery();
            }
            else
            {
                return qot.getTemplateCall();
            }
        }


        private static List<SimplePropertyPath> getViolationPaths(SpinWrapperDataset model, INode vio, INode root)
        {
            List<SimplePropertyPath> paths = new List<SimplePropertyPath>();
            IEnumerator<Triple> pit = vio.Graph.GetTriplesWithSubjectPredicate(vio, SPIN.violationPath).GetEnumerator();
            while (pit.MoveNext())
            {
                Triple p = pit.Current;
                if (p.Object is IUriNode)
                {
                    paths.Add(new ObjectPropertyPath(root, p.Object));
                }
                else if (p.Object is IBlankNode)
                {
                    IBlankNode path = (IBlankNode)p.Object;
                    if (model.spinProcessor._spinConfiguration.ContainsTriple(new Triple(path, RDF.type, SP.ReversePath)))
                    {
                        Triple reverse = model.spinProcessor._spinConfiguration.GetTriplesWithSubjectPredicate(path, SP.path).FirstOrDefault();
                        if (reverse != null && reverse.Object is IUriNode)
                        {
                            paths.Add(new SubjectPropertyPath(root, reverse.Object));
                        }
                    }
                }
            }
            return paths;
        }


        /**
         * Checks if a given property is a SPIN constraint property.
         * This is defined as a property that is spin:constraint or a sub-property of it.
         * @param property  the property to check
         * @return true if property is a constraint property
         */
        public static bool isConstraintProperty(INode property)
        {
            return SpinWrapperDataset.currentModel._spinConfiguration.ContainsTriple(new Triple(property, RDFS.subPropertyOf, SPIN.constraint));
        }


        internal static void run(SpinWrapperDataset model, List<ConstraintViolation> results, List<SPINStatistics> stats, IProgressMonitor monitor)
        {

            if (monitor != null)
            {
                monitor.setTaskName("Preparing SPIN Constraints");
            }
            Dictionary<IResource, List<CommandWrapper>> class2Query = SPINQueryFinder.GetClass2QueryMap(model.spinProcessor._spinConfiguration, model, SPIN.constraint, true, true);

            if (monitor != null)
            {
                int totalWork = 0;
                foreach (IResource cls in class2Query.Keys)
                {
                    List<CommandWrapper> arqs = class2Query[cls];
                    totalWork += arqs.Count;
                }
                monitor.beginTask("Checking SPIN Constraints on " + class2Query.Keys.Count + " classes", totalWork);
            }
            foreach (IResource cls in class2Query.Keys)
            {
                List<CommandWrapper> arqs = class2Query[cls];
                foreach (CommandWrapper arqWrapper in arqs)
                {
                    QueryWrapper queryWrapper = (QueryWrapper)arqWrapper;
                    //TODO remove references to the SparqlParameterizedString since it will be rewritten on the fly to adapt to the current model situation
                    SparqlParameterizedString arq = queryWrapper.getQuery();
                    String label = arqWrapper.getLabel();
                    if (queryWrapper.getSPINQuery() is IAsk)
                    {
                        arq = convertAskToConstruct(arq, queryWrapper.getSPINQuery(), label);
                    }
                    runQueryOnClass(results, arq, queryWrapper.getSPINQuery(), label, model, cls, queryWrapper.getTemplateBinding(), arqWrapper.isThisUnbound(), arqWrapper.isThisDeep(), arqWrapper.getSource(), stats, monitor);
                    if (monitor != null)
                    {
                        monitor.worked(1);
                        if (monitor.isCanceled())
                        {
                            return;
                        }
                    }
                }
                if (monitor != null)
                {
                    monitor.worked(1);
                }
            }
        }

        private static Regex bindThisToClass = new Regex("WHERE\\s+\\{", RegexOptions.Compiled | RegexOptions.Singleline);
        private static void runQueryOnClass(List<ConstraintViolation> results, SparqlParameterizedString arq, IQuery spinQuery, String label, SpinWrapperDataset model, IResource cls, Dictionary<String, IResource> initialBindings, bool thisUnbound, bool thisDeep, INode source, List<SPINStatistics> stats, IProgressMonitor monitor)
        {
            SpinProcessor spinModel = cls.getModel();
            // let the underlying engine handle this.
            if (true || thisUnbound || SPINUtil.isRootClass(Resource.Get(cls, spinModel)) /*|| model.contains(null, RDF.type, cls)*/)
            {
                QuerySolutionMap arqBindings = new QuerySolutionMap();
                if (!thisUnbound)
                {
                    arqBindings.Add(SPINUtil.TYPE_CLASS_VAR_NAME, cls);
                }
                if (initialBindings != null)
                {
                    foreach (String varName in initialBindings.Keys)
                    {
                        INode value = initialBindings[varName];
                        arqBindings.Add(varName, value);
                    }
                }

                if (monitor != null)
                {
                    monitor.subTask("Checking SPIN constraint on " + SPINLabels.getLabel(cls) + (label != null ? ": " + label : ""));
                }

                DateTime startTime = DateTime.Now;
                if (thisDeep && !thisUnbound)
                {
                    arq.CommandText = SPINUtil.addThisTypeClause(arq.CommandText);
                    arqBindings[SPINUtil.TYPE_CLASS_VAR_NAME] = cls;
                }
                //TODO replace this with the real query rewriting
                IGraph cm = model.MonitorConstruct(arq, spinQuery, arqBindings);
                model.DeleteGraphInternal(cm.BaseUri.ToString());
                DateTime endTime = DateTime.Now;
                if (stats != null)
                {
                    TimeSpan duration = endTime - startTime;
                    String queryText = SPINLabels.getLabel(spinQuery);
                    if (label == null)
                    {
                        label = queryText;
                    }
                    stats.Add(new SPINStatistics(label, queryText, duration, startTime, cls));
                }
                model.UpdateGraph(model._transactionUri.ToString(), cm.Triples, null);
                addConstructedProblemReports(cm, results, model, cls, null, label, source);
            }
        }

        private static void setInitialBindings(INode resource, ITemplateCall templateCall, SparqlParameterizedString arq)
        {
            QuerySolutionMap arqBindings = new QuerySolutionMap();
            arqBindings.Add(SPIN.THIS_VAR_NAME, resource);
            Dictionary<IArgument, IResource> args = templateCall.getArgumentsMap();
            foreach (IArgument arg in args.Keys)
            {
                INode value = args[arg];
                arq.SetParameter(arg.getVarName(), value);
            }
        }






        ///// <summary>
        ///// Checks all spin:constraints for a given Resource.
        ///// </summary>
        ///// <param name="model">the model containing the resource</param>
        ///// <param name="resource">the instance to run constraint checks on</param>
        ///// <param name="monitor">an (optional) progress monitor (currently ignored)</param>
        ///// <returns>a List of ConstraintViolations (empty if all is OK)</returns>
        //public static List<ConstraintViolation> check(SpinWrapperDataset model, INode resource, IProgressMonitor monitor)
        //{
        //    return check(model, resource, new List<SPINStatistics>(), monitor);
        //}

        ///// <summary>
        ///// Checks all spin:constraints for a given Resource.
        ///// </summary>
        ///// <param name="model">the model containing the resource</param>
        ///// <param name="resource">the instance to run constraint checks on</param>
        ///// <param name="stats">an (optional) List to add statistics to</param>
        ///// <param name="monitor">an (optional) progress monitor (currently ignored)</param>
        ///// <returns>a List of ConstraintViolations (empty if all is OK)</returns>
        //public static List<ConstraintViolation> check(SpinWrapperDataset model, INode resource, List<SPINStatistics> stats, IProgressMonitor monitor)
        //{
        //    List<ConstraintViolation> results = new List<ConstraintViolation>();
        //    addConstraintViolations( results, model, resource, SPIN.constraint, false, stats, monitor);
        //    return results;
        //}

        ///// <summary>
        ///// Checks all instances in a given Model against all spin:constraints and returns a List of constraint violations. 
        ///// A IProgressMonitor can be provided to enable the user to get intermediate status reports and to cancel the operation.
        ///// </summary>
        ///// <param name="model">the model to run constraint checks on</param>
        ///// <param name="monitor">an (optional) progress monitor (currently ignored)</param>
        ///// <returns>a List of ConstraintViolations (empty if all is OK)</returns>
        //public static List<ConstraintViolation> check(SpinWrapperDataset model, IProgressMonitor monitor)
        //{
        //    return check(model, (List<SPINStatistics>)null, monitor);
        //}


        ///// <summary>
        ///// Checks all instances in a given Model against all spin:constraints and returns a List of constraint violations. 
        ///// A IProgressMonitor can be provided to enable the user to get intermediate status reports and to cancel the operation.
        ///// </summary>
        ///// <param name="model">the model to run constraint checks on</param>
        ///// <param name="stats">an (optional) List to add statistics to</param>
        ///// <param name="monitor">an (optional) progress monitor (currently ignored)</param>
        ///// <returns>a List of ConstraintViolations (empty if all is OK)</returns>
        //public static List<ConstraintViolation> check(SpinWrapperDataset model, List<SPINStatistics> stats, IProgressMonitor monitor)
        //{
        //    List<ConstraintViolation> results = new List<ConstraintViolation>();
        //    run(model, results, stats, monitor);
        //    return results;
        //}

    }
}