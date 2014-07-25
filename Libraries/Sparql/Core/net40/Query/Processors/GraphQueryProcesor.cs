using VDS.RDF.Graphs;
using VDS.RDF.Query.Engine.Bgps;

namespace VDS.RDF.Query.Processors
{
    public class GraphQueryProcesor
        : MedusaQueryProcessor
    {
        public GraphQueryProcesor(IGraph g)
            : base(new GraphBgpExecutor(g))
        {
            this.Graph = g;
        }

        public IGraph Graph { get; private set; }
    }
}
