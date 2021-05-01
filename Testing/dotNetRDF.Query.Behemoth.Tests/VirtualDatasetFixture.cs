using System;
using System.Collections.Generic;
using System.Text;
using VDS.RDF;

namespace dotNetRDF.Query.Behemoth.Tests
{
    public class VirtualDatasetFixture
    {
        public INamespaceMapper NamespaceMap { get; }
        public INodeFactory NodeFactory { get; }
        public BehemothEvaluationContext EvaluationContext { get; }

        public VirtualDatasetFixture()
        {
            var dataset = new VirtualDataset();
            NamespaceMap = new NamespaceMapper(false);
            NodeFactory = new NodeFactory(namespaceMap:NamespaceMap);
            dataset.AddTripleProvider(new IntegerTripleProvider(1_000_000, NodeFactory));
            EvaluationContext = new BehemothEvaluationContext(dataset);
        }
    }
}
