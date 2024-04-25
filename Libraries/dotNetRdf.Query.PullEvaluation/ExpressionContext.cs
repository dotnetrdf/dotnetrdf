using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation
{
    public class ExpressionContext(ISet bindings, IRefNode? activeGraph)
    {
        public readonly ISet Bindings = bindings;
        public readonly IRefNode? ActiveGraph = activeGraph;
        private Dictionary<string, IBlankNode>? _blankNodeMap = null;

        public bool TryGetBlankNode(string key, out IBlankNode bNode)
        {
            bNode = null;
            return _blankNodeMap?.TryGetValue(key, out bNode) ?? false;
        }

        public void MapBlankNode(string key, IBlankNode bNode)
        {
            _blankNodeMap ??= new Dictionary<string, IBlankNode>();
            _blankNodeMap.Add(key, bNode);
        }
    }
}