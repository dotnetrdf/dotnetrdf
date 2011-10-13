using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.LinkedData.Kasabi.Apis
{
    public class LookupApi
        : KasabiApi
    {
        public LookupApi(String datasetID, String authKey)
            : base(datasetID, "lookup", authKey) { }

        public IGraph GetDescription(Uri u)
        {
            Dictionary<String, String> apiParams = new Dictionary<string, string>();
            apiParams.Add("about", u.ToString());
            return this.GetGraphResponse(apiParams);
        }

        public void GetDescription(Uri u, IRdfHandler handler)
        {
            Dictionary<String, String> apiParams = new Dictionary<string, string>();
            apiParams.Add("about", u.ToString());
            this.GetGraphResponse(apiParams, handler);
        }
    }
}
