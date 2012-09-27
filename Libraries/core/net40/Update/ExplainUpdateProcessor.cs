/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

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
