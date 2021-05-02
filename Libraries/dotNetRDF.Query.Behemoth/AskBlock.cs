using System.Collections.Generic;
using System.Linq;

namespace dotNetRDF.Query.Behemoth
{
    public class AskBlock : IEvaluationBlock
    {
        private readonly IEvaluationBlock _inner;

        public AskBlock(IEvaluationBlock inner)
        {
            _inner = inner;
        }


        public IEnumerable<Bindings> Evaluate(Bindings input)
        {
            return _inner.Evaluate(input).Take(1);
        }
    }
}
