using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.LinkedData.Kasabi.Apis;

namespace VDS.RDF.LinkedData.Kasabi
{
    public static class KasabiClient
    {
        public static String AuthenticationKey
        {
            internal get;
            set;
        }

        public static Dictionary<String, String> EmptyParams
        {
            get
            {
                return new Dictionary<String, String>();
            }
        }

        public static AttributionApi GetAttributionApi(String datasetID)
        {
            return new AttributionApi(datasetID);
        }

        public static SparqlApi GetSparqlApi(String datasetID)
        {
            return new SparqlApi(datasetID, KasabiClient.AuthenticationKey);
        }

        public static LookupApi GetLookupApi(String datasetID)
        {
            return new LookupApi(datasetID, KasabiClient.AuthenticationKey);
        }
    }
}
