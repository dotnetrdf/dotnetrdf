using System;

// 
namespace VDS.RDF.Query.Spin.Util
{

    public static class DCTerms
    {

        public readonly static String NS_URI = "http://purl.org/dc/terms/";

        public readonly static IUriNode PropertyCreated = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "created"));

    }

    public static class SD
    {

        public readonly static String NS_URI = "http://www.w3.org/ns/sparql-service-description#";

        public readonly static IUriNode ClassDataset = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Dataset"));
        public readonly static IUriNode ClassGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Graph"));

    }

    public static class RDFRuntime
    {
        public readonly static String NS_URI = "dotnetrdf-spin:";
        public readonly static String EVAL_NS_URI = NS_URI + "runtime#";

        public readonly static String DATASET_NS_URI = NS_URI + "dataset#";
        public readonly static String GRAPH_NS_URI = NS_URI + "graph#";
        public readonly static String ENTAILMENTGRAPH_NS_URI = NS_URI + "entailment-graph#";
        public readonly static String REMOVALGRAPH_NS_URI = NS_URI + "monitored-removals-graph#";
        public readonly static String ADDITIONGRAPH_NS_URI = NS_URI + "monitored-additions-graph#";

        public readonly static IUriNode PropertyUpdatesDataset = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "updatesDataset"));
        public readonly static IUriNode ClassUpdateControlledDataset = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "UpdateControlledDataset"));

        public readonly static IUriNode ClassExecutionGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "ExecutionGraph"));
        public readonly static IUriNode ClassFunctionEvalResultSet = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "FunctionEvaluationResult"));
        public readonly static IUriNode ClassSPINInferenceGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "SPINInferenceGraph"));
        public readonly static IUriNode ClassEntailmentGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "EntailmentGraph"));
        public readonly static IUriNode ClassReadOnlyGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "ReadOnlyGraph"));
        public readonly static IUriNode ClassUpdateControlGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "UpdateControlledGraph"));

        public readonly static IUriNode PropertyEntailsGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "entailsGraph"));

        public readonly static IUriNode PropertyReplacesGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "replacesGraph"));
        public readonly static IUriNode PropertyUpdatesGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "updatesGraph"));
        public readonly static IUriNode PropertyRemovesGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "removesGraph"));

        public readonly static IUriNode PropertyAddTriplesTo = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "addTriplesTo"));
        public readonly static IUriNode PropertyDeleteTriplesFrom = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "deleteTriplesFrom"));

        public readonly static IUriNode PropertyResets = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "resets"));
        public readonly static IUriNode PropertyTypeAdded = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "typeAdded"));
        public readonly static IUriNode PropertyTypeRemoved = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "typeRemoved"));

        public readonly static IUriNode PropertyExecutionBinding = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "executionBinding"));
        public readonly static IUriNode PropertyExecutionBindingPattern = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "executionBindingPattern"));

        public static Uri NewTempGraphUri()
        {
            return UriFactory.Create(RDFRuntime.EVAL_NS_URI + Guid.NewGuid().ToString());
        }

        public static Uri NewUpdateControlGraphUri()
        {
            return UriFactory.Create(RDFRuntime.GRAPH_NS_URI + Guid.NewGuid().ToString());
        }

        public static Uri NewTempDatasetUri()
        {
            return UriFactory.Create(RDFRuntime.DATASET_NS_URI + Guid.NewGuid().ToString());
        }
    }
}
