// unset

using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Datasets;

namespace dotNetRDF.Query.Behemoth
{
    public class BehemothEvaluationContext
    {
        public ISparqlDataset Data
        {
            get;
        }

        public ConstructContext ConstructContext { get; }

        public BehemothEvaluationContext(ISparqlDataset dataset)
        {
            Data = dataset;
            ConstructContext = new ConstructContext(new NodeFactory(), new Set(), true);
        }
    }
}