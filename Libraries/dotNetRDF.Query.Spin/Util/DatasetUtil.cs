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
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Storage;
using VDS.RDF.Query.Spin.Model;

namespace VDS.RDF.Query.Spin.Util
{

#if UNFINISHED
    public static class DatasetExtensions
    {

    #region ISparqlDataset extensions

        /// <summary>
        /// Executes a SPARQL Query on the Dataset
        /// </summary>
        /// <param name="sparqlQuery"></param>
        /// <returns></returns>
        public static object ExecuteQuery(this ISparqlDataset dataset, String sparqlQuery)
        {
        }

        /// <summary>
        /// Executes a SPARQL Update command set on the Dataset if it is updateable
        /// </summary>
        /// <param name="sparqlUpdateCommandSet"></param>
        public static void ExecuteUpdate(this ISparqlDataset dataset, String sparqlUpdateCommandSet)
        {
        }

    #endregion
    }
#endif

    /// <summary>
    /// This class contains extensions for the SpinWrappedDataset class. 
    /// These extensions are provided to handle the Dataset manipulation and updates made while processing queries and SPIN Rules/Constraints.
    /// TODO create additional EntailmentGraphs for each graph in the dataset ?
    /// </summary>
    internal static class DatasetUtil
    {
        #region Datasets creation and loading


        #endregion

        #region Dataset updates management

        internal static IGraph CreateUpdateControlledDataset(this SpinWrappedDataset queryModel)
        {
            IGraph dataset = queryModel._configuration;
            IUpdateableStorage storage = queryModel.UnderlyingStorage;

            if (!dataset.ContainsTriple(new Triple(RDFUtil.CreateUriNode(dataset.BaseUri), RDF.PropertyType, SD.ClassDataset)) && !dataset.ContainsTriple(new Triple(RDFUtil.CreateUriNode(dataset.BaseUri), RDF.PropertyType, RDFRuntime.ClassUpdateControlledDataset)))
            {
                throw new Exception("Invalid dataset to operate on : " + dataset.BaseUri.ToString());
            }

            // TODO ?See whether we should nest "transactions" or not? Currently not
            if (dataset.ContainsTriple(new Triple(RDFUtil.CreateUriNode(dataset.BaseUri), RDF.PropertyType, RDFRuntime.ClassUpdateControlledDataset)))
            {
                return dataset;
            }

            // creates the work Dataset
            SpinDatasetDescription workingset = new SpinDatasetDescription();
            workingset.BaseUri = UriFactory.Create(RDFRuntime.DATASET_NS_URI + Guid.NewGuid().ToString());
            workingset.Assert(dataset.Triples);

            INode datasetNode = RDFUtil.CreateUriNode(dataset.BaseUri);
            INode workingsetNode = RDFUtil.CreateUriNode(workingset.BaseUri);

            // adds workingset metadata
            workingset.Retract(dataset.GetTriplesWithSubject(datasetNode));
            workingset.Assert(workingsetNode, RDFRuntime.PropertyUpdatesGraph, workingsetNode);
            workingset.Assert(workingsetNode, RDF.PropertyType, RDFRuntime.ClassUpdateControlledDataset);
            workingset.Assert(workingsetNode, RDFRuntime.PropertyUpdatesDataset, datasetNode);
            workingset.Assert(workingsetNode, DCTerms.PropertyCreated, DateTime.Now.ToLiteral(RDFUtil.nodeFactory));

            queryModel._configuration = workingset;
            queryModel.Initialise();
            return workingset;
        }

        // TODO synchronise this 
        internal static void ApplyChanges(this SpinWrappedDataset queryModel)
        {
            IGraph currentDataset = queryModel._configuration;
            if (!currentDataset.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassUpdateControlledDataset).Any())
            {
                return;
            }
            // TODO check whether transactions are supported by the storage provider
            // Loads the original dataset will will flush into
            IGraph targetDataset = new Graph();
            targetDataset.BaseUri = ((IUriNode)currentDataset.GetTriplesWithSubjectPredicate(queryModel._datasetNode, RDFRuntime.PropertyUpdatesDataset).First().Object).Uri;
            queryModel._storage.LoadGraph(targetDataset, targetDataset.BaseUri);

            if (currentDataset.GetTriplesWithPredicateObject(RDF.PropertyType, SPIN.ClassConstraintViolation).Any())
            {
                throw new SpinException("Unable to flush the dataset while constraints are not satisfied");
            }
            // Flush pending changes to the graphs
            // TODO also handle entailment graphs updates
            foreach (Triple t in currentDataset.GetTriplesWithPredicateObject(RDF.PropertyType, SD.ClassGraph).Union(currentDataset.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassEntailmentGraph)))
            {
                IUriNode targetGraph = (IUriNode)t.Subject;
                if (currentDataset.GetTriplesWithPredicateObject(RDFRuntime.PropertyRemovesGraph, targetGraph).Any())
                {
                    targetDataset.Retract(currentDataset.GetTriplesWithObject(t.Object).Union(currentDataset.GetTriplesWithSubject(targetGraph)));
                }
                else
                {
                    Triple ucgTriple = currentDataset.GetTriplesWithPredicateObject(RDFRuntime.PropertyUpdatesGraph, targetGraph)
                        .Union(currentDataset.GetTriplesWithPredicateObject(RDFRuntime.PropertyReplacesGraph, targetGraph))
                        .FirstOrDefault();
                    if (ucgTriple != null)
                    {
                        IUriNode updateControlledGraph = (IUriNode)ucgTriple.Subject;
                        if (RDFUtil.sameTerm(ucgTriple.Predicate, RDFRuntime.PropertyReplacesGraph))
                        {
                            queryModel._storage.Update("WITH <" + updateControlledGraph.Uri.ToString() + "> DELETE { ?s <" + RDFRuntime.PropertyResets.Uri.ToString() + "> ?p } WHERE { ?s <" + RDFRuntime.PropertyResets.Uri.ToString() + "> ?p }"); // For safety only
                            queryModel._storage.Update("MOVE GRAPH <" + updateControlledGraph.Uri.ToString() + "> TO <" + targetGraph.Uri.ToString() + ">");
                        }
                        else
                        {
                            queryModel._storage.Update("DELETE { GRAPH <" + updateControlledGraph.Uri.ToString() + "> { ?s <" + RDFRuntime.PropertyResets.Uri.ToString() + "> ?p } . GRAPH <" + targetGraph.Uri.ToString() + "> { ?s ?p ?o } } USING <" + updateControlledGraph.Uri.ToString() + "> WHERE { ?s <" + RDFRuntime.PropertyResets.Uri.ToString() + "> ?p }");
                            queryModel._storage.Update("ADD GRAPH <" + updateControlledGraph.Uri.ToString() + "> TO <" + targetGraph.Uri.ToString() + ">");
                        }
                    }
                }
            }
            queryModel._storage.SaveGraph(targetDataset);
            queryModel.DisposeUpdateControlledDataset();
        }

        internal static void DiscardChanges(this SpinWrappedDataset queryModel)
        {
            queryModel.DisposeUpdateControlledDataset();
        }

        internal static void DisposeUpdateControlledDataset(this SpinWrappedDataset queryModel)
        {
            IGraph currentDataset = queryModel._configuration;
            IEnumerable<String> disposableGraphs = currentDataset.GetTriplesWithPredicate(RDFRuntime.PropertyUpdatesGraph)
                        .Union(currentDataset.GetTriplesWithPredicate(RDFRuntime.PropertyReplacesGraph))
                        .Union(currentDataset.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassExecutionGraph))
                        .Union(currentDataset.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassFunctionEvalResultSet))
                        .Union(currentDataset.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassUpdateControlGraph))
                        .Union(currentDataset.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassUpdateControlledDataset))
                        .Select(t => ((IUriNode)t.Subject).Uri.ToString());
            foreach (String graphUri in disposableGraphs)
            {
                queryModel._storage.DeleteGraph(graphUri);
            }
        }


        #endregion

        #region Graph Updates management

        internal static IResource GetUpdateControlledGraph(this SpinWrappedDataset queryModel, IResource graphNode)
        {
            IGraph currentDataset = queryModel._configuration;
            INode ucg = currentDataset.GetTriplesWithPredicateObject(RDFRuntime.PropertyUpdatesGraph, graphNode)
                .Union(currentDataset.GetTriplesWithPredicateObject(RDFRuntime.PropertyReplacesGraph, graphNode))
                .Select(t => t.Subject)
                .FirstOrDefault();
            return Resource.Get(ucg, queryModel.spinProcessor);
        }

        internal static IResource CreateUpdateControlledGraph(this SpinWrappedDataset queryModel, Uri graphUri, INode mode = null)
        {
            return queryModel.CreateUpdateControlledGraph(Resource.Get(RDFUtil.CreateUriNode(graphUri), queryModel.spinProcessor), mode);
        }

        internal static IResource CreateUpdateControlledGraph(this SpinWrappedDataset queryModel, IResource graphNode, INode mode = null)
        {
            IGraph currentDataset = queryModel.CreateUpdateControlledDataset();

            if (RDFUtil.sameTerm(graphNode, queryModel._datasetNode))
            {
                return graphNode;
            }

            INode updatedGraph = null;
            if (currentDataset.ContainsTriple(new Triple(graphNode, RDF.PropertyType, RDFRuntime.ClassReadOnlyGraph)))
            {
                throw new SpinException("The graph " + graphNode.Uri().ToString() + " is marked as Readonly for the current dataset");
            }
            if (!currentDataset.ContainsTriple(new Triple(graphNode.getSource(), RDF.PropertyType, SD.ClassGraph)))
            {
                currentDataset.Assert(graphNode.getSource(), Tools.CopyNode(RDF.PropertyType, currentDataset), Tools.CopyNode(SD.ClassGraph, currentDataset));
                currentDataset.Assert(graphNode.getSource(), Tools.CopyNode(RDFRuntime.PropertyUpdatesGraph, currentDataset), Tools.CopyNode(graphNode.getSource(), currentDataset));
            }
            updatedGraph = queryModel.GetUpdateControlledGraph(graphNode);
            if (updatedGraph == null)
            {
                if (mode == null)
                {
                    mode = RDFRuntime.PropertyUpdatesGraph;
                }
                currentDataset.Retract(currentDataset.GetTriplesWithObject(graphNode.getSource()).ToList());

                updatedGraph = currentDataset.CreateUriNode(UriFactory.Create(RDFRuntime.GRAPH_NS_URI + Guid.NewGuid().ToString()));
                currentDataset.Assert(updatedGraph, Tools.CopyNode(RDF.PropertyType, currentDataset), Tools.CopyNode(RDFRuntime.ClassUpdateControlGraph, currentDataset));
                currentDataset.Assert(updatedGraph, Tools.CopyNode(mode, currentDataset), Tools.CopyNode(graphNode.getSource(), currentDataset));
                // addition to simplify additional graph mapping constraints patterns
                currentDataset.Assert(updatedGraph, Tools.CopyNode(RDFRuntime.PropertyUpdatesGraph, currentDataset), updatedGraph);
            }
            else if (!RDFUtil.sameTerm(graphNode, updatedGraph))
            {
                updatedGraph = ((IResource)updatedGraph).getSource();
                currentDataset.Retract(graphNode.getSource(), Tools.CopyNode(RDFRuntime.PropertyUpdatesGraph, currentDataset), Tools.CopyNode(graphNode.getSource(), currentDataset));
                currentDataset.Assert(updatedGraph, Tools.CopyNode(mode, currentDataset), Tools.CopyNode(graphNode.getSource(), currentDataset));
            }
            return Resource.Get(updatedGraph, queryModel.spinProcessor);
        }

        #endregion

        #region "SPIN2Sparql translation for expression nodes"

        private static Regex sparqlEscapeCharacters = new Regex("(')", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        internal static String escapeString(String value)
        {
            return sparqlEscapeCharacters.Replace(value, "\\\\1");
        }

        internal static string StringForNode(IResource node, INamespaceMapper pm)
        {
            SpinWrappedDataset model = null; // TODO change this for a queryModel
            StringBuilder sb = new StringBuilder();
            if (node.canAs(SP.ClassExpression))
            {
                ((IPrintable)SPINFactory.asExpression(node)).print(new BaseSparqlFactory(model, sb));
            }
            else if (node.canAs(SP.ClassVariable))
            {
                ((IPrintable)SPINFactory.asVariable(node)).print(new BaseSparqlFactory(model, sb));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.PropertyType), SP.PropertyNot))
            {
                sb.Append("!(");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append(")");
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.PropertyType), SP.PropertyOr))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append(" || ");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.PropertyType), SP.PropertyAnd))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append(" && ");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.PropertyType), SP.PropertyEq))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append("=");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.PropertyType), SP.PropertyNeq))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append("!=");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.PropertyType), SP.PropertyLt))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append("<");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.PropertyType), SP.PropertyLeq))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append("<=");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.PropertyType), SP.PropertyGt))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append(">");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.PropertyType), SP.PropertyGeq))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append(">=");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.PropertyType), SP.PropertyBound))
            {
                sb.Append("bound(");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append(")");
            }
            else if (node.isUri())
            {
                sb.Append("<");
                sb.Append(node.Uri().ToString());
                sb.Append(">");
            }
            else if (node.isLiteral())
            {
                sb.Append(((ILiteralNode)node.getSource()).Value);
            }
            else
            {
                throw new Exception("Missing translation for expression " + node.getResource(RDF.PropertyType).Uri().ToString());
            }
            return sb.ToString();
        }

        #endregion

    }
}
