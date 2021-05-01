using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRDF.Query.Behemoth
{
    public class BgpBlock : IEvaluationBlock
    {
        private readonly List<IEvaluationBlock> _patterns;

        public BgpBlock(List<IEvaluationBlock> patterns)
        {
            _patterns = patterns;
        }

        public BgpBlock(IBgp bgp, BehemothEvaluationContext context)
        {
            _patterns = bgp.TriplePatterns.OfType<TriplePattern>().Select(x => new TriplePatternBlock(x, context)).Cast<IEvaluationBlock>().ToList();
        }

        public IEnumerable<Bindings> Evaluate(Bindings bindings)
        {
            return _patterns.Aggregate(bindings.AsEnumerable(),
                (b, m) => b.SelectMany(m.Evaluate));
        }
    }

    public interface IEvaluationBlock
    {
        IEnumerable<Bindings> Evaluate(Bindings input);
    }
}


