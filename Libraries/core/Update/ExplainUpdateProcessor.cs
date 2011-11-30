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

        /// <summary>
        /// Creates a new Explain Update Processor
        /// </summary>
        /// <param name="data">Dataset</param>
        public ExplainUpdateProcessor(ISparqlDataset data)
            : this(data, ExplanationLevel.Default) { }

        /// <summary>
        /// Creates a new Explain Update Processor
        /// </summary>
        /// <param name="data">Dataset</param>
        /// <param name="level">Explanation Level</param>
        public ExplainUpdateProcessor(ISparqlDataset data, ExplanationLevel level)
            : base(data)
        {
            this._level = level;
        }

        /// <summary>
        /// Creates a new Explain Update Processor
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="level">Explanation Level</param>
        public ExplainUpdateProcessor(IInMemoryQueryableStore store, ExplanationLevel level)
            : this(new InMemoryDataset(store), level) { }

        /// <summary>
        /// Creates a new Explain Update Processor
        /// </summary>
        /// <param name="store">Triple Store</param>
        public ExplainUpdateProcessor(IInMemoryQueryableStore store)
            : this(store, ExplanationLevel.Default) { }

        /// <summary>
        /// Gets the Query Processor to be used
        /// </summary>
        /// <returns></returns>
        protected override ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> GetQueryProcessor()
        {
            return new ExplainQueryProcessor(this._dataset, this._level);
        }
    }
}
