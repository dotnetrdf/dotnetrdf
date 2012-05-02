using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    public class UUIDFunction
        : BaseUUIDFunction
    {
        protected override IValuedNode EvaluateInternal(Guid uuid)
        {
            return new UriNode(null, new Uri("urn:uuid:" + uuid.ToString()));
        }

        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordUUID;
            }
        }
    }

    public class StrUUIDFunction
        : BaseUUIDFunction
    {

        protected override IValuedNode EvaluateInternal(Guid uuid)
        {
            return new StringNode(null, uuid.ToString());
        }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordStrUUID;
            }
        }
    }
}
