using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.LinkedData.Kasabi.Apis
{
    public class AttributionApi
        : KasabiApi
    {
        public AttributionApi(String datasetID)
            : base(new Uri(KasabiApi.KasabiBaseApiUri + datasetID + "/attribution"), datasetID, "attribution", null, false) { }

        public String GetJavascript()
        {
            return this.GetStringResponse(KasabiClient.EmptyParams);
        }

        public String GetRawJson()
        {
            Dictionary<String, String> apiParams = new Dictionary<string,string>();
            apiParams.Add(KasabiParamOutputFormat, "json");
            return this.GetStringResponse(apiParams);
        }

        public JToken GetJson()
        {
            Dictionary<String, String> apiParams = new Dictionary<string,string>();
            apiParams.Add(KasabiParamOutputFormat, "json");
            return this.GetJsonResponse(apiParams);
        }
    }
}
