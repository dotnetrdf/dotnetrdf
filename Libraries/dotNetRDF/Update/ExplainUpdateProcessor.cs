/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

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
            _level = level;
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
            return new ExplainQueryProcessor(_dataset, _level);
        }
    }
}
