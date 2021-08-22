using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing;

namespace VDS.RDF
{
    public static class SparqlMimeTypeExtensions
    {
        /// <summary>
        /// Default File Extension for SPARQL Queries.
        /// </summary>
        public const string DefaultSparqlQueryExtension = "rq";
        /// <summary>
        /// Default File Extension for SPARQL Updates.
        /// </summary>
        public const string DefaultSparqlUpdateExtension = "ru";
        /// <summary>
        /// Default File Extension for SPARQL XML Results Format.
        /// </summary>
        public const string DefaultSparqlXmlExtension = "srx";
        /// <summary>
        /// Default File Extension for SPARQL JSON Results Format.
        /// </summary>
        public const string DefaultSparqlJsonExtension = "srj";

        /// <summary>
        /// MIME Type for SPARQL Queries.
        /// </summary>
        public const string SparqlQuery = "application/sparql-query";

        /// <summary>
        /// MIME Type for SPARQL Updates.
        /// </summary>
        public const string SparqlUpdate = "application/sparql-update";

        /// <summary>
        /// MIME Types for SPARQL Result Sets.
        /// </summary>
        internal static string[] SparqlResults = { "application/sparql-results+xml", "application/sparql-results+json" };

        /// <summary>
        /// MIME Types for SPARQL Results XML.
        /// </summary>
        public static string[] SparqlResultsXml = { "application/sparql-results+xml" };

        /// <summary>
        /// MIME Types for SPARQL Results JSON.
        /// </summary>
        internal static string[] SparqlResultsJson = { "application/sparql-results+json" };

        /// <summary>
        /// MIME Types for SPARQL Boolean Result.
        /// </summary>
        internal static string[] SparqlResultsBoolean = { "text/boolean" };


        public static void RegisterSparqlMimeTypes()
        {
            var mimeTypes = new List<MimeTypeDefinition>();

            // Define SPARQL Query
            var qDef = new MimeTypeDefinition("SPARQL Query", new string[] { SparqlQuery }, new string[] { DefaultSparqlQueryExtension });
            qDef.SetObjectParserType<SparqlQuery>(typeof(SparqlQueryParser));
            mimeTypes.Add(qDef);

            // Define SPARQL Update
            var uDef = new MimeTypeDefinition("SPARQL Update", new string[] { SparqlUpdate }, new string[] { DefaultSparqlUpdateExtension });
            uDef.SetObjectParserType<SparqlUpdateCommandSet>(typeof(SparqlUpdateParser));
            mimeTypes.Add(uDef);

            MimeTypesHelper.Register(mimeTypes);
        }
    }
}
