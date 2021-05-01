// unset

using VDS.RDF.Query.Datasets;

namespace dotNetRDF.Query.Behemoth
{
    public class BehemothEvaluationContext
    {
        public ISparqlDataset Data { get; }

        public BehemothEvaluationContext(ISparqlDataset dataset)
        {
            Data = dataset;
        }
    }
}