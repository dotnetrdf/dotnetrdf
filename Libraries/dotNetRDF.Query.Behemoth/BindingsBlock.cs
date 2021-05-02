using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Query.Patterns;

namespace dotNetRDF.Query.Behemoth
{
    public class BindingsBlock : IEvaluationBlock
    {
        private readonly BindingsPattern _pattern;
        private readonly BehemothEvaluationContext _context;

        public BindingsBlock(BindingsPattern pattern, BehemothEvaluationContext context)
        {
            _pattern = pattern;
            _context = context;
        }

        public IEnumerable<Bindings> Evaluate(Bindings input)
        {
            return _pattern.Tuples.Select(t =>
                new Bindings(t.Values.Select(x =>
                    new KeyValuePair<string, INode>(x.Key, x.Value?.Construct(_context.ConstructContext)))));
        }
    }
}
