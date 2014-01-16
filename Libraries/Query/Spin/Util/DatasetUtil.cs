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
    /// </summary>
    internal static class DatasetUtil
    {
        internal static Dictionary<Uri, IGraph> datasets = new Dictionary<Uri, IGraph>(RDFUtil.uriComparer);

        private static SparqlParameterizedString isSystemGraphQuery;
        private static SparqlParameterizedString isSPINLibraryQuery;

        static DatasetUtil()
        {
            isSystemGraphQuery = new SparqlParameterizedString("ASK { @graph a ?type . FILTER ( ?type IN (@dataset,@transaction,@execution,@evaluation,@inference)) }");
            isSystemGraphQuery.SetParameter("dataset", SD.ClassDataset);
            isSystemGraphQuery.SetParameter("transaction", SPINRuntime.ClassUpdateControlledDataset);
            isSystemGraphQuery.SetParameter("execution", SPINRuntime.ClassExecutionGraph);
            isSystemGraphQuery.SetParameter("evaluation", SPINRuntime.ClassFunctionEvalResultSet);
            isSystemGraphQuery.SetParameter("inference", SPINRuntime.ClassInferenceGraph);

            isSPINLibraryQuery = new SparqlParameterizedString("ASK { @graph a @library }");
            isSPINLibraryQuery.SetParameter("library", SPIN.ClassLibraryOntology);
        }

        #region Datasets creation and loading

        //TODO make it an extension ?
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static IGraph ConstructDataset(IUpdateableStorage storage, Uri datasetUri = null, IEnumerable<Uri> graphsUri = null)
        {
            IGraph dataset = new Graph();
            lock (datasets)
            {
                dataset.BaseUri = datasetUri == null ? UriFactory.Create(SPINRuntime.DATASET_NS_URI + Guid.NewGuid().ToString()) : datasetUri;

                IUriNode datasetNode = RDFUtil.CreateUriNode(dataset.BaseUri);

                dataset.Assert(datasetNode, RDF.type, SD.ClassDataset);

                if (graphsUri == null)
                {
                    graphsUri = storage.ListGraphs();
                }

                foreach (Uri graphUri in graphsUri)
                {
                    isSystemGraphQuery.SetUri("graph", graphUri);
                    if (!((SparqlResultSet)storage.Query(isSystemGraphQuery.ToString())).Result)
                    {
                        isSPINLibraryQuery.SetUri("graph", graphUri);
                        if (!((SparqlResultSet)storage.Query(isSPINLibraryQuery.ToString())).Result)
                        {
                            dataset.Assert(RDFUtil.CreateUriNode(graphUri), RDF.type, SD.ClassGraph);

                        }
                        else
                        {
                            dataset.Assert(RDFUtil.CreateUriNode(graphUri), RDF.type, SPIN.ClassLibraryOntology);
                        }
                    }
                }
                if (!dataset.GetTriplesWithPredicateObject(RDF.type, SPINRuntime.ClassInferenceGraph).Any())
                {
                    Uri inferenceGraph = UriFactory.Create(SPINRuntime.GRAPH_NS_URI + Guid.NewGuid().ToString());
                    dataset.Assert(RDFUtil.CreateUriNode(inferenceGraph), RDF.type, SD.ClassGraph);
                    dataset.Assert(RDFUtil.CreateUriNode(inferenceGraph), RDF.type, SPINRuntime.ClassInferenceGraph);
                }

                datasets[dataset.BaseUri] = dataset;
            }
            return dataset;
        }

        //TODO make it an extension ?
        internal static IGraph LoadDataset(IUpdateableStorage storage, Uri datasetUri = null, IEnumerable<Uri> graphsUri = null)
        {
            IGraph dataset;
            if (datasetUri == null)
            {
                datasetUri = UriFactory.Create(SPINRuntime.DATASET_NS_URI + Guid.NewGuid().ToString());
            }
            if (datasets.ContainsKey(datasetUri))
            {
                dataset = datasets[datasetUri];
                if (dataset.GetTriplesWithPredicateObject(RDF.type, SD.ClassGraph).Any())
                {
                    return dataset;
                }
            }
            dataset = new Graph();
            dataset.BaseUri = datasetUri;
            storage.LoadGraph(dataset, datasetUri);

            if (!dataset.GetTriplesWithPredicateObject(RDF.type, SD.ClassGraph).Any())
            {
                dataset = ConstructDataset(storage, datasetUri, graphsUri);
            }
            datasets[datasetUri] = dataset;
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

        //TODO make it an extension ?
        internal static IGraph CreateUpdateControlledDataset(this SpinWrappedDataset queryModel)
        {
            IGraph dataset = queryModel._underlyingRDFDataset;
            IUpdateableStorage storage = queryModel.UnderlyingStorage;

            if (!dataset.ContainsTriple(new Triple(RDFUtil.CreateUriNode(dataset.BaseUri), RDF.type, SD.ClassDataset)) && !dataset.ContainsTriple(new Triple(RDFUtil.CreateUriNode(dataset.BaseUri), RDF.type, SPINRuntime.ClassUpdateControlledDataset)))
            {
                throw new Exception("Invalid dataset to operate on : " + dataset.BaseUri.ToString());
            }

            // TODO See wehther we should nest "transactions" or not currently not
            if (dataset.ContainsTriple(new Triple(RDFUtil.CreateUriNode(dataset.BaseUri), RDF.type, SPINRuntime.ClassUpdateControlledDataset)))
            {
                return dataset;
            }

            // creates the transaction
            IGraph transactionDataset = new Graph();
            transactionDataset.BaseUri = UriFactory.Create(SPINRuntime.DATASET_NS_URI + Guid.NewGuid().ToString());
            transactionDataset.Assert(dataset.Triples);

            INode datasetNode = RDFUtil.CreateUriNode(dataset.BaseUri);
            INode transactionNode = RDFUtil.CreateUriNode(transactionDataset.BaseUri);

            // adds transaction metadata
            transactionDataset.Retract(dataset.GetTriplesWithSubject(datasetNode));
            transactionDataset.Assert(transactionNode, SPINRuntime.PropertyUpdatesGraph, transactionNode);
            transactionDataset.Assert(transactionNode, RDF.type, SPINRuntime.ClassUpdateControlledDataset);
            transactionDataset.Assert(transactionNode, SPINRuntime.PropertyUpdatesDataset, datasetNode);
            transactionDataset.Assert(transactionNode, DCTerms.PropertyCreated, DateTime.Now.ToLiteral(RDFUtil.nodeFactory));

            queryModel._underlyingRDFDataset = transactionDataset;
            queryModel.Initialise();
            return transactionDataset;
        }

        internal static IResource GetUpdateControlledGraph(this SpinWrappedDataset queryModel, IResource graphNode)
        {
            IGraph currentDataset = queryModel._underlyingRDFDataset;
            INode ucg = currentDataset.GetTriplesWithPredicateObject(SPINRuntime.PropertyUpdatesGraph, graphNode)
                .Union(currentDataset.GetTriplesWithPredicateObject(SPINRuntime.PropertyReplacesGraph, graphNode))
                .Select(t => t.Subject)
                .FirstOrDefault();
            return Resource.Get(ucg, queryModel.spinProcessor);
        }

        internal static IResource CreateUpdateControlledGraph(this SpinWrappedDataset queryModel, IResource graphNode)
        {
            // TODO should we always do this or should we provide another CreateUpdate
            IGraph currentDataset = queryModel.CreateUpdateControlledDataset();

            if (RDFUtil.sameTerm(graphNode, queryModel._datasetNode))
            {
                return graphNode;
            }

            INode updatedGraph = null;
            if (!currentDataset.ContainsTriple(new Triple(graphNode, RDF.type, SPINRuntime.ClassReadOnlyGraph)))
            {
                throw new SpinException("The graph " + graphNode.Uri().ToString() + " is marked as Readonly for the current dataset");
            }
            if (!currentDataset.ContainsTriple(new Triple(graphNode, RDF.type, SD.ClassGraph)))
            {
                currentDataset.Assert(graphNode, Tools.CopyNode(RDF.type, currentDataset), Tools.CopyNode(SD.ClassGraph, currentDataset));
            }
            updatedGraph = queryModel.GetUpdateControlledGraph(graphNode);
            if (updatedGraph == null)
            {
                updatedGraph = currentDataset.CreateUriNode(UriFactory.Create(SPINRuntime.GRAPH_NS_URI + Guid.NewGuid().ToString()));
                currentDataset.Assert(updatedGraph, Tools.CopyNode(RDF.type, currentDataset), Tools.CopyNode(SD.ClassGraph, currentDataset));
                currentDataset.Assert(updatedGraph, Tools.CopyNode(SPINRuntime.PropertyUpdatesGraph, currentDataset), Tools.CopyNode(graphNode, currentDataset));
                // addition to simplify additional graph mapping constraints patterns
                currentDataset.Assert(updatedGraph, Tools.CopyNode(SPINRuntime.PropertyUpdatesGraph, currentDataset), updatedGraph);
            }
            return Resource.Get(updatedGraph == null ? graphNode : updatedGraph, queryModel.spinProcessor);
        }

        internal static IResource CreateExecutionGraph(this SpinWrappedDataset queryModel)
        {
            IGraph _underlyingRDFDataset = queryModel.CreateUpdateControlledDataset();
            INode executionGraph = _underlyingRDFDataset.CreateUriNode(UriFactory.Create(SPINRuntime.GRAPH_NS_URI + Guid.NewGuid().ToString()));
            _underlyingRDFDataset.Assert(executionGraph, Tools.CopyNode(RDF.type, _underlyingRDFDataset), Tools.CopyNode(SPINRuntime.ClassExecutionGraph, _underlyingRDFDataset));
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
            else if (RDFUtil.sameTerm(node.getResource(RDF.type), SP.PropertyNot))
            {
                sb.Append("!(");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append(")");
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.type), SP.PropertyOr))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append(" || ");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.type), SP.PropertyAnd))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append(" && ");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.type), SP.PropertyEq))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append("=");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.type), SP.PropertyNeq))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append("!=");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.type), SP.PropertyLt))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append("<");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.type), SP.PropertyLeq))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append("<=");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.type), SP.PropertyGt))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append(">");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.type), SP.PropertyGeq))
            {
                sb.Append(StringForNode(node.getObject(SP.PropertyArg1), pm));
                sb.Append(">=");
                sb.Append(StringForNode(node.getObject(SP.PropertyArg2), pm));
            }
            else if (RDFUtil.sameTerm(node.getResource(RDF.type), SP.PropertyBound))
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
                throw new Exception("Missing translation for expression " + node.getResource(RDF.type).Uri().ToString());
            }
            return sb.ToString();
        }

        #endregion

    }
}
