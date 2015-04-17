using System;
using VDS.RDF.Query.Spin.Utility;
using System.Collections.Generic;

// 
namespace VDS.RDF.Query.Spin.Core.Runtime
{

    // TODO merge this with DOTNETRDF_TRANS
    public static class SpinRuntime
    {
        public readonly static String NS_URI = "dotnetrdf-spin:";
        public readonly static String EVAL_NS_URI = NS_URI + "runtime#";

        public readonly static String CONTEXT_NS_URI = NS_URI + "transaction:";
        public readonly static String GRAPH_NS_URI = NS_URI + "graph#";
        public readonly static String ENTAILMENTGRAPH_NS_URI = NS_URI + "entailment-graph#";
        public readonly static String REMOVALGRAPH_NS_URI = NS_URI + "monitored-removals-graph#";
        public readonly static String ADDITIONGRAPH_NS_URI = NS_URI + "monitored-additions-graph#";

        public readonly static IUriNode NULL = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "nil"));
        
        public readonly static IUriNode BindDefaultGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "BindDefaultGraph"));
        public readonly static IUriNode BindNamedGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "BindNamedGraph"));

        public readonly static IUriNode PropertyUpdatesDataset = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "updatesDataset"));
        public readonly static IUriNode ClassTransaction = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Transaction"));

        public readonly static IUriNode ClassRuntimeGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "RuntimeGraph"));
        public readonly static IUriNode ClassExecutionGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "ExecutionGraph"));
        public readonly static IUriNode ClassFunctionEvalResultSet = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "FunctionEvaluationResult"));
        public readonly static IUriNode ClassSPINInferenceGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "SPINInferenceGraph"));
        public readonly static IUriNode ClassEntailmentGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "EntailmentGraph"));
        public readonly static IUriNode ClassReadOnlyGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "ReadOnlyGraph"));
        public readonly static IUriNode ClassUpdateControlGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "UpdateControlledGraph"));

        public readonly static IUriNode PropertyEntailsGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "entailsGraph"));

        public readonly static IUriNode PropertyReplacesGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "replacesGraph"));
        public readonly static IUriNode PropertyUpdatesGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "updatesGraph"));
        public readonly static IUriNode PropertyRemovesGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "removesGraph"));

        public readonly static IUriNode PropertyAddTriplesTo = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "addTriplesTo"));
        public readonly static IUriNode PropertyDeleteTriplesFrom = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "deleteTriplesFrom"));

        public readonly static IUriNode PropertyHasChanged = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "hasChanged"));
        public readonly static IUriNode PropertyResets = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "resets"));
        public readonly static IUriNode PropertyTypeAdded = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "typeAdded"));
        public readonly static IUriNode PropertyTypeRemoved = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "typeRemoved"));

        public readonly static IUriNode PropertyExecutionRestrictedTo = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "executionRestrictedTo"));


        public readonly static IUriNode PropertyExecutionBinding = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "executionBinding"));
        public readonly static IUriNode PropertyExecutionBindingPattern = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "executionBindingPattern"));

        public readonly static String ThisClassBindingVar = "_" + Guid.NewGuid().ToString().Replace("-", "");
        public static Uri NewTempGraphUri()
        {
            return UriFactory.Create(SpinRuntime.EVAL_NS_URI + Guid.NewGuid().ToString());
        }

        public static Uri NewUpdateControlGraphUri()
        {
            return UriFactory.Create(SpinRuntime.GRAPH_NS_URI + Guid.NewGuid().ToString());
        }

        public static Uri NewContextUri()
        {
            return UriFactory.Create(SpinRuntime.CONTEXT_NS_URI + Guid.NewGuid().ToString());
        }

        public static HashSet<Uri> RuntimeGraphClassUris = new HashSet<Uri>(RDFHelper.uriComparer) { 
            ClassTransaction.Uri, 
            ClassUpdateControlGraph.Uri,
            ClassRuntimeGraph.Uri,
            ClassExecutionGraph.Uri
        };
    }
}
