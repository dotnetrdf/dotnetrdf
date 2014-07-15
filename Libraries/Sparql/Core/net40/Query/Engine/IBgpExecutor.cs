using System.Collections.Generic;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Engine
{
    public interface IBgpExecutor {

        IEnumerable<ISet> Match(INode graphName, Triple t);

        IEnumerable<ISet> Match(INode graphName, Triple t, ISet input);
    }
}