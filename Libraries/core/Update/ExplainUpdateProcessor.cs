using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Update
{
    /// <summary>
    /// An Update Processor that extends the Leviathan Engine to include explanations of the query portions of the Updates
    /// </summary>
    public class ExplainUpdateProcessor
        : LeviathanUpdateProcessor
    {
        private ExplanationLevel _level = ExplanationLevel.Default;

        public ExplainUpdateProcessor(ISparqlDataset data)
            : this(data, ExplanationLevel.Default) { }

        public ExplainUpdateProcessor(ISparqlDataset data, ExplanationLevel level)
            : base(data)
        {
            this._level = level;
        }

        public ExplainUpdateProcessor(IInMemoryQueryableStore store, ExplanationLevel level)
            : this(new InMemoryDataset(store), level) { }

        public ExplainUpdateProcessor(IInMemoryQueryableStore store)
            : this(store, ExplanationLevel.Default) { }

        protected override ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> GetQueryProcessor()
        {
            return new ExplainQueryProcessor(this._dataset, this._level);
        }
    }
}
