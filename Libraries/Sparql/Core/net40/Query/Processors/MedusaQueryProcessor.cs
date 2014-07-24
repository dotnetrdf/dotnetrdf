using VDS.RDF.Query.Compiler;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Query.Engine.Bgps;
using VDS.RDF.Query.Engine.Joins.Strategies;

namespace VDS.RDF.Query.Processors
{
    public class MedusaQueryProcessor
        : AlgebraQueryProcessor
    {
        public MedusaQueryProcessor(IBgpExecutor bgpExecutor)
            : this(new DefaultQueryCompiler(), bgpExecutor, new DefaultJoinStrategySelector()) { }

        public MedusaQueryProcessor(IBgpExecutor bgpExecutor, IJoinStrategySelector joinStrategySelector)
            : this(new DefaultQueryCompiler(), bgpExecutor, joinStrategySelector) { }

        public MedusaQueryProcessor(DefaultQueryCompiler defaultQueryCompiler, IBgpExecutor bgpExecutor, IJoinStrategySelector joinStrategySelector)
            : base(defaultQueryCompiler, new MedusaAlgebraExecutor(bgpExecutor, joinStrategySelector)) { }
    }
}
