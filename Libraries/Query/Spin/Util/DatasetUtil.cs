using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using org.topbraid.spin.model;
using org.topbraid.spin.vocabulary;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Storage;

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
    /// TODO Should we create additional EntailmentGraph for each graph in the dataset ?
    /// TODO add a specific graph for constructors logging and constraints checking ?
    /// </summary>
    internal static class DatasetUtil
    {
        internal static Dictionary<Uri, IGraph> datasets = new Dictionary<Uri, IGraph>(RDFUtil.uriComparer);

        private static SparqlParameterizedString isSystemGraphQuery;
        private static SparqlParameterizedString isSPINLibraryQuery;

        static DatasetUtil()
        {
            isSystemGraphQuery = new SparqlParameterizedString("ASK { @graph a ?type . FILTER ( ?type IN (@dataset,@updateControlledDataset) ) }");
            isSystemGraphQuery.SetParameter("dataset", SD.ClassDataset);
            isSystemGraphQuery.SetParameter("updateControlledDataset", SPINRuntime.ClassUpdateControlledDataset);

            isSPINLibraryQuery = new SparqlParameterizedString("ASK { @graph a @library }");
            isSPINLibraryQuery.SetParameter("library", SPIN.ClassLibraryOntology);
        }

        #region Datasets creation and loading

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static IGraph ConstructDataset(IUpdateableStorage storage, Uri datasetUri = null, IEnumerable<Uri> graphsUri = null)
        {
            IGraph dataset = new Graph();
            lock (datasets)
            {
                dataset.BaseUri = datasetUri == null ? UriFactory.Create(SPINRuntime.DATASET_NS_URI + Guid.NewGuid().ToString()) : datasetUri;

                IUriNode datasetNode = RDFUtil.CreateUriNode(dataset.BaseUri);

                dataset.Assert(datasetNode, RDF.PropertyType, SD.ClassDataset);

                if (graphsUri == null)
                {
                    graphsUri = storage.ListGraphs();
                }

                foreach (Uri graphUri in graphsUri)
                {
                    // TODO check whether the graph is registered somewhere as a LibraryOntology
                    dataset.Assert(RDFUtil.CreateUriNode(graphUri), RDF.PropertyType, SD.ClassGraph);
                    /*
                    isSystemGraphQuery.SetUri("graph", graphUri);
                    if (!((SparqlResultSet)storage.Query(isSystemGraphQuery.ToString())).Result)
                    {
                        isSPINLibraryQuery.SetUri("graph", graphUri);
                        if (!((SparqlResultSet)storage.Query(isSPINLibraryQuery.ToString())).Result)
                        {
                            

                        }
                        else
                        {
                            dataset.Assert(RDFUtil.CreateUriNode(graphUri), RDF.type, SPIN.ClassLibraryOntology);
                        }
                    }
                    */
                }
                if (!dataset.GetTriplesWithPredicateObject(RDF.PropertyType, SPINRuntime.ClassInferenceGraph).Any())
                {
                    Uri inferenceGraph = UriFactory.Create(SPINRuntime.GRAPH_NS_URI + Guid.NewGuid().ToString());
                    dataset.Assert(RDFUtil.CreateUriNode(inferenceGraph), RDF.PropertyType, SD.ClassGraph);
                    dataset.Assert(RDFUtil.CreateUriNode(inferenceGraph), RDF.PropertyType, SPINRuntime.ClassInferenceGraph);
                }

                datasets[dataset.BaseUri] = dataset;
                storage.SaveGraph(dataset);
            }
            return dataset;
        }

        internal static IGraph LoadDataset(IUpdateableStorage storage, Uri datasetUri = null, IEnumerable<Uri> graphsUri = null)
        {
            IGraph dataset;
            if (datasetUri == null)
            {
                SparqlResultSet datasetDiscovery = (SparqlResultSet)storage.Query("SELECT ?dataset WHERE {?dataset a <" + SD.ClassDataset.Uri.ToString() + ">}");
                int datasetCount = datasetDiscovery.Results.Count;
                if (datasetCount > 1)
                {
                    throw new Exception("Multiple datasets found in the current storage provider. Please specify which to use through the datasetUri parameter.");
                }
                else if (datasetCount == 1)
                {
                    datasetUri = ((IUriNode)datasetDiscovery.Results.FirstOrDefault().Value("dataset")).Uri;
                }
                else
                {
                    datasetUri = UriFactory.Create(SPINRuntime.DATASET_NS_URI + Guid.NewGuid().ToString());
                }
            }
            /* Do not cache yet
            if (datasets.ContainsKey(datasetUri))
            {
                dataset = datasets[datasetUri];
                if (dataset.GetTriplesWithPredicateObject(RDF.type, SD.ClassGraph).Any())
                {
                    return dataset;
                }
            }
            */
            dataset = new Graph();
            dataset.BaseUri = datasetUri;
            storage.LoadGraph(dataset, datasetUri);

            if (dataset.IsEmpty)
            {
                dataset = ConstructDataset(storage, datasetUri, graphsUri);
            }
            /* Do not cache yet
            datasets[datasetUri] = dataset;
            */
            return dataset;
        }

        internal static void PersistDataset(this SpinWrappedDataset queryModel)
        {
            if (queryModel.HasDatasetChanged)
            {
                queryModel._storage.SaveGraph(queryModel._underlyingRDFDataset);
                queryModel.HasDatasetChanged = false;
            }
        }

        #endregion

        #region Dataset updates management

        internal static IGraph CreateUpdateControlledDataset(this SpinWrappedDataset queryModel)
        {
            IGraph dataset = queryModel._underlyingRDFDataset;
            IUpdateableStorage storage = queryModel.UnderlyingStorage;

            if (!dataset.ContainsTriple(new Triple(RDFUtil.CreateUriNode(dataset.BaseUri), RDF.PropertyType, SD.ClassDataset)) && !dataset.ContainsTriple(new Triple(RDFUtil.CreateUriNode(dataset.BaseUri), RDF.PropertyType, SPINRuntime.ClassUpdateControlledDataset)))
            {
                throw new Exception("Invalid dataset to operate on : " + dataset.BaseUri.ToString());
            }

            // TODO ?See whether we should nest "transactions" or not? Currently not
            if (dataset.ContainsTriple(new Triple(RDFUtil.CreateUriNode(dataset.BaseUri), RDF.PropertyType, SPINRuntime.ClassUpdateControlledDataset)))
            {
                return dataset;
            }

            // creates the work Dataset
            IGraph workingset = new Graph();
            workingset.BaseUri = UriFactory.Create(SPINRuntime.DATASET_NS_URI + Guid.NewGuid().ToString());
            workingset.Assert(dataset.Triples);

            INode datasetNode = RDFUtil.CreateUriNode(dataset.BaseUri);
            INode workingsetNode = RDFUtil.CreateUriNode(workingset.BaseUri);

            // adds workingset metadata
            workingset.Retract(dataset.GetTriplesWithSubject(datasetNode));
            workingset.Assert(workingsetNode, SPINRuntime.PropertyUpdatesGraph, workingsetNode);
            workingset.Assert(workingsetNode, RDF.PropertyType, SPINRuntime.ClassUpdateControlledDataset);
            workingset.Assert(workingsetNode, SPINRuntime.PropertyUpdatesDataset, datasetNode);
            workingset.Assert(workingsetNode, DCTerms.PropertyCreated, DateTime.Now.ToLiteral(RDFUtil.nodeFactory));

            queryModel._underlyingRDFDataset = workingset;
            queryModel.Initialise();
            return workingset;
        }

        // TODO synchronise this 
        internal static void ApplyChanges(this SpinWrappedDataset queryModel)
        {
            IGraph currentDataset = queryModel._underlyingRDFDataset;
            if (!currentDataset.GetTriplesWithPredicateObject(RDF.PropertyType, SPINRuntime.ClassUpdateControlledDataset).Any())
            {
                return;
            }
            // TODO check whether transactions are supported by the storage provider
            // Loads the original dataset will will flush into
            IGraph targetDataset = new Graph();
            targetDataset.BaseUri = ((IUriNode)currentDataset.GetTriplesWithSubjectPredicate(queryModel._datasetNode, SPINRuntime.PropertyUpdatesDataset).First().Object).Uri;
            queryModel._storage.LoadGraph(targetDataset, targetDataset.BaseUri);

            if (currentDataset.GetTriplesWithPredicateObject(RDF.PropertyType, SPIN.ClassConstraintViolation).Any())
            {
                throw new SpinException("Unable to flush the dataset while constraints are not satisfied");
            }
            // Flush pending changes to the graphs
            foreach (Triple t in currentDataset.GetTriplesWithPredicateObject(RDF.PropertyType, SD.ClassGraph))
            {
                IUriNode targetGraph = (IUriNode)t.Subject;
                if (currentDataset.GetTriplesWithPredicateObject(SPINRuntime.PropertyRemovesGraph, targetGraph).Any())
                {
                    targetDataset.Retract(currentDataset.GetTriplesWithObject(t.Object).Union(currentDataset.GetTriplesWithSubject(targetGraph)));
                }
                else
                {
                    Triple ucgTriple = currentDataset.GetTriplesWithPredicateObject(SPINRuntime.PropertyUpdatesGraph, targetGraph)
                        .Union(currentDataset.GetTriplesWithPredicateObject(SPINRuntime.PropertyReplacesGraph, targetGraph))
                        .FirstOrDefault();
                    if (ucgTriple != null)
                    {
                        IUriNode updateControlledGraph = (IUriNode)ucgTriple.Subject;
                        if (RDFUtil.sameTerm(ucgTriple.Predicate, SPINRuntime.PropertyReplacesGraph))
                        {
                            queryModel._storage.Update("WITH <" + updateControlledGraph.Uri.ToString() + "> DELETE { ?s <" + SPINRuntime.PropertyResets.Uri.ToString() + "> ?p } WHERE { ?s <" + SPINRuntime.PropertyResets.Uri.ToString() + "> ?p }"); // For safety only
                            queryModel._storage.Update("MOVE GRAPH <" + updateControlledGraph.Uri.ToString() + "> TO <" + targetGraph.Uri.ToString() + ">");
                        }
                        else
                        {
                            // TODO check wheter SPARQL updates expect the targetGraph in the USING clause
                            queryModel._storage.Update("DELETE { GRAPH <" + updateControlledGraph.Uri.ToString() + "> { ?s <" + SPINRuntime.PropertyResets.Uri.ToString() + "> ?p } . GRAPH <" + targetGraph.Uri.ToString() + "> { ?s ?p ?o } } USING <" + updateControlledGraph.Uri.ToString() + "> WHERE { ?s <" + SPINRuntime.PropertyResets.Uri.ToString() + "> ?p }");
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
            IGraph currentDataset = queryModel._underlyingRDFDataset;
            IEnumerable<String> disposableGraphs = currentDataset.GetTriplesWithPredicate(SPINRuntime.PropertyUpdatesGraph)
                        .Union(currentDataset.GetTriplesWithPredicate(SPINRuntime.PropertyReplacesGraph))
                        .Union(currentDataset.GetTriplesWithPredicateObject(RDF.PropertyType, SPINRuntime.ClassExecutionGraph))
                        .Union(currentDataset.GetTriplesWithPredicateObject(RDF.PropertyType, SPINRuntime.ClassFunctionEvalResultSet))
                        .Union(currentDataset.GetTriplesWithPredicateObject(RDF.PropertyType, SPINRuntime.ClassUpdateControlledGraph))
                        .Union(currentDataset.GetTriplesWithPredicateObject(RDF.PropertyType, SPINRuntime.ClassUpdateControlledDataset))
                        .Select(t => ((IUriNode)t.Subject).Uri.ToString());
            foreach (String graphUri in disposableGraphs) {
                queryModel._storage.DeleteGraph(graphUri);
            }
        }


        #endregion

        #region Graph Updates management

        internal static IResource GetUpdateControlledGraph(this SpinWrappedDataset queryModel, IResource graphNode)
        {
            IGraph currentDataset = queryModel._underlyingRDFDataset;
            INode ucg = currentDataset.GetTriplesWithPredicateObject(SPINRuntime.PropertyUpdatesGraph, graphNode)
                .Union(currentDataset.GetTriplesWithPredicateObject(SPINRuntime.PropertyReplacesGraph, graphNode))
                .Select(t => t.Subject)
                .FirstOrDefault();
            return Resource.Get(ucg, queryModel.spinProcessor);
        }

        internal static IResource CreateUpdateControlledGraph(this SpinWrappedDataset queryModel, IResource graphNode, INode mode = null)
        {
            IGraph currentDataset = queryModel.CreateUpdateControlledDataset();

            if (RDFUtil.sameTerm(graphNode, queryModel._datasetNode))
            {
                return graphNode;
            }

            INode updatedGraph = null;
            if (currentDataset.ContainsTriple(new Triple(graphNode, RDF.PropertyType, SPINRuntime.ClassReadOnlyGraph)))
            {
                throw new SpinException("The graph " + graphNode.Uri().ToString() + " is marked as Readonly for the current dataset");
            }
            if (!currentDataset.ContainsTriple(new Triple(graphNode.getSource(), RDF.PropertyType, SD.ClassGraph)))
            {
                currentDataset.Assert(graphNode.getSource(), Tools.CopyNode(RDF.PropertyType, currentDataset), Tools.CopyNode(SD.ClassGraph, currentDataset));
                currentDataset.Assert(graphNode.getSource(), Tools.CopyNode(SPINRuntime.PropertyUpdatesGraph, currentDataset), Tools.CopyNode(graphNode.getSource(), currentDataset));
            }
            updatedGraph = queryModel.GetUpdateControlledGraph(graphNode);
            if (updatedGraph == null)
            {
                if (mode == null)
                {
                    mode = SPINRuntime.PropertyUpdatesGraph;
                }
                currentDataset.Retract(currentDataset.GetTriplesWithObject(graphNode.getSource()).ToList());

                updatedGraph = currentDataset.CreateUriNode(UriFactory.Create(SPINRuntime.GRAPH_NS_URI + Guid.NewGuid().ToString()));
                currentDataset.Assert(updatedGraph, Tools.CopyNode(RDF.PropertyType, currentDataset), Tools.CopyNode(SD.ClassGraph, currentDataset));
                currentDataset.Assert(updatedGraph, Tools.CopyNode(mode, currentDataset), Tools.CopyNode(graphNode.getSource(), currentDataset));
                // addition to simplify additional graph mapping constraints patterns
                currentDataset.Assert(updatedGraph, Tools.CopyNode(SPINRuntime.PropertyUpdatesGraph, currentDataset), updatedGraph);
            }
            else if (!RDFUtil.sameTerm(graphNode, updatedGraph))
            {
                updatedGraph = ((IResource)updatedGraph).getSource();
                currentDataset.Retract(graphNode.getSource(), Tools.CopyNode(SPINRuntime.PropertyUpdatesGraph, currentDataset), Tools.CopyNode(graphNode.getSource(), currentDataset));
                currentDataset.Assert(updatedGraph, Tools.CopyNode(mode, currentDataset), Tools.CopyNode(graphNode.getSource(), currentDataset));
            }
            return Resource.Get(updatedGraph, queryModel.spinProcessor);
        }

        /// <summary>
        /// Execution Graph is meant to capture SPIN specific triples on update queries i.e. new RDF.type triples or ConstraintViolations related triples
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        internal static IResource CreateExecutionGraph(this SpinWrappedDataset queryModel)
        {
            IGraph _underlyingRDFDataset = queryModel.CreateUpdateControlledDataset();
            INode executionGraph = queryModel.GetExecutionGraph();
            if (executionGraph == null)
            {
                _underlyingRDFDataset.CreateUriNode(UriFactory.Create(SPINRuntime.GRAPH_NS_URI + Guid.NewGuid().ToString()));
                _underlyingRDFDataset.Assert(executionGraph, Tools.CopyNode(RDF.PropertyType, _underlyingRDFDataset), Tools.CopyNode(SPINRuntime.ClassExecutionGraph, _underlyingRDFDataset));
            }
            else
            {
                queryModel._storage.Query("CLEAR GRAPH <" + ((IResource)executionGraph).Uri().ToString() + ">");
            }
            return Resource.Get(executionGraph, queryModel.spinProcessor);
        }

        internal static IResource GetExecutionGraph(this SpinWrappedDataset queryModel)
        {
            IGraph dataset = queryModel.CreateUpdateControlledDataset();
            INode executionGraph = dataset.GetTriplesWithPredicateObject(RDF.PropertyType, SPINRuntime.ClassExecutionGraph).Select(t => t.Subject).FirstOrDefault();
            return Resource.Get(executionGraph, queryModel.spinProcessor);
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
                ((IPrintable)SPINFactory.asExpression(node)).print(new StringContextualSparqlPrinter(model, sb));
            }
            else if (node.canAs(SP.ClassVariable))
            {
                ((IPrintable)SPINFactory.asVariable(node)).print(new StringContextualSparqlPrinter(model, sb));
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
