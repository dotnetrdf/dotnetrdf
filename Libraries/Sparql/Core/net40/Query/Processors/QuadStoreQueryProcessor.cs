using System.Collections.Generic;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine.Bgps;

namespace VDS.RDF.Query.Processors
{
    public class QuadStoreQueryProcessor
        : MedusaQueryProcessor
    {
        public QuadStoreQueryProcessor(IQuadStore quadStore)
            : base(new QuadStoreBgpExecutor(quadStore))
        {
            this.QuadStore = quadStore;
        }

        public IQuadStore QuadStore { get; private set; }

        public override IEnumerable<INode> DatasetNamedGraphs
        {
            get { return this.QuadStore.GraphNames; }
        }
    }
}
