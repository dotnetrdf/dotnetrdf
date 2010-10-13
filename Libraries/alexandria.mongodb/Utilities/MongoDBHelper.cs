using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;
using MongoDB.Configuration;
using Newtonsoft.Json.Linq;
using VDS.Alexandria.Documents;
using VDS.Alexandria.Indexing;

namespace VDS.Alexandria.Utilities
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

        public static MongoConfiguration GetConfiguration()
        {
            return new MongoConfiguration();
        }

        public static MongoConfiguration GetConfiguration(String connectionString)
        {
            MongoConfiguration config = new MongoConfiguration();
            config.ConnectionString = connectionString;
            return config;
        }

        public static IIndexManager GetIndexManager(MongoDBDocumentManager manager)
        {
            switch (manager.Schema)
            {
                case MongoDBSchemas.GraphCentric:
                    return new MongoDBGraphCentricIndexManager(manager);
                case MongoDBSchemas.TripleCentric:
                    return new MongoDBTripleCentricIndexManager(manager);
                default:
                    throw new NotSupportedException("Unknown MongoDB Schema not supported");
            }
        }

        public static Object[] JsonArrayToObjects(String json)
        {
            JArray array = JArray.Parse(json);
            return JsonArrayToObjects(array);
        }

        private static Object[] JsonArrayToObjects(JArray array)
        {
            List<Object> objects = new List<Object>();

            foreach (JToken arrayItem in array)
            {
                if (arrayItem.Type == JTokenType.Object)
                {
                    objects.Add(JsonObjectToDocument((JObject)arrayItem));
                }
                else if (arrayItem.Type == JTokenType.Array)
                {
                    objects.Add(JsonArrayToObjects((JArray)arrayItem));
                }
                else
                {
                    switch (arrayItem.Type)
                    {
                        case JTokenType.Boolean:
                            objects.Add((bool)arrayItem);
                            break;

                        case JTokenType.Date:
                            objects.Add((DateTime)arrayItem);
                            break;
                        
                        case JTokenType.Float:
                            objects.Add((float)arrayItem);
                            break;

                        case JTokenType.Integer:
                            objects.Add((int)arrayItem);
                            break;

                        case JTokenType.Null:
                            objects.Add(null);
                            break;

                        case JTokenType.String:
                            objects.Add((String)arrayItem);
                            break;

                        default:
                            throw new AlexandriaException("Unable to convert JToken of Type " + arrayItem.Type.ToString() + " to a value in a MongoDB Document");
                    }
                }
            }

            return objects.ToArray();
        }

        public static Document JsonObjectToDocument(String json)
        {
            JObject obj = JObject.Parse(json);
            return JsonObjectToDocument(obj);
        }

        private static Document JsonObjectToDocument(JObject obj)
        {
            Document doc = new Document();
            foreach (JProperty property in obj.Properties())
            {
                switch (property.Value.Type)
                {
                    case JTokenType.Array:
                        doc[property.Name] = JsonArrayToObjects((JArray)property.Value);
                        break;

                    case JTokenType.Object:
                        doc[property.Name] = JsonObjectToDocument(property.Value.ToString());
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

        public static String DocumentListToJsonArray(Object obj)
        {
            if (obj == null) return String.Empty;
            if (obj is List<Document>)
            {
                StringBuilder json = new StringBuilder();
                json.AppendLine("[");
                List<Document> docs = (List<Document>)obj;
                for (int i = 0; i < docs.Count; i++)
                {
                    json.Append(docs[i].ToString());
                    if (i < docs.Count - 1)
                    {
                        json.AppendLine(",");
                    }
                    else
                    {
                        json.AppendLine();
                    }
                }
                json.AppendLine("]");
                return json.ToString();
            }
            else if (obj is List<Object>)
            {
                List<Object> objList = (List<Object>)obj;
                if (objList.Count == 0)
                {
                    return "[]";
                }
                else
                {
                    return DocumentListToJsonArray(objList.Select(o => (Document)o));
                }
            }
            else
            {
                throw new InvalidCastException("Cannot convert an Object which is not a List<Document> to a JSON Array");
            }
        }

        public static List<Object> DocumentListToObjectList(Object obj)
        {
            if (obj is List<Object>)
            {
                return (List<Object>)obj;
            }
            else if (obj is List<Document>)
            {
                return ((List<Document>)obj).Select(o => (Object)o).ToList();
            }
            else
            {
                throw new InvalidCastException("Cannot convert an Object which is not a List<Document> to a List<Object>");
            }
        }
    }
}
