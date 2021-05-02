using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;

namespace dotNetRDF.Query.Behemoth
{
    public class SelectBlock : IEvaluationBlock
    {
        private IEvaluationBlock _inner;
        private bool _selectAll;
        private List<string> _selectVars;

        public SelectBlock(IEvaluationBlock innerBlock, bool selectAll, List<string> selectVars)
        {
            _inner = innerBlock;
            _selectAll = selectAll;
            _selectVars = selectVars;
        }
        public IEnumerable<Bindings> Evaluate(Bindings input)
        {
            if (_selectAll) return _inner.Evaluate(input);
            return _inner.Evaluate(input).Select(x=>x.Select(_selectVars));
        }
    }
}
