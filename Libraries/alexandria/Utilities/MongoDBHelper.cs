using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;
using MongoDB.Configuration;
using Newtonsoft.Json.Linq;

namespace Alexandria.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// <para>
    /// Parts of the Helper Code here based upon <a href="http://daniel.wertheim.se/2010/02/05/getting-started-with-mongodb-using-json-net-and-castle-dynamic-proxy/">MongoDB using Json.Net</a> by Daniel Wertheim
    /// </para>
    /// </remarks>
    public static class MongoDBHelper
    {
        public static MongoConfiguration GetConfiguration(String connectionString)
        {
            MongoConfiguration config = new MongoConfiguration();
            config.ConnectionString = connectionString;
            return config;
        }

        public static Document JsonToDocument(String json)
        {
            JObject obj = JObject.Parse(json);
            Document doc = new Document();
            foreach (JProperty property in obj.Properties())
            {
                switch (property.Value.Type)
                {
                    case JTokenType.Array:
                        JArray array = (JArray)property.Value;
                        List<Document> items = new List<Document>();
                        foreach (JToken arrayItem in array)
                        {
                            items.Add(JsonToDocument(arrayItem.ToString()));
                        }
                        doc[property.Name] = items.ToArray();
                        break;

                    case JTokenType.Object:
                        doc[property.Name] = JsonToDocument(property.Value.ToString());
                        break;

                    case JTokenType.Boolean:
                        doc[property.Name] = (bool)property.Value;
                        break;

                    case JTokenType.Date:
                        doc[property.Name] = (DateTime)property.Value;
                        break;
                    
                    case JTokenType.Float:
                        doc[property.Name] = (float)property.Value;
                        break;

                    case JTokenType.Integer:
                        doc[property.Name] = (int)property.Value;
                        break;

                    case JTokenType.Null:
                        doc[property.Name] = null;
                        break;

                    case JTokenType.String:
                        doc[property.Name] = (String)property.Value;
                        break;

                    default:
                        throw new AlexandriaException("Unable to convert JToken of Type " + property.Type.ToString() + " to a value in a MongoDB Document");
                }
            }

            return doc;
        }

        public static String DocumentToJson(Document doc)
        {
            return doc.ToString();
        }
    }
}
