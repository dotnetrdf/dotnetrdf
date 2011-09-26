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
    }
}
