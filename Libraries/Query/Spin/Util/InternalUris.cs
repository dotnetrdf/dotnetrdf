using System;

// 
namespace VDS.RDF.Query.Spin.Util
{

    public static class DCTerms
    {

        public readonly static String NS_URI = "http://purl.org/dc/terms/";

        public readonly static IUriNode PropertyCreated = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "created"));

    }

    public static class SD {

        public readonly static String NS_URI = "http://www.w3.org/ns/sparql-service-description#";

        public readonly static IUriNode ClassDataset = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Dataset"));
        public readonly static IUriNode ClassGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Graph"));

    }

    public static class SPINRuntime
    {
        public readonly static String NS_URI = "dotnetrdf-spin:";
        public readonly static String EVAL_NS_URI = NS_URI + "runtime#";

        public readonly static String DATASET_NS_URI = NS_URI + "dataset#";
        public readonly static String GRAPH_NS_URI = NS_URI + "graph#";

        public readonly static IUriNode ClassExecutionGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "ExecutionGraph"));
        public readonly static IUriNode ClassFunctionEvalResultSet = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "FunctionEvaluationResult"));
        public readonly static IUriNode ClassInferenceGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "InferenceGraph"));
        public readonly static IUriNode ClassReadOnlyGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "ReadOnlyGraph"));
        public readonly static IUriNode ClassUpdateControlledDataset = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "UpdateControlledDataset"));
        public readonly static IUriNode ClassUpdateControlledGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "UpdateControlledGraph"));

        public readonly static IUriNode PropertyReplacesGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "replacesGraph"));
        public readonly static IUriNode PropertyUpdatesDataset = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "updatesDataset"));
        public readonly static IUriNode PropertyUpdatesGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "updatesGraph"));

        public readonly static IUriNode PropertyResets = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "resets"));

        public readonly static IUriNode PropertyExecutionBinding = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "executionBinding"));
        public readonly static IUriNode PropertyExecutionBindingPattern = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "executionBindingPattern"));
    }
}
