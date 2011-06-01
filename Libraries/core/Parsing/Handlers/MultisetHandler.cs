using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A SPARQL Results Handler which loads directly into a <see cref="Multiset">Multiset</see>
    /// </summary>
    public class MultisetHandler : BaseResultsHandler
    {
        private Multiset _mset;

        /// <summary>
        /// Creates a new Multiset Handler
        /// </summary>
        /// <param name="mset">Multiset</param>
        public MultisetHandler(Multiset mset)
        {
            if (mset == null) throw new ArgumentNullException("mset", "Multiset to load into cannot be null");
            this._mset = mset;
        }

        protected override void HandleBooleanResultInternal(bool result)
        {
            //Does Nothing
        }

        protected override bool HandleVariableInternal(string var)
        {
            this._mset.AddVariable(var);
            return true;
        }

        protected override bool HandleResultInternal(SparqlResult result)
        {
            this._mset.Add(new Set(result));
            return true;
        }
    }
}
